using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandListD: CommandBaseDir
    {

        public override string ActionKey { get; } = @"listd";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(string.Concat(@"<root>", this.DirHandler.GetDirsXml(this.Request.Pattern, (this.Request.Attr == @"R")), @"</root>"));
        }
    }
}