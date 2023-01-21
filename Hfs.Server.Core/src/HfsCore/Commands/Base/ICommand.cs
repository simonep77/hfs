using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.CLASSI.FileHandling;
using Hfs.Server.CODICE.VFS;
using Microsoft.AspNetCore.Mvc;

namespace Hfs.Server.HfsCore.Commands
{
    public interface ICommand: IDisposable
    {
        /// <summary>
        /// Stringa di identificazione del comando
        /// </summary>
        string ActionKey { get; }

        /// <summary>
        /// Livello di azione del comando
        /// </summary>
        VfsAction Action { get; }
        VfsAction ActionDest { get; }

        /// <summary>
        /// Indica se il comando deve caricare e verificare i dati del VFS
        /// </summary>
        bool IsVfsRequired { get; }

        long BytesReceived{ get; }

        long BytesSent { get; }

        Controller Controller { get; }

        HttpContext Context { get; }

        HfsRequest Request { get; }

        HfsResponseVfs Response { get; }

        HfsResponseVfs ResponseDest { get; }

        /// <summary>
        /// Inizializza il comando
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="request"></param>
        void Init(Controller controller, HfsRequest request);
     
        /// <summary>
        /// Esegue il comando specificato
        /// </summary>
        Task Execute();

    }
}
