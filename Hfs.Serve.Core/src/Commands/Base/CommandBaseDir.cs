using Hfs.Server.Core.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseDir : CommandBase
    {
        protected IDirHandler? DirHandler { get; private set; }


        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            this.DirHandler = FileDirHandlerFactory.GetDirFromHfsResp(this.Response);
        }

        #endregion

        #region PUBLIC METHODS

        protected override void commandDispose()
        {
            this.DirHandler?.Dispose();
        }

        #endregion

    }
}