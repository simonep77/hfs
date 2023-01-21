using Hfs.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    internal class HfsWriteStream: FileStream
    {
        private HfsClient mClient;
        private string mVpath;
        private bool mOverwrite;

        public HfsWriteStream(HfsClient cli, string vpath, string tempFile, bool overwrite)
            :base(tempFile, FileMode.Create, FileAccess.Write)
        {
            this.mClient = cli;
            this.mVpath = vpath;
            this.mOverwrite = overwrite;
        }


        public override long Seek(long offset, SeekOrigin loc)
        {
            //Se richiesta la scrittura in un punto specifico del file allora lo deve leggere prima tutto
            if (this.Length == 0 && (!this.mOverwrite) && (offset > 0 || loc == SeekOrigin.End))
            {
                this.mClient.FileReadToStream(mVpath, this);
                this.Position = 0;
            }
            return base.Seek(offset, loc);
        }

        /// <summary>
        /// Invia dati e chiude
        /// </summary>
        public override void Close()
        {
            string sFile = this.Name;
            try
            {
                if (this.Length > 0)
                {
                    this.Flush();
                    this.Position = 0;
                    this.mClient.FileWriteFromStream(mVpath, this);
                }
                else
                {
                    this.mClient.FileTouch(mVpath);
                }
                base.Close();
            }
            finally
            {
                File.Delete(sFile);
            }
            
        }
    }
}