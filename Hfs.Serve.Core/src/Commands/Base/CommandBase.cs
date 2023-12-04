using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using Hfs.Server.Core.Common;
using Hfs.Server.Core.FileHandling;
using Hfs.Server.Core.Vfs;


namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi
    /// </summary>
    public abstract class CommandBase : ICommand
    {

        #region PROPERTIES

        public long BytesReceived { get; private set; }

        public long BytesSent { get; private set; }

        public HttpContext Context { get; private set; }

        public HfsRequest Request { get; private set; }

        public HfsResponseVfs Response { get; private set; }

        public HfsResponseVfs? ResponseDest { get; private set; }

        public abstract string ActionKey { get; }
        public abstract VfsAction Action { get; }
        public virtual VfsAction ActionDest { get; } = VfsAction.None;
        public virtual bool IsVfsRequired { get; } = true;

        #endregion

        public virtual void Init(HttpContext context, HfsRequest request)
        {
            this.Context = context;
            this.Request = request;

            //Inizializza il comando specifico
            this.commandInit();

            //Pulisce response e preimposta httpcode a 200
            this.Context.Response.Clear();
            //this.Context.Response. = false;
            //this.Context.Response.BufferOutput = false;
            this.Context.Response.StatusCode = 200; //Sempre
            this.SetResponseHeaders(EStatusCode.Ok, string.Empty);

        }

        /// <summary>
        /// Inizializzazione comando specifico
        /// </summary>
        protected virtual void commandInit()
        { }


        private void ApplyVfs()
        {
            //Controllo path virtuale principale e se ok carico
            Utility.CheckVirtualPath(this.Request.VPath, string.Empty);
            this.Response = HfsData.Vfs.GetResponse(this.Request, false);

            //Verifica permessi su vpath
            this.Response.Path.CheckAccess(this.Response.User, this.Action);

            //Logga accesso a risorsa
            if (HfsData.LogAccess && this.Action.ToString()[0] != Const.VFS_ACCESS_LIST)
                HfsData.WriteLog(string.Format(Const.LOG_ACCESS_MSG_FMT, this.Action.ToString(), this.ActionKey, this.Response.User.Name, this.Request.IP, this.Request.VPath));

            //Se presente destinazione viene controllata e poi caricata
            if (this.ActionDest != VfsAction.None)
            {
                if (string.IsNullOrWhiteSpace(this.Request.VPathDest))
                    throw new HfsException(EStatusCode.NoInputPath, $"Il comando richiesto necessita la valorizzazione del parametro {Const.QS_VPATH_DEST}");

                Utility.CheckVirtualPath(this.Request.VPathDest, string.Empty);
                this.ResponseDest = HfsData.Vfs.GetResponse(this.Request, true);

                //Verifica permessi su vpathdest
                this.ResponseDest.Path.CheckAccess(this.ResponseDest.User, this.ActionDest);

                //Logga accesso dest
                if (HfsData.LogAccess && this.ActionDest.ToString()[0] != Const.VFS_ACCESS_LIST)
                    HfsData.WriteLog(string.Format(Const.LOG_ACCESS_MSG_FMT, this.ActionDest.ToString(), this.ActionKey, this.Response.User.Name, this.Request.IP, this.Request.VPathDest));

            }

            //Chiama prepare del comando
            this.ApplyVfsCustomActions();
        }

        /// <summary>
        /// Richiama preparazione specifica di ogni comando
        /// </summary>
        protected virtual void ApplyVfsCustomActions()
        {

        }

        async public Task Execute()
        {
            if (this.IsVfsRequired)
                this.ApplyVfs();

            await this.CommandExecute();
        }


        protected abstract Task CommandExecute();


        #region PROTECTED METHODS

        /// <summary>
        /// Inizia flusso output impostando esito
        /// </summary>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        protected void SetResponseHeaders(EStatusCode code, string msg)
        {
            this.Context.Response.Headers.Clear();
            this.Context.Response.Headers[Const.HEADER_STATUS_CODE] = code.ToString("D");
            this.Context.Response.Headers[Const.HEADER_STATUS_MSG] = msg;
        }


        /// <summary>
        /// Scrive testo nell'output
        /// </summary>
        protected async Task WriteResponseText(string text)
        {
            //Imposta Content Type
            this.Context.Response.ContentType = Const.HEADER_CONTENT_TYPE_TEXT;
            
            //Esegue
            await this.Context.Response.WriteAsync(text);
            await this.Context.Response.Body.FlushAsync();
            //await this.Context.Response.CompleteAsync();
            //Segna bytes
            this.BytesSent += text.Length;
        }

        /// <summary>
        /// Scrive file binario nell'output
        /// </summary>
        /// <param name="file"></param>
        async protected Task WriteResponseFile(IFileHandler fh)
        {
            using (Stream fs = fh.OpenRead())
            {
                this.BytesSent += fs.Length;
                //Scrivo header
                this.Context.Response.ContentType = MimeHelper.GetMimeFromFilename(fh.Name);
                this.Context.Response.ContentLength = fs.Length;
                this.Context.Response.Headers[Const.HEADER_CONTENT_DISP] = string.Format(Const.HEADER_CONTENT_DISP_VAL, fh.Name);

                await fs.CopyToAsync(this.Context.Response.Body, Const.BUFFER_LEN);
                await this.Context.Response.Body.FlushAsync();

            }
        }


        /// <summary>
        /// Salva dati file arrivati
        /// </summary>
        /// <param name="file"></param>
        /// <param name="origin"></param>
        async protected Task SaveRequestFile(IFileHandler fh, bool append)
        {
            //Controlla che ci siano dati
            if (this.Context.Request.ContentLength <= 0)
                throw new HfsException(EStatusCode.DataExpected, "Non sono stati forniti dati da scrivere");

            //Salva dimensione dati
            this.BytesReceived += this.Context.Request.ContentLength ?? 0;

            //Appende dati
            using (Stream oFs = fh.OpenWrite(!append))
            {
                //Imposta seek
                if (append)
                {
                    //Verifica se puo' eseguire seek
                    if (!oFs.CanSeek)
                        throw new HfsException(EStatusCode.CannotSeek, $"Impossibile eseguire il seek sul file '{fh.FullName}'");
                    //Seek
                    oFs.Seek(0, SeekOrigin.End);
                }

                try
                {
                    await this.Context.Request.Body.CopyToAsync(oFs, Const.BUFFER_LEN);
                    await oFs.FlushAsync();
                }
                finally
                {
                    //Chiude stream
                    this.Context.Request.Body.Close();
                }
            }
        }


        #endregion

        public void Dispose()
        {
            this.commandDispose();
        }

        /// <summary>
        /// Metodo per azioni custom di dispose
        /// </summary>
        protected virtual void commandDispose()
        { }


    }
}