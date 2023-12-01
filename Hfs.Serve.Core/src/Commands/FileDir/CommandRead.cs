using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandRead: CommandBaseFile
    {

        public override string ActionKey { get; } = @"read";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override Task CommandExecute()
        {
            //Scrive
            await this.WriteResponseFile(this.FileHandler);
        }
    }
}