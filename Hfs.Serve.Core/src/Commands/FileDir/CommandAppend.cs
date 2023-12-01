using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandAppend: CommandBaseFile
    {
        public override string ActionKey { get; } = @"append";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            await this.SaveRequestFile(this.FileHandler, true);
        }
    }
}