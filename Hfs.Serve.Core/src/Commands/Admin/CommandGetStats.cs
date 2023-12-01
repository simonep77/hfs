


using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandGetStats: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"getstats";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(HfsData.Stats.GetXmlStats());
        }
    }
}