using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandGetLink: CommandBase
    {

        public override string ActionKey { get; } = @"getlink";
        public override VfsAction Action { get; } = VfsAction.List;

        protected override void ApplyVfsCustomActions()
        {
        //Non deve fare nulla
        }

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(Utility.CreateReadLink(this.Context, this.Request));
        }
    }
}