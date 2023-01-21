using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;
using System.Text;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandGenRSA: CommandBaseFile
    {
        public override string ActionKey { get; } = @"genrsa";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            using (var s = this.FileHandler.OpenWrite(true))
            {
                var rsabytes = Encoding.UTF8.GetBytes(CryptoUtils.RsaGenerateToXml());
                s.Write(rsabytes, 0, rsabytes.Length);
            }


        }
    }
}