using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Logger asincrono
    /// </summary>
    public class HfsAsyncLogger : System.Web.Hosting.IRegisteredObject
    {
        #region INTERNAL CLASS

        /// <summary>
        /// Classe interna per la gestione del log
        /// </summary>
        private class LogDefinition
        {
            public object SyncRoot = new object();
            public ELogType LogType;
            public string FileFormatPath;
            public StringBuilder LogBuffer;

            /// <summary>
            /// Crea un nuovo buffer per il log
            /// </summary>
            public void RenewBuffer()
            {
                this.LogBuffer = new StringBuilder(2000);
            }
        }

        #endregion
        private bool mSecurityUnrestricted;
        private Thread mThread;
        private ManualResetEvent mRunEvent = new ManualResetEvent(false);
        private bool mStarted;
        private Dictionary<ELogType, LogDefinition> mLogDefs = new Dictionary<ELogType, LogDefinition>();

        #region PUBLIC

        public bool IsActive
        {
            get { 
                return (this.mThread != null && this.mThread.IsAlive);
            }
        }

        /// <summary>
        /// Registra un log da gestire
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fileFormatPath"></param>
        public void InitLog(ELogType type, string fileFormatPath)
        {
            if (this.mLogDefs.ContainsKey(type))
                throw new HfsException(EStatusCode.GenericError, "Tipo log {0} gia' definito.", type);

            //Controlla path
            if (string.IsNullOrEmpty(fileFormatPath))
                throw new HfsException(EStatusCode.GenericError, "File di log non fornito.");

            //Crea directory
            string sDir = Path.GetDirectoryName(fileFormatPath);
            if (!Directory.Exists(sDir))
                Directory.CreateDirectory(sDir);

            //Aggiunge
            LogDefinition oDef = new LogDefinition();
            oDef.LogType = type;
            oDef.FileFormatPath = fileFormatPath;
            oDef.RenewBuffer();
            this.mLogDefs.Add(type, oDef);
        }

        /// <summary>
        /// Inizia scrittura esclusiva
        /// </summary>
        public void BeginWrite(ELogType type)
        {
            Monitor.Enter(this.mLogDefs[type].SyncRoot);
        }

        /// <summary>
        /// Termina scrittura esclusiva
        /// </summary>
        public void EndWrite(ELogType type)
        {
            Monitor.Exit(this.mLogDefs[type].SyncRoot);
        }


        /// <summary>
        /// Accoda record log
        /// </summary>
        /// <param name="msgFmt"></param>
        /// <param name="args"></param>
        public void WriteMessage(ELogType type, string msgFmt, params object[] args)
        {
            string sLine = string.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), @" - T",
                System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), @" - ",
                string.Format(msgFmt, args));

            LogDefinition ld = this.mLogDefs[type];

            lock (ld.SyncRoot)
            {
                ld.LogBuffer.AppendLine(sLine);
            }
        }

        /// <summary>
        /// Scrive eccezione
        /// </summary>
        /// <param name="type"></param>
        /// <param name="e"></param>
        public void WriteException(ELogType type, Exception e)
        {

            StringBuilder sb = new StringBuilder(1500);
            //Scrive
            Exception oException;
            int iIndentEx = 0;
            int iInnerCount = 0;
            string sSep = string.Empty.PadRight(210, '=');
            string sMsgBase = string.Concat(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), @" - T",
                System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), @" - ");

            sb.Append(sMsgBase);
            sb.AppendLine(sSep);

            oException = e;

            while (oException != null)
            {
                string sIndent = string.Empty.PadRight(iIndentEx);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                if (iIndentEx == 0)
                {
                    sb.AppendLine("ECCEZIONE!");
                }
                else
                {
                    sb.Append(iInnerCount.ToString("D2"));
                    sb.AppendLine(") Inner");
                }

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Tipo     : ");
                sb.AppendLine(oException.GetType().Name);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Messaggio: ");
                sb.AppendLine(oException.Message);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Source   : ");
                sb.AppendLine(oException.Source);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Classe   : ");
                sb.AppendLine(oException.TargetSite.DeclaringType.Name);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Metodo   : ");
                sb.AppendLine(oException.TargetSite.Name);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Namespace: ");
                sb.AppendLine(oException.TargetSite.DeclaringType.Namespace);

                sb.Append(sMsgBase);
                sb.Append(sIndent);
                sb.Append("  * Stack    : ");
                sb.AppendLine(oException.StackTrace);

                //Successiva
                iInnerCount++;
                oException = oException.InnerException;
                iIndentEx += 4;
            }

            sb.Append(sMsgBase);
            sb.AppendLine(sSep);

            //infine scrive
            LogDefinition ld = this.mLogDefs[type];

            lock (ld.SyncRoot)
            {
                ld.LogBuffer.Append(sb.ToString());
            }
        }


        /// <summary>
        /// Avvia logging asincrono
        /// </summary>
        public void Start()
        {
            //check avviato
            if (this.mStarted)
                return;

            this.mSecurityUnrestricted = System.Security.SecurityManager.IsGranted(new System.Security.Permissions.SecurityPermission(System.Security.Permissions.PermissionState.Unrestricted));

            if (this.mSecurityUnrestricted)
            {
                if (System.Web.Hosting.HostingEnvironment.IsHosted)
                    System.Web.Hosting.HostingEnvironment.RegisterObject(this);
            }            

            //Imposta flag off
            this.mStarted = true;

            //avvia thread di gestione
            this.mThread = new Thread(new ThreadStart(this.handleLogs));
            this.mThread.Start();
            this.mRunEvent.Reset();
        }

        #endregion

        #region PRIVATE

        /// <summary>
        /// Loop principale di gestione dei log
        /// </summary>
        private void handleLogs()
        {
            while (this.mStarted)
            {
                //Attende
                this.mRunEvent.WaitOne(15000, false);
                //Flush logs
                this.flushLogs(false);
                
            }
        }

        /// <summary>
        /// Scrive log su file
        /// </summary>
        private void flushLogs(bool force)
        {

            //Controlla ogni entry di log
            foreach (KeyValuePair<ELogType, LogDefinition> item in this.mLogDefs)
            {
                StringBuilder sb;

                //Esegue lock per liberare il buffer
                LogDefinition ld = this.mLogDefs[item.Value.LogType];

                lock (ld.SyncRoot)
                {
                    //Copia il buffer
                    sb = ld.LogBuffer;
                    ld.RenewBuffer();
                }

                //Se vuoto esce
                if (sb.Length == 0)
                    continue;

                //Ok, scrive (se il nome del file fornito comprende paramtri data allora procede alla sostituzione)
                DateTime dtNow = DateTime.Now;
                File.AppendAllText(string.Format(item.Value.FileFormatPath, dtNow.Year, dtNow.Month, dtNow.Day), sb.ToString());

                //Svuota buffer
                sb.Length = 0;
                sb.Capacity = 0;
                sb = null;
            }
        }

        #endregion

        #region IRegisteredObject Membri di

        public void Stop(bool immediate)
        {
            if (this.mSecurityUnrestricted)
            {
                if (System.Web.Hosting.HostingEnvironment.IsHosted)
                    System.Web.Hosting.HostingEnvironment.UnregisterObject(this);
            }

            //Imposta flag off
            this.mStarted = false;
            this.mRunEvent.Set();
        }

        #endregion
    }
}