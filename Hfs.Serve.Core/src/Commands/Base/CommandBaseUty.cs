using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    /// <summary>
    /// Classe base per esecuzione comandi che hanno come target il file
    /// </summary>
    public abstract class CommandBaseUty: CommandBase
    {
        public override bool IsVfsRequired { get; } = false;
        public override VfsAction Action { get; } = VfsAction.None;


    }
}