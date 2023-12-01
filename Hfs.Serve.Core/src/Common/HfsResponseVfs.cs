using Hfs.Server.Core.Vfs;
using System;
using System.Collections.Generic;
using System.Web;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Classe di ritorno informazioni su path utente
    /// </summary>
    public class HfsResponseVfs
    {
        public string VirtualPath;
        public string PhysicalPath;
        public VfsPath Path;
        public VfsUser User;
        public VfsAccess Access;

        public HfsResponseVfs Clone()
        {
            return new HfsResponseVfs() { 
                VirtualPath = this.VirtualPath,
                PhysicalPath = this.PhysicalPath,
                Path = this.Path,
                User = this.User,
                Access = this.Access
            };
        }
    }
}