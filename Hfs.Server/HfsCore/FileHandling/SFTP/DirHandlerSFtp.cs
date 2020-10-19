using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.UTILITY;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// File handler per directory su hfs remoti
    /// </summary>
    internal class DirHandlerSFtp: DirHandlerBase
    {
        private HfsResponseVfs mVfsResp;
        private SFtpClient mClient;
        private string mNormalizedPath;
        public DirHandlerSFtp(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;

            this.mClient = new SFtpClient(resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_HOST],
                            System.Convert.ToInt32(resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_PORT])
                            , resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_USER],
                            resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_PASS],
                            resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_CURRDIR],
                            resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_KEYBASE64]);

            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mClient.CurrenDir, this.mVfsResp.VirtualPath.Replace(this.mVfsResp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));


            this.mClient.Open();

        }

        public override string FullName
        {
            get { return this.mVfsResp.VirtualPath; }
        }

        public override string Name
        {
            get { return Path.GetFileName(this.mVfsResp.VirtualPath); }
        }

        public override void Create()
        {
            this.mClient.Client.CreateDirectory(this.mNormalizedPath);
        }

        public override bool Exist()
        {
            try
            {
                var tDir = this.mClient.Client.GetAttributes(this.mNormalizedPath).IsDirectory;
                return true;
            }
            catch (Exception)
            {

                return false;
            }

            
        }

        public override void Delete()
        {
            //Elimina file
            foreach (var item in this.GetFiles())
            {
                item.Delete();
            }

            //Cartelle
            foreach (var item in this.GetDirs())
            {
                item.Delete();
            }

            //Se stessa
            this.mClient.Client.DeleteDirectory(this.mNormalizedPath);
        }

        public override long GetSize()
        {
            return 0;
        }

        public override string GetFilesXml(string pattern, bool recursive)
        {
            var files = this.mClient.Client.ListDirectory(this.mNormalizedPath);

            Regex rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    if (!this.isValidItem(item, true, rgx))
                        continue;

                    Utility.GetFileInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, item.Name),
                        this.mVfsResp.Access.ToString(),
                        item.Length,
                        item.LastWriteTime,
                        item.LastWriteTime,
                        item.LastAccessTime,
                        FileAttributes.Normal);
                }

                return xw.ToString();
            }
 
        }


        /// <summary>
        /// Determina se item puo essere incluso in elenco
        /// </summary>
        /// <param name="item"></param>
        /// <param name="checkFile"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private bool isValidItem(Renci.SshNet.Sftp.SftpFile item, bool checkFile, Regex filter)
        {
            if (checkFile)
            {
                if (item.IsDirectory)
                    return false;
            }
            else
            {
                if (!item.IsDirectory)
                    return false;
            }

            if (item.Name == @".")
                return false;

            if (item.Name == @"..")
                return false;

            //Verifica pattern
            if (filter != null && !filter.IsMatch(item.Name))
                return false;

            return true;
        }


        public override string GetDirsXml(string pattern, bool recursive)
        {
            var files = this.mClient.Client.ListDirectory(this.mNormalizedPath);

            Regex rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    if (!this.isValidItem(item, false, rgx))
                        continue;

                    Utility.GetDirectoryInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, item.Name),
                        item.LastWriteTime,
                        this.mVfsResp.Access.ToString());
                }

                return xw.ToString();
            }
        }

        public override IFileHandler GetFileHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new FileHandlerSFtp(resp, this.mClient);
        }

        public override IDirHandler GetDirHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new DirHandlerSFtp(resp);
        }


        public override IFileHandler[] GetFiles()
        {
            return this.GetFiles("*");
        }


        public override IFileHandler[] GetFiles(string pattern)
        {
            var files = this.mClient.Client.ListDirectory(this.mNormalizedPath);
            List<IFileHandler> ret = new List<IFileHandler>();

            Regex rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);


            foreach (var item in files)
            {
                if (!this.isValidItem(item, true, rgx))
                    continue;

                ret.Add(this.GetFileHandler(item.Name));
            }

            return ret.ToArray();
        }


        public override IDirHandler[] GetDirs()
        {
            return this.GetDirs("*");
        }

        public override IDirHandler[] GetDirs(string pattern)
        {
            List<IDirHandler> ret = new List<IDirHandler>();
            var files = this.mClient.Client.ListDirectory(this.mNormalizedPath);

            Regex rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);

            foreach (var item in files)
            {
                if (!this.isValidItem(item, false, rgx))
                    continue;

                ret.Add(this.GetDirHandler(item.Name));
            }

            return ret.ToArray();
        }

        public override void Dispose()
        {
            try
            {
                this.mClient.Close();
            }
            catch (Exception ex)
            {
                HfsData.Logger.WriteException(ELogType.HfsGlobal, ex);
            }
        }

    }
}