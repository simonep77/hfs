
namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Enum che definisce le azioni
    /// </summary>
    public enum EVfsPathType
    {
        /// <summary>
        /// Indica che il path e' locale (file system)
        /// </summary>
        Local = 1,

        /// <summary>
        /// Indica che il path e' un HFS remoto -> serve 2 server
        /// </summary>
        Hfs = 2,

        /// <summary>
        /// Indica che il path e' un SFTP remoto
        /// </summary>
        SFtp = 3,

        /// <summary>
        /// Indica che il path e' un FTP remoto
        /// </summary>
        Ftp = 4

    }
}
