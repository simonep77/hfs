using Hfs.Server.Core.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseDirs: CommandBaseDir
    {
        protected IDirHandler? DirHandlerDest { get; private set; }
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            base.ApplyVfsCustomActions();
            this.DirHandlerDest = FileDirHandlerFactory.GetDirFromHfsResp(this.ResponseDest);
        }

        #endregion

        #region PUBLIC METHODS

        protected override void commandDispose()
        {
            this.DirHandlerDest?.Dispose();
        }

        #endregion

    }
}