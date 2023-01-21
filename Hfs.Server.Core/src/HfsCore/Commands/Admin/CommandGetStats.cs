using Hfs.Server.CODICE.VFS;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandGetStats: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"getstats";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(HfsData.Stats.GetXmlStats());
        }
    }
}