using System;
using System.Text;
using System.IO;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Classe per log su file di testo
    /// </summary>
    public class FileLogger   
    {
        #region FIELDS

        private object mSyncLock = new object();
        private string mFilePath;
        private string mDateFormat = @"dd/MM/yyyy HH:mm:ss";
        private bool mWriteThreadId;

        #endregion 

        #region FIELDS

        /// <summary>
        /// Crea logger su file fornito in modalità append fornita e attivo specificato
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="append"></param>
        /// <param name="startOpen"></param>
        public FileLogger(string filePath)
        {
            this.mFilePath = filePath;
            this.mWriteThreadId = false;
        }

        #endregion

        #region ILogger Membri di

        /// <summary>
        /// Path completo file
        /// </summary>
        public string FilePath
        {
            get { return this.mFilePath; }
        }

        /// <summary>
        /// Formato data
        /// </summary>
        public string DateFormat
        {
            get
            {
                return this.mDateFormat;
            }
            set
            {
                try
                {
                    DateTime.Now.ToString(value);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Il formato data impostato non risulta valido.", e);
                }
                
                this.mDateFormat = value;
            }
        }

        /// <summary>
        /// Indica se scrivere nel log l'id del thread
        /// </summary>
        public bool WriteThreadId
        {
            get { return this.mWriteThreadId; }
            set { this.mWriteThreadId = value; }
        }


        /// <summary>
        /// Scrive Messaggio Log
        /// </summary>
        /// <param name="msgIn"></param>
        /// <param name="args"></param>
        public void LogMessage(string msgIn, params object[] args)
        {
            lock (this.mSyncLock)
            {
                //scrive
                using (StreamWriter oStream = File.AppendText(this.mFilePath))
                {
                    //Scrive
                    string sDate = DateTime.Now.ToString(this.mDateFormat);
                    string sMsg = string.Format(msgIn, args);

                    oStream.Write(sDate);
                    //Se necessario scrive Id del thread
                    if (this.mWriteThreadId)
                    {
                        string sThread = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4");
                        oStream.Write(@" - T");
                        oStream.Write(sThread);
                    }
                    oStream.Write(@" - ");
                    oStream.Write(sMsg);
                    oStream.WriteLine();

                    oStream.Flush();
                }
            }
        }


        /// <summary>
        /// Scrive informazioni su eccezione
        /// </summary>
        /// <param name="e"></param>
        public void LogException(Exception e, bool includeStack)
        {
            lock (this.mSyncLock)
            {
                //scrive
                using (StreamWriter oStream = File.AppendText(this.mFilePath))
                {
                    //Scrive
                    Exception oException;
                    int iIndentEx = 4;
                    int iInnerCount = 0;
                    string sSep = string.Empty.PadRight(210, '=');
                    string sDate = DateTime.Now.ToString(this.mDateFormat);
                    string sThread = string.Empty;

                    //Se necessario scrive Id del thread
                    if (this.mWriteThreadId)
                    {
                        sThread = string.Concat(@" - T", System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"));
                    }

                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sSep));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "ECCEZIONE!"));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Tipo     : ", e.GetType().Name));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Messaggio: ", e.Message));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Source   : ", e.Source));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Classe   : ", e.TargetSite.DeclaringType.Name));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Metodo   : ", e.TargetSite.Name));
                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Namespace: ", e.TargetSite.DeclaringType.Namespace));
                    if (includeStack)
                    {
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", "  * Stack    : ", e.StackTrace));
                    }

                    oException = e.InnerException;

                    while (oException != null)
                    {
                        string sIndent = string.Empty.PadRight(iIndentEx);
                        iInnerCount += 1;

                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, iInnerCount.ToString("D2"),") Inner"));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Tipo     : ", e.InnerException.GetType().Name));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Messaggio: ", e.InnerException.Message));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Source   : ", e.InnerException.Source));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Classe   : ", e.InnerException.TargetSite.DeclaringType.Name));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Metodo   : ", e.InnerException.TargetSite.Name));
                        oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent, "  + Namespace: ", e.InnerException.TargetSite.DeclaringType.Namespace));
                        if (includeStack)
                        {
                            oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sIndent , "  + Stack     : ", e.InnerException.StackTrace));
                        }

                        //Successiva
                        oException = oException.InnerException;
                        iIndentEx += 4;
                    }

                    oStream.WriteLine(string.Concat(sDate, sThread, @" - ", sSep));
                }
            }
        }


        /// <summary>
        /// Acquisisce esclusivo accesso al log per scrivere un blocco
        /// </summary>
        public void InitLogBlock()
        {
            System.Threading.Monitor.Enter(this.mSyncLock);
        }

        /// <summary>
        /// Libera accesso al log
        /// </summary>
        public void EndLogBlock()
        {
            System.Threading.Monitor.Exit(this.mSyncLock);
        }


        #endregion

    }
}
