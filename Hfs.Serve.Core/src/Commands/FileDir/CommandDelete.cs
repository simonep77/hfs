using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandDelete: CommandBaseFile
    {

        public override string ActionKey { get; } = @"delete";
        public override VfsAction Action { get; } = VfsAction.Delete;

        async protected override Task CommandExecute()
        {
             this.FileHandler.Delete();
        }
    }
}