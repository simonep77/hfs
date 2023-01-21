using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandRmDir: CommandBaseDir
    {
        public override string ActionKey { get; } = @"rmdir";

        public override VfsAction Action { get; } = VfsAction.Delete;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.DirHandler.Delete();
        }
    }
}