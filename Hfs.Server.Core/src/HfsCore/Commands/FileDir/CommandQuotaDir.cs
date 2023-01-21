using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandQuotaDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"quotadir";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(this.DirHandler.GetSize().ToString());
        }
    }
}