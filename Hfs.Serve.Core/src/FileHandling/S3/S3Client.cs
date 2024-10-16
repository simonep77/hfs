using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;



namespace Hfs.Server.Core.FileHandling
{
    public class S3Client: IDisposable
    {
        private string currentDir = @"/";
        private string bucketName;

        public IMinioClient Client { get; private set; }


        /// <summary>
        /// Directory corrente
        /// </summary>
        public string CurrenDir
        {
            get
            {
                return this.currentDir;
            }
        }


        /* Construct Object */
        public S3Client(string url, string accessKey, string secretKey, string bucketName, string currentDir = null) 
        {
            this.Client = new MinioClient()
                                    .WithEndpoint(url)
                                    .WithCredentials(accessKey, secretKey)
                                    .WithSSL(true)
                                    .Build();

            this.currentDir = currentDir;
            this.bucketName = bucketName;
        }

        public async Task<bool> Exist(string remoteFile)
        {
            var args = new GetObjectArgs().WithBucket(this.bucketName).WithObject(remoteFile);
            try
            {
                var stats = await this.Client.GetObjectAsync(args);
                return true;
            }
            catch (ObjectNotFoundException)
            {
                return false;
            }
        }


        /* Download File */
        public async Task Download(string remoteFile, string localFile)
        {
            var args = new GetObjectArgs().WithBucket(this.bucketName).WithObject(remoteFile).WithFile(localFile);
            await this.Client.GetObjectAsync(args);
        }

        public async Task DownloadStream(string remoteFile, Stream stream)
        {
            var args = new GetObjectArgs().WithBucket(this.bucketName).WithObject(remoteFile).WithCallbackStream(s =>
            {
                s.CopyToAsync(stream);
            });
            await this.Client.GetObjectAsync(args);
        }

        public async Task UploadStream(string remoteFile, Stream stream)
        {
            var args = new PutObjectArgs().WithBucket(this.bucketName).WithObject(remoteFile).WithStreamData(stream);
            await this.Client.PutObjectAsync(args);
        }


        /* Upload File */


        /* Delete File */
        public async Task Delete(string remoteFile)
        {
            var args = new RemoveObjectArgs().WithBucket(this.bucketName).WithObject(remoteFile);
            await this.Client.RemoveObjectAsync(args);
        }

        /* Rename File */
        public async Task Rename(string oldRemoteFile, string newRemoteFile)
        {
            var argscps = new CopySourceObjectArgs().WithBucket(this.bucketName).WithObject(oldRemoteFile);
            var argscpd = new CopyObjectArgs().WithBucket(this.bucketName).WithObject(newRemoteFile).WithCopyObjectSource(argscps);
            await this.Client.CopyObjectAsync(argscpd);
            await this.Delete(oldRemoteFile);
        }

        /* Create a New Directory on the FTP Server */
        public void CreateDirectory(string newDirectory)
        {
            //Non fa nulla
        }

        /* Get the Date/Time a File was Created */
        public async Task<ObjectStat> GetFileInfo(string fileName)
        {
            var args = new GetObjectArgs().WithBucket(this.bucketName).WithObject(fileName);
            return await this.Client.GetObjectAsync(args);
        }



        /* List Directory Contents File/Folder Name Only */
        public IEnumerable<Minio.DataModel.Item> DirectoryListSimple(string directory)
        {
            var args = new ListObjectsArgs().WithBucket(this.bucketName).WithPrefix(directory);

            return this.Client.ListObjectsEnumAsync(args).ToBlockingEnumerable();
        }

        /* List Directory Contents in Detail (Name, Size, Created, etc.) */
        public string[] DirectoryListDetailed(string directory)
        {
            try
            {
                return new string[] { };
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { };
        }

        public void Dispose()
        {
            this.Client?.Dispose();
        }
    }
}