using Hfs.Server.Core.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseFile : CommandBase
    {
        protected IFileHandler? FileHandler { get; private set; }

        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            //Imposta l'handler
            this.FileHandler = FileDirHandlerFactory.GetFileFromHfsResp(this.Response);
        }

        #endregion


        #region PUBLIC METHODS

        protected override void commandDispose()
        {
            this.FileHandler?.Dispose();
        }

        #endregion
    }
}