using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandMvDir: CommandBaseDirs
    {

        public override string ActionKey { get; } = @"mvdir";
        public override VfsAction Action { get; } = VfsAction.Read;
        public override VfsAction ActionDest { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.DirHandler.MoveTo(this.DirHandlerDest);
        }
    }
}