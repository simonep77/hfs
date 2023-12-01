using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandInfo: CommandBaseFile
    {

        public override string ActionKey { get; } = @"info";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(string.Concat(@"<root>", this.FileHandler.GetInfoXml(), @"</root>"));
        }
    }
}