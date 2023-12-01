
using Hfs.Server.Core.Common;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandEncPassword: CommandBaseUty
    {
        public override string ActionKey { get; } = @"encpassword";


        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(CryptoUtils.AesEncryptString(this.Request.VPath, this.Request.Pass));
        }
    }
}