using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandMkDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"mkdir";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.DirHandler.Create();
        }
    }
}