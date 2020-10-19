using Hfs.Server.CODICE.CLASSI.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseFile: CommandBase
    {
        private IFileHandler mFilehandler;
        protected IFileHandler FileHandler
        {
            get
            {
                return this.mFilehandler;
            }
        }
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            //Imposta l'handler
            this.mFilehandler = FileDirHandlerFactory.GetFileFromHfsResp(this.Response).Result;
        }

        #endregion


        #region PUBLIC METHODS

        public override void Dispose()
        {
            base.Dispose();

            if (this.mFilehandler != null)
                this.mFilehandler.Dispose();
        }

        #endregion
    }
}