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
    internal class FileHandlerSFtp: FileHandlerBase
    {
        private SFtpClient mClient;
        private bool mIsOwnClient = false;
        private string mNormalizedPath;

        public FileHandlerSFtp(HfsResponseVfs resp)
            :base(resp)
        {
            var cli = new SFtpClient(resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_HOST],
                System.Convert.ToInt32(resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_PORT])
                , resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_USER],
                resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_PASS],
                resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_CURRDIR],
                resp.Path.Params[Const.SFTP_File_Handling.PATH_PARAM_KEYBASE64]);

            this.mIsOwnClient = true;
            this.initClient(resp, cli);

        }

        public FileHandlerSFtp(HfsResponseVfs resp, SFtpClient cli)
            :base(resp)
        {
            this.initClient(resp, cli);
        }

        private void initClient(HfsResponseVfs resp, SFtpClient cli)
        {
            this.mClient = cli;

            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mClient.CurrenDir, resp.VirtualPath.Replace(resp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));


            this.mClient.Open();
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
            try
            {
                var rFile = this.mClient.Client.GetAttributes(this.mNormalizedPath);
                return rFile.IsRegularFile;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Delete()
        {
            if (this.Exist())
                this.mClient.Delete(this.mNormalizedPath);
        }

        public override void SetAttribute(FileAttributes attr)
        {
            throw new NotImplementedException();
        }

        public override string GetInfoXml()
        {
            var attr = this.mClient.GetFileInfo(this.mNormalizedPath);

            using (var xw = new XmlWrite())
            {
                Utility.GetFileInfoXml(xw,
                    Utility.HfsCombine(this.VfsResp.Path.Virtual, this.VfsResp.VirtualPath),
                    this.VfsResp.Access.ToString(),
                    attr.Size,
                    attr.LastWriteTime,
                    attr.LastWriteTime,
                    attr.LastAccessTime,
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
            this.mClient.Download(this.mNormalizedPath, sTempFile);
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
            return this.mClient.Client.OpenRead(this.mNormalizedPath);
        }

        public override Stream OpenWrite(bool overwrite)
        {
            var stream = this.mClient.Client.OpenWrite(this.mNormalizedPath);
            stream.Timeout = TimeSpan.FromMinutes(10D);
            //stream.WriteTimeout = (10 * 60 * 1000);
            return stream;
        }

        public override void Dispose()
        {
            base.Dispose();

            try
            {
                if (this.mIsOwnClient)
                    this.mClient.Close();
            }
            catch (Exception ex)
            {
                HfsData.WriteException(ex);
            }


        }

    }
}