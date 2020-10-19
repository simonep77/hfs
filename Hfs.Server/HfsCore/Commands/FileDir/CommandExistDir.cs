using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandExistDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"existdir";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(this.DirHandler.Exist() ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }
    }
}