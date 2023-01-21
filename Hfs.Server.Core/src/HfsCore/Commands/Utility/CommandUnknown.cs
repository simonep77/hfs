using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandUnknown: CommandBaseUty
    {
        public override string ActionKey { get; } = @"unknown";

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            throw new HfsException(EStatusCode.UnknownAction, "Azione {0} sconosciuta", this.Request.ActionStr);
        }
    }
}