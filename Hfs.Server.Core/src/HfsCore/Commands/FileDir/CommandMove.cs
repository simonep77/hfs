using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandMove: CommandBaseFiles
    {

        public override string ActionKey { get; } = @"move";
        public override VfsAction Action { get; } = VfsAction.Read;
        public override VfsAction ActionDest { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.FileHandler.MoveTo(this.FileHandlerDest);
        }
    }
}