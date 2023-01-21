using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandListVfs: CommandBaseUty
    {

        public override string ActionKey { get; } = @"listvfs";

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(HfsData.Vfs.GetUserPathsXml(this.Request.User, this.Request.Pass));
        }
    }
}