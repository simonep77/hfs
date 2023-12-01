using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;
using System.Text;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandGenRSA: CommandBaseFile
    {
        public override string ActionKey { get; } = @"genrsa";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            using (var s = this.FileHandler.OpenWrite(true))
            {
                var rsabytes = Encoding.UTF8.GetBytes(CryptoUtils.RsaGenerateToXml());
                await s.WriteAsync(rsabytes, 0, rsabytes.Length);
            }


        }
    }
}