using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;
using Microsoft.AspNetCore.Mvc;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandExist: CommandBaseFile
    {

        public override string ActionKey { get; } = @"exist";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(this.FileHandler.Exist() ? Const.RESP_TEXT_TRUE : Const.RESP_TEXT_FALSE);
        }


    }
}