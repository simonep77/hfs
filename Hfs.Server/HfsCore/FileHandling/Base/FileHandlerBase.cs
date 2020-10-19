using System;
using System.Collections.Generic;
using System.IO;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
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

        public virtual bool CanConvert()
        {
            //Non presente conversione
            if (string.IsNullOrEmpty(this.VfsResp.FileConvertType))
                return false;

            //File supportato
            if (!FileConverterFactory.IsSupportedExt(this.VfsResp.FileConvertType, this.Extension.ToLower()))
                return false;

            //Ok, convertibile
            return true;
        }

        public abstract void Delete();

        public abstract string GetInfoXml();

        public abstract string GetSHA1();

        public abstract void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody);

        public abstract Stream OpenRead();

        public abstract Stream OpenWrite(bool overwrite);

        public abstract void SetAttribute(FileAttributes attr);

        public void MoveTo(IFileHandler file)
        {
            this.CopyTo(file);
            this.Delete();
        }

        public abstract void Convert(bool doAsync, bool refreshCache);

        public void CopyTo(IFileHandler file)
        {
            if (this.FullName == file.FullName)
                throw new HfsException(EStatusCode.PathAreSame, "Il path sorgente non puo' coincidere con quello di destinazione");

            using (Stream sr = this.OpenRead())
            {
                using (Stream sw = file.OpenWrite(true))
                {
                    byte[] buff = new byte[Const.BUFFER_LEN];
                    int iRead;

                    while ((iRead = sr.Read(buff, 0, buff.Length)) > 0)
                    {
                        sw.Write(buff, 0, iRead);
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