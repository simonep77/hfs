using Hfs.Server.CODICE.CLASSI;
using Microsoft.AspNetCore.Mvc;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseAdmin: CommandBase
    {

        public override void Init(Controller controller, HfsRequest request)
        {
            base.Init(controller, request);

            //Imposta come virtual path il path di admin
            this.Request.VPath = Const.VFS_PATH_ADMIN;
        }
    }
}