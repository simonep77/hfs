using System;


namespace Hfs.Server.CODICE.VFS
{
    /// <summary>
    /// Definisce la tipologia di azione possibile
    /// </summary>
    public enum VfsAction: byte 
    {
        None = 0x0,
        List = 0x1,
        Read = 0x2,
        Write = 0x4,
        Delete = 0x8,
        All = 0x0F
    }
}
