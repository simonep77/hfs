


using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandCheckVfs: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"checkvfs";
        public override VfsAction Action { get; } = VfsAction.Read;


        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(HfsData.Vfs.IsFullLoaded ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }
    }
}