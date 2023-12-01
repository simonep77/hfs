
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandWrite: CommandBaseFile
    {
        public override string ActionKey { get; } = @"write";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            await this.SaveRequestFile(this.FileHandler, false);
        }
    }
}