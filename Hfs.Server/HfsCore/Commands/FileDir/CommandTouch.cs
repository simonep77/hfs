using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandTouch: CommandBaseFile
    {

        public override string ActionKey { get; } = @"touch";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.FileHandler.OpenWrite(true).Close();
        }
    }
}