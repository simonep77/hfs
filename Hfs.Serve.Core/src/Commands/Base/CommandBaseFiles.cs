using Hfs.Server.Core.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target 2 files
    /// </summary>
    public abstract class CommandBaseFiles: CommandBaseFile
    {
        protected IFileHandler? FileHandlerDest {  get; private set; }
       
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            base.ApplyVfsCustomActions();
            this.FileHandlerDest = FileDirHandlerFactory.GetFileFromHfsResp(this.ResponseDest);
        }

        #endregion

        #region PUBLIC METHODS

        protected override void commandDispose()
        {
            this.FileHandlerDest?.Dispose();
        }

        #endregion

    }
}