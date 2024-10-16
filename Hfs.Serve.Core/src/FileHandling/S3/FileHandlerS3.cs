using Hfs.Server.Core.Common;
using Minio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler per file su hfs remoti
    /// </summary>
    internal class FileHandlerS3: FileHandlerBase
    {
        private S3Client mClient;
        private bool mIsOwnClient = false;
        private string mNormalizedPath;

        public FileHandlerS3(HfsResponseVfs resp)
            :base(resp)
        {
            var cli = new S3Client(resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_ENDPOINT],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_ACCESS_KEY],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_SECRET_KEY],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_BUCKET_NAME],
                resp.Path.Params[Const.S3_File_Handling.PATH_PARAM_CURRDIR]);

            this.mIsOwnClient = true;
            this.initClient(resp, cli);

        }

        public FileHandlerS3(HfsResponseVfs resp, S3Client cli)
            :base(resp)
        {
            this.initClient(resp, cli);
        }

        private void initClient(HfsResponseVfs resp, S3Client cli)
        {
            this.mClient = cli;

            //Imposta path normalizzato
            this.mNormalizedPath = string.Concat(Const.URI_SEPARATOR,
                Utility.NormalizeVirtualPath(string.Concat(this.mClient.CurrenDir, resp.VirtualPath.Replace(resp.Path.Virtual, ""))).TrimStart(Const.URI_SEPARATOR));
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
            return AsyncHelper.RunSync(() => this.mClient.Exist(this.mNormalizedPath));
        }

        public override void Delete()
        {
            if (this.Exist())
                AsyncHelper.RunSync(() => this.mClient.Delete(this.mNormalizedPath));
        }

        public override void SetAttribute(FileAttributes attr)
        {
            throw new NotImplementedException();
        }

        public override string GetInfoXml()
        {
            var attr = AsyncHelper.RunSync(() => this.mClient.GetFileInfo(this.mNormalizedPath));

            using (var xw = new XmlWrite())
            {
                Utility.GetFileInfoXml(xw,
                    Utility.HfsCombine(this.VfsResp.Path.Virtual, this.VfsResp.VirtualPath),
                    this.VfsResp.Access.ToString(),
                    attr.Size,
                    attr.LastModified,
                    attr.LastModified,
                    attr.LastModified,
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
            AsyncHelper.RunSync(() => this.mClient.Download(this.mNormalizedPath, sTempFile));
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
            var ms = new MemoryStream();
            AsyncHelper.RunSync(() => this.mClient.DownloadStream(this.mNormalizedPath, ms));
            ms.Position = 0;
            return ms;
        }

        public override Stream OpenWrite(bool overwrite)
        {
            return new S3WriteStream(this.mClient, this.VfsResp.VirtualPath, overwrite);
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