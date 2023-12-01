using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandCpDir: CommandBaseDirs
    {

        public override string ActionKey { get; } = @"cpdir";
        public override VfsAction Action { get; } = VfsAction.Read;
        public override VfsAction ActionDest { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            this.DirHandler.CopyTo(this.DirHandlerDest ?? throw new ArgumentNullException("Destinazione nulla"));
        }
    }
}