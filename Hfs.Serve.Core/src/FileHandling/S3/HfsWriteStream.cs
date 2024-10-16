using Hfs.Client;
using Hfs.Server.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.Core.FileHandling
{
    internal class S3WriteStream: MemoryStream
    {
        private S3Client mClient;
        private string mVpath;
        private bool mOverwrite;

        public S3WriteStream(S3Client cli, string vpath,  bool overwrite)
            :base()
        {
            this.mClient = cli;
            this.mVpath = vpath;
            this.mOverwrite = overwrite;

            //Se non fa overwrite si posiziona alla fine
            if (!overwrite)
                this.Seek(0, SeekOrigin.End);
        }


        public override long Seek(long offset, SeekOrigin loc)
        {
            //Se richiesta la scrittura in un punto specifico del file allora lo deve leggere prima tutto
            if (this.Length == 0 && (!this.mOverwrite) && (offset > 0 || loc == SeekOrigin.End))
            {
                AsyncHelper.RunSync(() => this.mClient.DownloadStream(mVpath, this));
                this.Position = 0;
            }
            return base.Seek(offset, loc);
        }

        /// <summary>
        /// Invia dati e chiude
        /// </summary>
        public override void Close()
        {
            this.Flush();
            this.Position = 0;
            AsyncHelper.RunSync(() => this.mClient.UploadStream(this.mVpath, this));
        }
    }
}