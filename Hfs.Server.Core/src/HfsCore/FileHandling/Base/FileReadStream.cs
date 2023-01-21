using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    internal class FileReadStream : FileStream
    {

        public FileReadStream(string tempFile)
            :base(tempFile, FileMode.Open, FileAccess.Read)
        {
        }


        /// <summary>
        /// Invia dati e chiude
        /// </summary>
        public override void Close()
        {
            string localfile = this.Name;
            try
            {
                base.Close();
            }
            finally
            {
                File.Delete(localfile);
            }
        }
    }
}