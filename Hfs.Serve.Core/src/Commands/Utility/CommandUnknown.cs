using Hfs.Server.Core.Common;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandUnknown: CommandBaseUty
    {
        public override string ActionKey { get; } = @"unknown";

        async protected override Task CommandExecute()
        {
            throw new HfsException(EStatusCode.UnknownAction, $"Azione {this.Request.ActionStr} sconosciuta");
        }
    }
}