using System;
using System.Collections.Generic;
using System.IO;
using System.Net;



namespace Hfs.Server.Core.FileHandling
{
    public class SFtpClient
    {
        private string currentDir = @"/";
        private string host;
        private int port = 22;
        private string user;
        private string pass;
        private string privateKeyBase64;

        private Renci.SshNet.SftpClient mClient;

        public Renci.SshNet.SftpClient Client {
            get {
                return this.mClient;
            }
        }


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
        public SFtpClient(string hostIP, int port, string userName, string password, string currentDir, string privateKeyBase64) 
        {
            this.host = hostIP;
            this.port = port;
            this.user = userName;
            this.pass = password;
            this.privateKeyBase64 = privateKeyBase64;
            this.currentDir = currentDir ?? @"/";

            if (string.IsNullOrEmpty(privateKeyBase64))
            {
                this.mClient = new Renci.SshNet.SftpClient(this.host,
                                                            this.port,
                                                            this.user,
                                                            this.pass);
            }
            else
            {
                var pkf = new Renci.SshNet.PrivateKeyFile(new MemoryStream(Convert.FromBase64String(privateKeyBase64)), this.pass);
                this.mClient = new Renci.SshNet.SftpClient(this.host,
                                                            this.port,
                                                            this.user,
                                                            new Renci.SshNet.PrivateKeyFile[] { pkf });
            }

            this.mClient.ConnectionInfo.Timeout = TimeSpan.FromMinutes(5d);
            this.mClient.OperationTimeout = TimeSpan.FromMinutes(10d);

        }


        public void Close()
        {
            if (this.mClient.IsConnected)
                this.mClient.Disconnect();
        }

        public void Open()
        {
            if (!this.mClient.IsConnected)
                this.mClient.Connect();

            this.mClient.ChangeDirectory(this.currentDir);
        }


        /* Download File */
        public void Download(string remoteFile, string localFile)
        {
            using (var fs = File.Create(localFile))
            {
                this.DownloadStream(remoteFile, fs);
            }
        }

        public void DownloadStream(string remoteFile, Stream stream)
        {
            this.mClient.DownloadFile(remoteFile, stream);
        }


        /* Upload File */


        /* Delete File */
        public void Delete(string remoteFile)
        {
            this.mClient.DeleteFile(remoteFile);
        }

        /* Rename File */
        public void Rename(string oldRemoteFile, string newRemoteFile)
        {
            this.mClient.RenameFile(oldRemoteFile, newRemoteFile);
        }

        /* Create a New Directory on the FTP Server */
        public void CreateDirectory(string newDirectory)
        {
            this.mClient.CreateDirectory(newDirectory);
        }

        /* Get the Date/Time a File was Created */
        public Renci.SshNet.Sftp.SftpFileAttributes GetFileInfo(string fileName)
        {
            return this.mClient.GetAttributes(fileName);
        }



        /* List Directory Contents File/Folder Name Only */
        public string[] GirectoryListSimple(string directory)
        {
            try
            {
                return new string[] { };
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            /* Return an Empty string Array if an Exception Occurs */
            return new string[] { };
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
    }
}