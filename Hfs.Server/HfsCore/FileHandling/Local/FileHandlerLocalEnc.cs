using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.UTILITY;
using Hfs.Server.HfsCore.FileHandling.Local;
using System.Net;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// File handler per file locali
    /// </summary>
    internal class FileHandlerLocalEnc: FileHandlerBase
    {
        private FileInfo mFileInfo;
        private NetworkConnection mNetConn;
        public FileHandlerLocalEnc(HfsResponseVfs resp)
            :base(resp)
        {
            //Se trattasi di una share SMB e sono presenti credenziali specifiche 
            if (resp.Path.IsUncWithCredentials)
            {
                this.mNetConn = new NetworkConnection(resp.PhysicalPath, 
                    new NetworkCredential(resp.Path.Params[Const.SMB_File_Handling.PATH_PARAM_USER], resp.Path.Params[Const.SMB_File_Handling.PATH_PARAM_PASS]));
                
            }
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

        /// <summary>
        /// Prepara il file temporaneo convertito
        /// </summary>
        public override void Convert(bool doAsync, bool refreshCache)
        {
            //Tipo convertibile
            if (!this.CanConvert())
                return;

            //Converte
            FileInfo fSource = this.mFileInfo;
            this.mFileInfo = new FileInfo(Utility.GetConvertedFileName(fSource, this.VfsResp.VirtualPath, string.Concat(@".", this.VfsResp.FileConvertType)));

            FileConverterBase fc = FileConverterFactory.CreateConverter(this.VfsResp.FileConvertType, fSource, this.mFileInfo);

            if (!doAsync)
                fc.Convert(refreshCache);
            else
                fc.ConvertAsync(refreshCache);
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
                throw new HfsException(EStatusCode.FileNotFound, "File {0} non trovato", this.VfsResp.VirtualPath);
        }


        public override void Dispose()
        {
            base.Dispose();
            //Se presente una connessione esplicita SMB libera la connessione
            if (this.mNetConn != null)
                this.mNetConn.Dispose();
        }
    }
}