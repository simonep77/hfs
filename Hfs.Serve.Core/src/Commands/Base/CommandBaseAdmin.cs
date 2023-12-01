using Hfs.Server.Core.Common;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseAdmin: CommandBase
    {

        protected override void commandInit()
        {
            //Imposta come virtual path il path di admin
            this.Request.VPath = Const.VFS_PATH_ADMIN;
        }
    }
}