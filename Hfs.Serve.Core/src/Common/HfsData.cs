using Hfs.Server.Core.Vfs;
using System.Diagnostics;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Dati volatili applicazione
    /// </summary>
    public class HfsData
    {

        public static IWebHostEnvironment? HostingEnv { get; set; }
        public static WebApplication? WebApp { get; set; }

        public static string AdminPass = string.Empty;
        public static int LogKeepDays = 30;
        public static int TempFilesKeepDays = 30;
        public static bool LogAccess = false;
        public static bool AllowQueryStringParams = true;
        public static bool Debug = false;


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
        /// Directory per file temporanei condivisi
        /// </summary>
        public static string TempDirSharedFiles = string.Empty;


        /// <summary>
        /// Directory di log
        /// </summary>
        public static string LogDir = string.Empty;

        public static string VfsFilePath = string.Empty;


        /// <summary>
        /// Virtual file system
        /// </summary>
        public static VfsHandler Vfs;


        /// <summary>
        /// Statistiche
        /// </summary>
        public static HfsStatsCollectorEX Stats;


        /// <summary>
        /// Logger asincrono
        /// </summary>
        public static HfsAsyncLogger Logger;


        public static void Init()
        {
            if (WebApp is null || HostingEnv is null)
                throw new ArgumentException($"{nameof(WebApp)} e {nameof(HostingEnv)} devono essere valorizzati");

            VfsFilePath = string.IsNullOrWhiteSpace(WebApp?.Configuration["Hfs:VfsFilePath"]) ? Path.Combine(HostingEnv?.ContentRootPath, "data", "vfs.json") : WebApp?.Configuration["Hfs:VfsFilePath"];
            LogDir = string.IsNullOrWhiteSpace(WebApp.Configuration["Hfs:LogDirectory"])? Path.Combine(HfsData.HostingEnv.ContentRootPath, "data", "log") : WebApp.Configuration["Hfs:LogDirectory"];
            TempDir = string.IsNullOrWhiteSpace(WebApp.Configuration["Hfs:TempDirectory"]) ? Path.Combine(HfsData.HostingEnv.ContentRootPath, "data", "temp") : WebApp.Configuration["Hfs:TempDirectory"];
            LogKeepDays = Convert.ToInt32(WebApp.Configuration["Hfs:LogKeepDays"] ?? "30");
            TempFilesKeepDays = Convert.ToInt32(WebApp.Configuration["Hfs:TempFilesKeepDays"] ?? "30");
            LogAccess = Convert.ToBoolean(WebApp.Configuration["Hfs:LogAccess"] ?? "false");
            AllowQueryStringParams = Convert.ToBoolean(WebApp.Configuration["Hfs:AllowQueryStringParams"] ?? "true");
            Debug = Convert.ToBoolean(WebApp.Configuration["Hfs:Debug"] ?? "false");
            AdminPass = WebApp.Configuration["Hfs:AdminPass"] ?? "admin";

            internalInit();
        }

        private static void internalInit()
        {
            Directory.CreateDirectory(HfsData.LogDir);
            //inizializza log vari e avvia
            HfsData.Logger = new HfsAsyncLogger();
            HfsData.Logger.InitLog(ELogType.HfsGlobal, System.IO.Path.Combine(HfsData.LogDir, string.Format(Const.LOG_GLOBAL_FILENAME_FMT, DateTime.Now)));
            HfsData.Logger.InitLog(ELogType.HfsAccess, System.IO.Path.Combine(HfsData.LogDir, string.Format(Const.LOG_ACCESS_FILENAME_FMT, DateTime.Now)));
            HfsData.Logger.Start();

            //Scrive log base
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "");
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_START_LINE, ApplicationInfo.FileVersion);
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "");

            try
            {
                //Crea VFS e dati globali
                HfsData.Vfs = new VfsHandler(HfsData.VfsFilePath);
                HfsData.Stats = new HfsStatsCollectorEX();
                HfsData.TempDirUserFiles = System.IO.Path.Combine(HfsData.TempDir, @"users");
                HfsData.TempDirRemoteFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\remote");
                HfsData.TempDirSharedFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\shared");

                //Crea cartelle
                System.IO.Directory.CreateDirectory(HfsData.LogDir);
                System.IO.Directory.CreateDirectory(HfsData.TempDir);
                System.IO.Directory.CreateDirectory(HfsData.TempDirRemoteFiles);
                System.IO.Directory.CreateDirectory(HfsData.TempDirSharedFiles);

                //Avvia task
                HfsTaskHandler.Start();

                //Carica e aggiunge
                HfsData.Vfs.Load();
            }
            catch (Exception ex)
            {
                //Logga eventuale eccezione
                HfsData.Logger.WriteException(ELogType.HfsGlobal, ex);
            }
        }


        public static void Stop()
        {
            HfsData.Logger.Stop(true);
        }


    }
}