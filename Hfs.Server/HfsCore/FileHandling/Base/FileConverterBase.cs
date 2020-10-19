using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Hfs.Server;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// Classe base per la conversione
    /// </summary>
    internal abstract class FileConverterBase
    {
        protected FileInfo Source;
        protected FileInfo Dest;
        protected static Semaphore _Pool_Processi_SYNC = new Semaphore(Properties.Settings.Default.MaxConversionProcesses, Properties.Settings.Default.MaxConversionProcesses);
        protected static Semaphore _Pool_Processi_ASYNC = new Semaphore(Properties.Settings.Default.MaxConversionProcesses, Properties.Settings.Default.MaxConversionProcesses);

        public FileConverterBase(FileInfo src, FileInfo dest)
        {
            this.Source = src;
            this.Dest = dest;
        }

        /// <summary>
        /// Verifica esistenza destinazione ed eventualmente aggiorna cache
        /// </summary>
        /// <param name="refreshCache"></param>
        /// <returns></returns>
        private bool checkExist(bool refreshCache)
        {
            //Se esiste destinazione verifica solo se necessario eseguire il refresh della cache
            if (this.Dest.Exists)
            {
                //Il refresh consiste nel movimentare la data ultima modifica
                if (refreshCache)
                {
                    this.Dest.LastWriteTime = DateTime.Now;
                    this.Dest.Refresh();
                }

                return true;
            }

            return false;
        }

        public void Convert(bool refreshCache)
        {
            //Se esiste destinazione verifica solo se necessario eseguire il refresh della cache
            if (this.checkExist(refreshCache))
                return;

            int iHalfWait = Properties.Settings.Default.MaxConversionWaitMsec / 2;

            //Ottiene avvio oppure attende fine altri
            if (!_Pool_Processi_SYNC.WaitOne(iHalfWait, false))
            {
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"{0} - Raggiunto limite massimo conversioni concorrenti ({1}) con tempo di attesa {2}", this.GetType().Name, Properties.Settings.Default.MaxConversionProcesses, iHalfWait);
                if (!_Pool_Processi_SYNC.WaitOne(iHalfWait, false))
                    throw new HfsException(EStatusCode.GenericError, @"Interrotto processo di conversione per attesa troppo lunga ({0} msec)", Properties.Settings.Default.MaxConversionWaitMsec);
            }
            try
            {
                this.doConvert();
            }
            finally
            {
                //Libera pool
                _Pool_Processi_SYNC.Release();
            }
        }

        /// <summary>
        /// Conversione asincrona
        /// </summary>
        public void ConvertAsync(bool refreshCache)
        {
            //Se esiste destinazione verifica solo se necessario eseguire il refresh della cache
            if (this.checkExist(refreshCache))
                return;

            //Lancia conversione
            Thread thd = new Thread(this.doConvertAsync);
            thd.Start();
        }

        private void doConvertAsync()
        {
            try
            {
                //Blocca indefinitamente la coda asincrona
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"{0} - In coda conversione asincrona file '{1}'", this.GetType().Name, this.Source.FullName);
                _Pool_Processi_ASYNC.WaitOne();
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"{0} - Inizio conversione asincrona file '{1}'", this.GetType().Name, this.Source.FullName);
                this.doConvert();
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"{0} - Fine conversione asincrona file '{1}'", this.GetType().Name, this.Source.FullName);
            }
            finally
            {
                //Libera pool
                _Pool_Processi_ASYNC.Release();
            }
            
        }


        protected abstract void doConvert();
    }
}