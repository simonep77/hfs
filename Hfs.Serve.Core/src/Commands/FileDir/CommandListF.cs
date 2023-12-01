using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandListF: CommandBaseDir
    {
        public override string ActionKey { get; } = @"listf";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(string.Concat(@"<root>", this.DirHandler.GetFilesXml(this.Request.Pattern, (this.Request.Attr == @"R")), @"</root>"));
        }
    }
}