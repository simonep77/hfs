using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandDelete: CommandBaseFile
    {

        public override string ActionKey { get; } = @"delete";
        public override VfsAction Action { get; } = VfsAction.Delete;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
             this.FileHandler.Delete();
        }
    }
}