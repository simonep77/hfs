
using Hfs.Server.Core.Common;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandHelp: CommandBaseUty
    {

        public override string ActionKey { get; } = @"help";

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(Utility.GetHelpHtml());
        }
    }
}