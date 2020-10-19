using Hfs.Server.CODICE.CLASSI.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseDir: CommandBase
    {
        private IDirHandler mDirHandler;
        protected IDirHandler DirHandler
        {
            get
            {
                return this.mDirHandler;
            }
        }
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            this.mDirHandler = FileDirHandlerFactory.GetDirFromHfsResp(this.Response).Result;
        }

        #endregion

        #region PUBLIC METHODS

        public override void Dispose()
        {
            base.Dispose();
            if (this.mDirHandler != null)
                this.mDirHandler.Dispose();
        }

        #endregion

    }
}