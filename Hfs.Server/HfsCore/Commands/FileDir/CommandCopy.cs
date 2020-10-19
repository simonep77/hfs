using Hfs.Server.CODICE.VFS;
using System.Threading.Tasks;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandCopy: CommandBaseFiles
    {

        public override string ActionKey { get; } = @"copy";
        public override VfsAction Action { get; } = VfsAction.Read;
        public override VfsAction ActionDest { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            this.FileHandler.CopyTo(this.FileHandlerDest);
        }
    }
}