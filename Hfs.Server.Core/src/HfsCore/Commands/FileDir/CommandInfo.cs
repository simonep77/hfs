using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandInfo: CommandBaseFile
    {

        public override string ActionKey { get; } = @"info";
        public override VfsAction Action { get; } = VfsAction.List;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.WriteResponseText(string.Concat(@"<root>", this.FileHandler.GetInfoXml(), @"</root>"));
        }
    }
}