
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandTouch: CommandBaseFile
    {

        public override string ActionKey { get; } = @"touch";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            this.FileHandler.OpenWrite(true).Close();
        }
    }
}