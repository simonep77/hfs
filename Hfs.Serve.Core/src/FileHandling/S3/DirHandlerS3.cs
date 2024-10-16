using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using Hfs.Server.Core.Common;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler per directory su hfs remoti
    /// </summary>
    internal class DirHandlerS3 : DirHandlerBase
    {
        private HfsResponseVfs mVfsResp;
        private S3Client mClient;
        private string mNormalizedPath;
        public DirHandlerS3(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;

            this.mClient = new S3Client(resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_ENDPOINT],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_ACCESS_KEY],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_SECRET_KEY],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_BUCKET_NAME],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_CURRDIR]);

            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mClient.CurrenDir, this.mVfsResp.VirtualPath.Replace(this.mVfsResp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));

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

        }

        public override bool Exist()
        {
            return true;
        }

        public override void Delete()
        {
            //Elimina file
            foreach (var item in this.GetFiles())
            {
                item.Delete();
            }
        }

        public override long GetSize()
        {
            return 0;
        }

        public override string GetFilesXml(string pattern, bool recursive)
        {
            var files = this.mClient.DirectoryListSimple(this.mNormalizedPath).Where(x => !x.IsDir);

            Regex? rgx = (pattern != "*") ? Utility.GetRegexFromPattern(pattern) : null;

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    var nome = Path.GetFileName(item.Key);

                    if (!(!rgx?.IsMatch(nome) ?? true))
                        continue;

                    Utility.GetFileInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, nome),
                        this.mVfsResp.Access.ToString(),
                        Convert.ToInt32(item.Size),
                        item.LastModifiedDateTime.GetValueOrDefault(),
                        item.LastModifiedDateTime.GetValueOrDefault(),
                        item.LastModifiedDateTime.GetValueOrDefault(),
                        FileAttributes.Normal);
                }

                return xw.ToString();
            }

        }



        public override string GetDirsXml(string pattern, bool recursive)
        {
            var files = this.mClient.DirectoryListSimple(this.mNormalizedPath).Where(x => x.IsDir);

            Regex? rgx = (pattern != "*") ? Utility.GetRegexFromPattern(pattern) : null;

            using (var xw = new XmlWrite())
            {
                foreach (var item in files)
                {
                    var nome = Path.GetFileName(item.Key);

                    if (!(!rgx?.IsMatch(nome) ?? true))
                        continue;

                    Utility.GetDirectoryInfoXml(xw,
                        Utility.HfsCombine(this.mVfsResp.VirtualPath, nome),
                        item.LastModifiedDateTime.GetValueOrDefault(),
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
            return new FileHandlerS3(resp, this.mClient);
        }

        public override IDirHandler GetDirHandler(string name)
        {
            HfsResponseVfs resp = this.mVfsResp.Clone();
            resp.VirtualPath = Utility.HfsCombine(resp.VirtualPath, name);
            resp.PhysicalPath = resp.VirtualPath;
            return new DirHandlerS3(resp);
        }


        public override IFileHandler[] GetFiles()
        {
            return this.GetFiles("*");
        }


        public override IFileHandler[] GetFiles(string pattern)
        {
            var files = this.mClient.DirectoryListSimple(this.mNormalizedPath);
            List<IFileHandler> ret = new List<IFileHandler>();

            Regex? rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);


            foreach (var item in files)
            {
                var nome = Path.GetFileName(item.Key);
                if (!(rgx?.IsMatch(nome) ?? true))
                    continue;

                ret.Add(this.GetFileHandler(nome));
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
            var files = this.mClient.DirectoryListSimple(this.mNormalizedPath).Where(x => x.IsDir);

            Regex? rgx = null;
            //Verifica pattern
            if (pattern != "*")
                rgx = new Regex(Regex.Escape(pattern).Replace(@"\*", @".*"), RegexOptions.IgnoreCase);

            foreach (var item in files)
            {
                var nome = Path.GetFileName(item.Key);
                if (!(rgx?.IsMatch(nome) ?? true))
                    continue;

                ret.Add(this.GetDirHandler(nome));
            }

            return ret.ToArray();
        }

        public override void Dispose()
        {
            this.mClient?.Dispose();
        }

    }
}