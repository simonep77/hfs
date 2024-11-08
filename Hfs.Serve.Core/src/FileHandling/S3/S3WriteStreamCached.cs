using Hfs.Client;
using Hfs.Server.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.Core.FileHandling
{
    internal class S3WriteStreamCached : FileStream
    {
        private Action<Stream> mOnComplete;

        public S3WriteStreamCached(string cachepath, FileMode mode, Action<Stream> onComplete)
            : base(cachepath, mode, FileAccess.ReadWrite, FileShare.ReadWrite)
        {
            if (onComplete is null)
                throw new ArgumentNullException("E' necessario fornire un'azione di completamento");
            
            this.mOnComplete = onComplete;
        }


        /// <summary>
        /// Invia dati e chiude
        /// </summary>
        public override void Close()
        {
            this.Flush();

            try
            {
                this.mOnComplete.Invoke(this);
            }
            finally
            {
                base.Close();
            }

        }
    }
}