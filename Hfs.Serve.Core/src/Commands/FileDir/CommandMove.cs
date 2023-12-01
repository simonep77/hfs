using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandMove: CommandBaseFiles
    {
        public override string ActionKey { get; } = @"move";
        public override VfsAction Action { get; } = VfsAction.Read;
        public override VfsAction ActionDest { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            this.FileHandler.MoveTo(this.FileHandlerDest ?? throw new ArgumentNullException("Destinazione nulla"));
        }
    }
}