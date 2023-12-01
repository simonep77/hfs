using Hfs.Server.Core.Common;
using System;


namespace Hfs.Server.Core.Vfs
{

    /// <summary>
    /// Classi per la deserializzazione del VFS
    /// </summary>
    public class VfsModelRoot
    {
        public VfsModelUser[] users { get; set; }
        public VfsModelPath[] paths { get; set; }
    }

    public class VfsModelUser
    {
        public string name { get; set; }
        public string pass { get; set; }
    }

    public class VfsModelPath
    {
        public string type { get; set; }
        public string virtualpath { get; set; }
        public string physicalpath { get; set; }
        public VfsModelAuth[] authorizations { get; set; }
        public VfsModelParam[] parameters { get; set; }
    }

    public class VfsModelAuth
    {
        public string name { get; set; }
        public string access { get; set; }
    }

    public class VfsModelParam
    {
        public string key { get; set; }
        public string value { get; set; }
    }


}
