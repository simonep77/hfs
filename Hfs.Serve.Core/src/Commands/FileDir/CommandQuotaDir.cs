using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandQuotaDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"quotadir";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(this.DirHandler.GetSize().ToString());
        }
    }
}