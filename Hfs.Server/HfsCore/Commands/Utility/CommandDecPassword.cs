using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandDecPassword: CommandBaseUty
    {
        public override string ActionKey { get; } = @"decpassword";

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(CryptoUtils.AesDecryptString(this.Request.VPath, this.Request.Pass));
        }
    }
}