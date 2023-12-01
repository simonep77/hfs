

using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;
using System;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandReloadVfs: CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"reloadvfs";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            //Ricarica
            HfsData.Vfs.Load();
            //Forza un GC
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}