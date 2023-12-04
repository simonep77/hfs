using Hfs.Server.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler con funzioni base
    /// </summary>
    internal abstract class FileHandlerBase: IFileHandler
    {
        private HfsResponseVfs mVfsResp;

        protected HfsResponseVfs VfsResp 
        {
            get {
                return this.mVfsResp;
            } 
        }

        #region IFileHandler Membri di
 
        public FileHandlerBase(HfsResponseVfs resp)
        {
            this.mVfsResp = resp;
        }

        public abstract string FullName  { get; }

        public abstract string Name { get; }

        public abstract string Extension { get; }

        public abstract bool Exist();

        public abstract void Delete();

        public abstract string GetInfoXml();

        public abstract string GetSHA1();

        public abstract void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody);

        public abstract Stream OpenRead();

        public abstract Stream OpenWrite(bool overwrite);

        public abstract void SetAttribute(FileAttributes attr);

        public async Task MoveTo(IFileHandler file)
        {
            await this.CopyTo(file);
            this.Delete();
        }

        public async Task CopyTo(IFileHandler file)
        {
            if (this.FullName == file.FullName)
                throw new HfsException(EStatusCode.PathAreSame, "Il path sorgente non puo' coincidere con quello di destinazione");

            using (Stream sr = this.OpenRead())
            {
                using (Stream sw = file.OpenWrite(true))
                {
                    byte[] buff = new byte[Const.BUFFER_LEN];
                    int iRead;

                    while ((iRead = await sr.ReadAsync(buff, 0, buff.Length)) > 0)
                    {
                        await sw.WriteAsync(buff, 0, iRead);
                    }
                }
            }
        }

        public virtual void Dispose()
        { }

        #endregion


        protected string createTempFileName()
        {
            return Path.Combine(HfsData.TempDirRemoteFiles, string.Concat(Guid.NewGuid().ToString("N"), ".tmp"));
        }

    }
}