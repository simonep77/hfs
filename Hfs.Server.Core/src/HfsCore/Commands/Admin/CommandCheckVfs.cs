using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandCheckVfs: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"checkvfs";
        public override VfsAction Action { get; } = VfsAction.Read;


        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(HfsData.Vfs.IsFullLoaded ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }
    }
}