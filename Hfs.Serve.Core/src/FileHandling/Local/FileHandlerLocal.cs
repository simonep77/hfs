using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Net;
using Hfs.Server.Core.Common;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler per file locali
    /// </summary>
    internal class FileHandlerLocal: FileHandlerBase
    {
        private FileInfo mFileInfo;
        public FileHandlerLocal(HfsResponseVfs resp)
            :base(resp)
        {
            this.mFileInfo = new FileInfo(resp.PhysicalPath);
        }

        public override string FullName
        {
            get { return this.mFileInfo.FullName; }
        }

        public override string Name
        {
            get { return this.mFileInfo.Name; }
        }

        public override string Extension
        {
            get { return this.mFileInfo.Extension; }
        }

        public override bool Exist()
        {
            return this.mFileInfo.Exists;
        }

        public override void Delete()
        {
            if (this.mFileInfo.Exists)
                this.mFileInfo.Delete();
        }

        public override void SetAttribute(FileAttributes attr)
        {
            this.checkExist();
            this.mFileInfo.Attributes = attr;
            this.mFileInfo.Refresh();
        }

        public override string GetInfoXml()
        {
            this.checkExist();
            using (var xw = new XmlWrite())
            {
                Utility.GetFileInfoXml(xw, this.VfsResp.VirtualPath, this.mFileInfo, this.VfsResp.Access.ToString());

                return xw.ToString();
            }


        }

        public override string GetSHA1()
        {
            this.checkExist();
            return Utility.CalculateSHA1(this.mFileInfo.FullName);
        }

        public override void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody)
        {
            this.checkExist();
            Utility.SendByMail(mailTo, mailFrom, mailSubj, mailBody, this.mFileInfo.FullName);
        }


        public override Stream OpenRead()
        {
            this.checkExist();

            return this.mFileInfo.OpenRead();
        }

        public override Stream OpenWrite(bool overwrite)
        {
            Directory.CreateDirectory(this.mFileInfo.DirectoryName);
            if (overwrite)
                return this.mFileInfo.Open(FileMode.Create);
            else
                return this.mFileInfo.OpenWrite();
        }


        protected void checkExist()
        {
            if (!this.mFileInfo.Exists)
                throw new HfsException(EStatusCode.FileNotFound, $"File {this.VfsResp.VirtualPath} non trovato");
        }


        public override void Dispose()
        {
            base.Dispose();
        }
    }
}