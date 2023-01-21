using Hfs.Server.CODICE.VFS;
using System.Web;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandRestart: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"restart";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            //HttpRuntime.UnloadAppDomain();
        }
    }
}