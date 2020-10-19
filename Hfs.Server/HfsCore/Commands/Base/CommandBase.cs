using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.CLASSI.FileHandling;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi
    /// </summary>
    public abstract class CommandBase: ICommand
    {
        private Controller _controller;
        private HfsRequest _request;
        private HfsResponseVfs _response;
        private HfsResponseVfs _responseDest;
        private int _bytesReceived;
        private int _bytesSent;

        #region PROPERTIES

        public int BytesReceived
        {
            get
            {
                return this._bytesReceived;
            }
        }

        public int BytesSent
        {
            get
            {
                return this._bytesSent;
            }
        }

        public Controller Controller
        {
            get
            {
                return this._controller;
            }
        }

        public HttpContextBase Context
        {
            get
            {
                return this._controller.HttpContext;
            }
        }

        public HfsRequest Request
        {
            get
            {
                return this._request;
            }
        }

        public HfsResponseVfs Response
        {
            get
            {
                return this._response;
            }
        }

        public HfsResponseVfs ResponseDest
        {
            get
            {
                return this._responseDest;
            }
        }

        public abstract string ActionKey { get; }
        public abstract VfsAction Action { get; }
        public virtual VfsAction ActionDest { get; } = VfsAction.None;



        public virtual bool IsVfsRequired { get; } = true;

        protected bool IsRedirectToHfs { get; set; }

        #endregion

        public virtual void Init(Controller controller, HfsRequest request)
        {
            this._controller = controller;
            this._request = request;
            
            //Pulisce response e preimposta httpcode a 200
            this.Context.Response.Clear();
            this.Context.Response.Buffer = false;
            this.Context.Response.BufferOutput = false;
            this.Context.Response.StatusCode = 200; //Sempre
            this.SetResponseHeaders(EStatusCode.Ok, string.Empty);

        }


        private void ApplyVfs()
        {
            //Controllo path virtuale principale e se ok carico
            Utility.CheckVirtualPath(this._request.VPath, string.Empty);
            this._response = HfsData.Vfs.GetResponse(this._request, false);

            //Verifica permessi su vpath
            if (!this._response.Path.CheckAccess(this._response.User, this.Action))
                throw new ApplicationException($"Utente '{this._response.User.Name}' non autorizzato all'operazione '{this.Action}' sulla risorsa");

            //Logga accesso a risorsa
            if (Properties.Settings.Default.LogAccess && this.Action.ToString()[0] != Const.VFS_ACCESS_LIST)
                HfsData.Logger.WriteMessage(ELogType.HfsAccess, Const.LOG_ACCESS_MSG_FMT, this.Action.ToString(), this.ActionKey, this.Response.User.Name, this.Request.IP, this.Request.VPath);


            //Se presente destinazione viene controllata e poi caricata
            if (!string.IsNullOrEmpty(this._request.VPathDest))
            {
                Utility.CheckVirtualPath(this._request.VPathDest, string.Empty);
                this._responseDest = HfsData.Vfs.GetResponse(this._request, true);

                //Verifica permessi su vpathdest
                if (!this._responseDest.Path.CheckAccess(this._responseDest.User, this.ActionDest))
                    throw new ApplicationException($"Utente '{this._responseDest.User.Name}' non autorizzato all'operazione '{this.ActionDest}' sulla risorsa");
                //Logga accesso dest
                if (Properties.Settings.Default.LogAccess && this.ActionDest.ToString()[0] != Const.VFS_ACCESS_LIST)
                    HfsData.Logger.WriteMessage(ELogType.HfsAccess, Const.LOG_ACCESS_MSG_FMT, this.ActionDest.ToString(), this.ActionKey, this.Response.User.Name, this.Request.IP, this.Request.VPathDest);

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
            {
                this.ApplyVfs();
            }

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
            this.Context.Response.ClearHeaders();
            this.Context.Response.AddHeader(Const.HEADER_STATUS_CODE, code.ToString("D"));
            this.Context.Response.AddHeader(Const.HEADER_STATUS_MSG, msg);
        }


        /// <summary>
        /// Scrive testo nell'output
        /// </summary>
        protected async Task WriteResponseText(string text)
        {
            //Imposta Content Type
            this.Context.Response.ContentType = Const.HEADER_CONTENT_TYPE_TEXT;

            //Esegue
            await this.Context.Response.Output.WriteAsync(text);
            await this.Context.Response.Output.FlushAsync();
            //Segna bytes
            this._bytesSent += text.Length;
        }

        /// <summary>
        /// Scrive file binario nell'output
        /// </summary>
        /// <param name="file"></param>
        async protected Task WriteResponseFile(IFileHandler fh, FileBlock block)
        {

            //context.Response.WriteFile(file);
            //context.Response.Flush();
            byte[] buffer = new byte[Const.BUFFER_LEN];
            using (Stream fs = fh.OpenRead())
            {
                //Calcola la dimensione massima che si vuole ritornare
                long lLength = (block.Length == Const.BLOCK_LENGHT_MAX) ? fs.Length : Math.Max(block.Length, fs.Length);

                //Scrivo header
                this.Context.Response.ContentType = MimeHelper.GetMimeFromExtension(fh.Extension);
                this.Context.Response.AddHeader(Const.HEADER_CONTENT_LEN, lLength.ToString());
                this.Context.Response.AddHeader(Const.HEADER_CONTENT_DISP, string.Format(Const.HEADER_CONTENT_DISP_VAL, fh.Name));

                //Si posiziona al punto specificato
                if (fs.CanSeek)
                    fs.Seek(block.Offset, SeekOrigin.Begin);

                int iRead = 0;
                while (lLength > 0)
                {
                    iRead = await fs.ReadAsync(buffer, 0, Const.BUFFER_LEN);
                    lLength -= iRead;
                    await this.Context.Response.OutputStream.WriteAsync(buffer, 0, iRead);
                    await this.Context.Response.OutputStream.FlushAsync();
                    this._bytesSent += iRead;
                }
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
            this._bytesReceived += this.Context.Request.ContentLength;

            //Carica dati
            byte[] buffer = new byte[Const.BUFFER_LEN];

            //Appende dati
            using (Stream oFs = fh.OpenWrite(!append))
            {
                //Imposta seek
                if (append && oFs.CanSeek)
                    oFs.Seek(0, SeekOrigin.End);

                try
                {
                    //Inizia lettura dati
                    int iRead = 0;
                    while ((iRead = await this.Context.Request.InputStream.ReadAsync(buffer, 0, Const.BUFFER_LEN)) > 0)
                    {
                        await oFs.WriteAsync(buffer, 0, iRead);
                    }
                }
                finally
                {
                    //Flush file
                    oFs.Flush();
                    //Chiude stream
                    this.Context.Request.InputStream.Close();
                }
            }
        }


        #endregion

        public virtual void Dispose()
        {
        }


    }
}