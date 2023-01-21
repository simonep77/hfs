using System;
using System.Collections.Generic;
using System.Web;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Classe di ritorno informazioni su path utente
    /// </summary>
    public class HfsResponseVfs
    {
        public string VirtualPath;
        public string PhysicalPath;
        public string FileConvertType;
        public VfsPath Path;
        public VfsUser User;
        public VfsAccess Access;

        public HfsResponseVfs Clone()
        {
            return new HfsResponseVfs() { 
                VirtualPath = this.VirtualPath,
                PhysicalPath = this.PhysicalPath,
                FileConvertType = this.FileConvertType,
                Path = this.Path,
                User = this.User,
                Access = this.Access
            };
        }
    }
}