using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

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

        HttpContext Context { get; }

        HfsRequest Request { get; }

        HfsResponseVfs Response { get; }

        HfsResponseVfs ResponseDest { get; }

        /// <summary>
        /// Inizializza il comando
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="request"></param>
        void Init(HttpContext context, HfsRequest request);
     
        /// <summary>
        /// Esegue il comando specificato
        /// </summary>
        Task Execute();

    }
}
