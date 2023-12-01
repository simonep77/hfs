using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandExistDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"existdir";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(this.DirHandler.Exist() ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }
    }
}