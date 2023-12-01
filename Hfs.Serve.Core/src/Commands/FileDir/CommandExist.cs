using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandExist: CommandBaseFile
    {

        public override string ActionKey { get; } = @"exist";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(this.FileHandler.Exist() ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }
    }
}