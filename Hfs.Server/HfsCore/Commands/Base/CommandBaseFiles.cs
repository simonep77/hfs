using Hfs.Server.CODICE.CLASSI.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target 2 files
    /// </summary>
    public abstract class CommandBaseFiles: CommandBaseFile
    {
        private IFileHandler mFileHandlerDest;
        protected IFileHandler FileHandlerDest
        {
            get
            {
                return this.mFileHandlerDest;
            }
        }
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            base.ApplyVfsCustomActions();
            this.mFileHandlerDest = FileDirHandlerFactory.GetFileFromHfsResp(this.ResponseDest).Result;
        }

        #endregion

        #region PUBLIC METHODS

        public override void Dispose()
        {
            base.Dispose();
            this.mFileHandlerDest.Dispose();
        }

        #endregion

    }
}