using Hfs.Server.CODICE.VFS;
using System;
using System.IO;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandSetAttr: CommandBaseFile
    {

        public override string ActionKey { get; } = @"setattr";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            this.FileHandler.SetAttribute((FileAttributes)Convert.ToInt32(this.Request.Attr));
        }
    }
}