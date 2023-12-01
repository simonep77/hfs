using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandRmDir: CommandBaseDir
    {
        public override string ActionKey { get; } = @"rmdir";

        public override VfsAction Action { get; } = VfsAction.Delete;

        async protected override Task CommandExecute()
        {
            this.DirHandler.Delete();
        }
    }
}