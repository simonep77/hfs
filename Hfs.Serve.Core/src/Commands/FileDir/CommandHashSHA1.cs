using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandHashSHA1: CommandBaseFile
    {

        public override string ActionKey { get; } = @"hashsha1";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override Task CommandExecute()
        {
            await this.WriteResponseText(this.FileHandler.GetSHA1());
        }
    }
}