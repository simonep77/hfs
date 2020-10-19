using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandHelp: CommandBaseUty
    {

        public override string ActionKey { get; } = @"help";

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(Utility.GetHelpHtml());
        }
    }
}