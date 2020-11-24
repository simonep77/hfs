using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Hfs.Client;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// File handler per file su hfs remoti
    /// </summary>
    internal class FileHandlerHfs: FileHandlerBase
    {
        private HfsClient mClient;

        public FileHandlerHfs(HfsResponseVfs resp, Uri uri)
            : this(resp, new HfsClient(uri.AbsoluteUri))
        {  }

        public FileHandlerHfs(HfsResponseVfs resp, HfsClient cli)
            :base(resp)
        {
            this.mClient = cli;
        }

        public override string FullName
        {
            get { return this.VfsResp.PhysicalPath; }
        }

        public override string Name
        {
            get { return Path.GetFileName(this.VfsResp.PhysicalPath); }
        }

        public override string Extension
        {
            get { return Path.GetExtension(this.VfsResp.PhysicalPath); }
        }

        public override bool Exist()
        {
            return this.mClient.FileExist(this.VfsResp.VirtualPath);
        }

        public override void Delete()
        {
            this.mClient.FileDelete(this.VfsResp.VirtualPath);
        }

        public override void SetAttribute(FileAttributes attr)
        {
            throw new NotImplementedException();
        }

        public override string GetInfoXml()
        {
            Regex rgx = new Regex(@"(<name>.*</name>)", System.Text.RegularExpressions.RegexOptions.Compiled);
            return rgx.Replace(this.mClient.FileGetInfo(this.VfsResp.VirtualPath), string.Concat("<name>", Utility.HfsCombine(this.VfsResp.Path.Virtual, this.VfsResp.VirtualPath), "</name>"));
        }

        public override string GetSHA1()
        {
            return (string)this.mClient.FileHashSHA1(this.VfsResp.VirtualPath);
        }

        public override void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody)
        {
            this.mClient.FileEmail(this.VfsResp.VirtualPath, mailTo, mailFrom, mailSubj, mailBody);
        }

        public override bool CanConvert()
        {
            return false;
        }

        public override void Convert(bool doAsync, bool refreshCache)
        {
            throw new NotImplementedException();
        }

        public override Stream OpenRead()
        {
            string sTempFile = this.createTempFileName();
            this.mClient.FileReadToDisk(this.VfsResp.VirtualPath, sTempFile);
            return new FileReadStream(sTempFile);
        }

        public override Stream OpenWrite(bool overwrite)
        {
            return new HfsWriteStream(this.mClient, this.VfsResp.VirtualPath, this.createTempFileName(), overwrite);
        }

    }
}