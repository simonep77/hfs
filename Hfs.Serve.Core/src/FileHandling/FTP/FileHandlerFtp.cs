using Hfs.Server.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler per file su hfs remoti
    /// </summary>
    internal class FileHandlerFtp: FileHandlerBase
    {
        private FluentFTP.FtpClient mClient;
        private bool mIsOwnClient = false;
        private string mCurrentDir;
        private string mNormalizedPath;

        public FileHandlerFtp(HfsResponseVfs resp)
            :base(resp)
        {


            this.mIsOwnClient = true;
            this.mCurrentDir = resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_CURRDIR];
            var bPassive = string.IsNullOrWhiteSpace(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASSIVE]) ? false : resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASSIVE] == "1";

            var fcli = new FluentFTP.FtpClient(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_USER],
                resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PASS],
                resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_HOST],
                System.Convert.ToInt32(resp.Path.Params[Const.FTP_File_Handling.PATH_PARAM_PORT]),
                new FluentFTP.FtpConfig { DataConnectionType = bPassive ? FluentFTP.FtpDataConnectionType.AutoPassive : FluentFTP.FtpDataConnectionType.AutoActive}
                );


            this.initClient(resp, fcli);

        }

        public FileHandlerFtp(HfsResponseVfs resp, FluentFTP.FtpClient cli)
            :base(resp)
        {
            this.initClient(resp, cli);
        }

        private void initClient(HfsResponseVfs resp, FluentFTP.FtpClient cli)
        {
            this.mClient = cli;

            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mCurrentDir, resp.VirtualPath.Replace(resp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));

            if (this.mIsOwnClient)
            {
                this.mClient.AutoConnect();
                if (!string.IsNullOrWhiteSpace(this.mCurrentDir))
                    this.mClient.SetWorkingDirectory(this.mCurrentDir);
            }
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
            return (this.mClient.FileExists(this.mNormalizedPath));
        }

        public override void Delete()
        {
            if (this.Exist())
                this.mClient.DeleteFile(this.mNormalizedPath);
        }

        public override void SetAttribute(FileAttributes attr)
        {
            throw new NotImplementedException();
        }

        public override string GetInfoXml()
        {
            var attr = this.mClient.GetObjectInfo(this.mNormalizedPath);
          
            if (attr == null)
                return string.Empty;
            
            System.Text.StringBuilder sbXml = new System.Text.StringBuilder(400);

            using (var xw = new XmlWrite())
            {
                Utility.GetFileInfoXml(xw,
                    Utility.HfsCombine(this.VfsResp.Path.Virtual, this.VfsResp.VirtualPath),
                    this.VfsResp.Access.ToString(),
                    attr.Size,
                    attr.Created,
                    attr.Modified,
                    attr.Modified,
                    FileAttributes.Normal);

                return xw.ToString();
            }


        }

        public override string GetSHA1()
        {
            throw new NotImplementedException();
        }

        public override void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody)
        {
            string sTempFile = this.createTempFileName();
            this.mClient.DownloadFile(sTempFile, this.mNormalizedPath);
            try 
	        {	        
                Utility.SendByMail(mailTo, mailFrom, mailSubj, mailBody, sTempFile);
	        }
	        finally
	        {
                File.Delete(sTempFile);
	        }
        }


        public override Stream OpenRead()
        {
            return this.mClient.OpenRead(this.mNormalizedPath);
        }

        public override Stream OpenWrite(bool overwrite)
        {
            if (overwrite)
                return this.mClient.OpenWrite(this.mNormalizedPath);
            else
                return this.mClient.OpenAppend(this.mNormalizedPath);
        }

        public override void Dispose()
        {
            base.Dispose();

            try
            {
                if (this.mIsOwnClient)
                    this.mClient.Dispose();
            }
            catch (Exception ex)
            {
                HfsData.WriteException(ex);
            }


        }

    }
}