using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Dati volatili applicazione
    /// </summary>
    public class HfsData
    {

        /// <summary>
        /// Directory base di lavoro hfs
        /// </summary>
        public static string TempDir = string.Empty;


        /// <summary>
        /// Directory file temporanei utente
        /// </summary>
        public static string TempDirUserFiles = string.Empty;

        /// <summary>
        /// Directory file temporanei remoti
        /// </summary>
        public static string TempDirRemoteFiles = string.Empty;


        /// <summary>
        /// Directory file temporanei convertiti
        /// </summary>
        public static string TempDirConvertedFiles = string.Empty;


        /// <summary>
        /// Directory per file temporanei condivisi
        /// </summary>
        public static string TempDirSharedFiles = string.Empty;


        /// <summary>
        /// Directory di log
        /// </summary>
        public static string LogDir = string.Empty;


        /// <summary>
        /// Virtual file system
        /// </summary>
        public static Vfs Vfs;


        /// <summary>
        /// Statistiche
        /// </summary>
        public static HfsStatsCollectorEX Stats;


        /// <summary>
        /// Logger asincrono
        /// </summary>
        public static HfsAsyncLogger Logger;


 
    }
}