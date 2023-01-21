using Hfs.Server.CODICE.CLASSI.FileHandling;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseDirs: CommandBaseDir
    {
        private IDirHandler mDirHandlerDest;
        protected IDirHandler DirHandlerDest
        {
            get
            {
                return this.mDirHandlerDest;
            }
        }
       
        #region PROTECTED METHODS

        protected override void ApplyVfsCustomActions()
        {
            base.ApplyVfsCustomActions();
            this.mDirHandlerDest = FileDirHandlerFactory.GetDirFromHfsResp(this.ResponseDest).Result;
        }

        #endregion

        #region PUBLIC METHODS

        public override void Dispose()
        {
            base.Dispose();
            this.mDirHandlerDest.Dispose();
        }

        #endregion

    }
}