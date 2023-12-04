using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using FluentFTP;
using Hfs.Server.Core.Common;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler per directory su hfs remoti
    /// </summary>
    internal class DirHandlerFtp: DirHandlerBase
    {
        private HfsResponseVfs mVfsResp;
        private FluentFTP.FtpClient mClient;
        private string mNormalizedPath;
        private string mCurrentDir;

        public DirHandlerFtp(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;

            this.mCurrentDir = resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_CURRDIR];
            var bPassive = string.IsNullOrWhiteSpace(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASSIVE]) ? false : resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASSIVE] == "1";

            this.mClient = new FluentFTP.FtpClient(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_HOST],
                resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_USER],
                resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASS],
                System.Convert.ToInt32(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PORT]),
                new FtpConfig { DataConnectionType = bPassive ? FluentFTP.FtpDataConnectionType.PASV : FtpDataConnectionType.AutoActive }
                );


            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mCurrentDir, Const.URI_SEPARATOR, this.mVfsResp.VirtualPath.Replace(this.mVfsResp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));

            //Si collega
            //this.mClient.BulkListing = true;
            this.mClient.AutoConnect();
          
            //Eventalmente imposta la current directory
            if (!string.IsNullOrWhiteSpace(this.mCurrentDir))
                this.mClient.SetWorkingDirectory(this.mCurrentDir);


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
            this.mClient.CreateDirectory(this.mNormalizedPath);
        }

        public override bool Exist()
        {
            return this.mClient.DirectoryExists(this.mNormalizedPath);
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
            this.mClient.DeleteDirectory(this.mNormalizedPath);
        }

        public override long GetSize()
        {
            return 0;
        }


        /// <summary>
        /// Determina se item puo essere incluso in elenco
        /// </summary>
        /// <param name="item"></param>
        /// <param name="checkFile"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private bool isValidItem(FtpListItem item, bool checkFile, Regex filter)
        {
            if (checkFile)
            {
                if (item.Type != FtpObjectType.File)
                    return false;
            }
            else
            {
                if (item.Type != FtpObjectType.Directory)
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

        public override string GetFilesXml(string pattern, bool recursive)
        {
            var files = this.mClient.GetListing(this.mNormalizedPath);
            Regex rgx = null;

            //Verifica pattern
            if (pattern != "*")
                rgx = Utility.GetRegexFromPattern(pattern);

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    if (!this.isValidItem(item, true, rgx))
                        continue;

                    Utility.GetFileInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, item.Name),
                        this.mVfsResp.Access.ToString(),
                        item.Size,
                        item.Created,
                        item.Modified,
                        item.Modified,
                        FileAttributes.Normal);
                }

                return xw.ToString();
            }

        }

        public override string GetDirsXml(string pattern, bool recursive)
        {
            var files = this.mClient.GetListing(this.mNormalizedPath);

            Regex rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = Utility.GetRegexFromPattern(pattern);

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    if (!this.isValidItem(item, false, rgx))
                        continue;

                    Utility.GetDirectoryInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, item.Name),
                        item.Modified,
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
            return new FileHandlerFtp(resp, this.mClient);
        }

        public override IDirHandler GetDirHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new DirHandlerFtp(resp);
        }


        public override IFileHandler[] GetFiles()
        {
            return this.GetFiles("*");
        }


        public override IFileHandler[] GetFiles(string pattern)
        {
            var files = this.mClient.GetListing(this.mNormalizedPath);
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
            var files = this.mClient.GetListing(this.mNormalizedPath);

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
                this.mClient.Dispose();
            }
            catch (Exception ex)
            {
                HfsData.WriteException(ex);
            }
        }

    }
}