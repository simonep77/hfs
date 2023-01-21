using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;
using System;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandReloadVfs: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"reloadvfs";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            //Ricarica
            HfsData.Vfs.Load();
            //Forza un GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}