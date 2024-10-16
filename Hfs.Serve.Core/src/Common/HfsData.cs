using FluentScheduler;
using Hfs.Server.Core.Vfs;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Dati volatili applicazione
    /// </summary>
    public class HfsData
    {

        public static WebApplication? WebApp { get; set; }

        public static string AdminPass = string.Empty;
        public static int TempFilesKeepDays = 30;
        public static bool LogAccess = false;
        public static bool AllowQueryStringParams = true;
        public static bool Debug = false;


        public static IMemoryCache ApplicationCache { get; } = new MemoryCache(new MemoryCacheOptions
        {
            ExpirationScanFrequency = TimeSpan.FromMinutes(10),
            SizeLimit = 2000,
            CompactionPercentage = 0.30,
        });

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


        public static string VfsFilePath = string.Empty;


        /// <summary>
        /// Virtual file system
        /// </summary>
        public static VfsHandler Vfs;


        /// <summary>
        /// Statistiche
        /// </summary>
        public static HfsStatsCollectorEX Stats;



        public static void Init()
        {
            if (WebApp is null)
                throw new ArgumentException($"{nameof(WebApp)} deve essere valorizzata");

            VfsFilePath = Environment.GetEnvironmentVariable("VfsFilePath") ?? WebApp.Configuration["Hfs:TempDirectory"] ?? Path.Combine(HfsData.WebApp.Environment.ContentRootPath, "data", "vfs.json");
            TempDir = Environment.GetEnvironmentVariable("TempDirectory") ?? WebApp.Configuration["Hfs:TempDirectory"] ?? Path.Combine(HfsData.WebApp.Environment.ContentRootPath, "data", "temp");
            TempFilesKeepDays = Convert.ToInt32(Environment.GetEnvironmentVariable("TempFilesKeepDays") ?? WebApp.Configuration["Hfs:TempFilesKeepDays"] ?? "30");
            LogAccess = Convert.ToBoolean(Environment.GetEnvironmentVariable("LogAccess") ?? WebApp.Configuration["Hfs:LogAccess"] ?? "true");
            AllowQueryStringParams = Convert.ToBoolean(Environment.GetEnvironmentVariable("AllowQueryStringParams") ?? WebApp.Configuration["Hfs:AllowQueryStringParams"] ?? "true");
            Debug = Convert.ToBoolean(Environment.GetEnvironmentVariable("Debug") ?? WebApp.Configuration["Hfs:TempDireDebugctory"] ?? "false");
            AdminPass = Environment.GetEnvironmentVariable("AdminPass") ?? WebApp.Configuration["Hfs:AdminPass"] ?? "admin";

            internalInit();
        }

        #region Console Log Async
        private static readonly object _LogLock = new object();
        private static StringBuilder _logSb = new StringBuilder(1024 * 1024);

        /// <summary>
        /// Appende allo string builder un testo in notazione log
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sb"></param>
        public static void AppendLog(string text, StringBuilder sb)
        {
            sb.AppendLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} - T{Thread.CurrentThread.ManagedThreadId:00000} - " + text);
        }

        /// <summary>
        /// Accoda uno stringbuilder nel log senza formattazione
        /// </summary>
        /// <param name="sb"></param>
        public static void WriteLog(StringBuilder sb)
        {
            lock (_LogLock)
            {
                _logSb.Append(sb);
            }
        }


        /// <summary>
        /// Scrive riga di log con data e ora
        /// </summary>
        /// <param name="text"></param>
        public static void WriteLog(string text)
        {
            lock (_LogLock)
            {
                AppendLog(text, _logSb);
            }
        }


        public static void WriteException(Exception e)
        {
            StringBuilder sb = new StringBuilder(1500);
            //Scrive
            Exception? oException;
            int iIndentEx = 0;
            int iInnerCount = 0;

            AppendLog(Const.LOG_SEPARATOR, sb);

            oException = e;

            while (oException != null)
            {
                string sIndent = string.Empty.PadRight(iIndentEx);

                if (iIndentEx == 0)
                    AppendLog($"{sIndent}ECCEZIONE!", sb);
                else
                    AppendLog($"{iInnerCount:D2}) Inner", sb);

                AppendLog($"  * Tipo     : {sIndent}{oException.GetType().Name}", sb);
                AppendLog($"  * Messaggio: {sIndent}" + oException.Message, sb);
                AppendLog($"  * Source   : {sIndent}{oException.Source}", sb);
                AppendLog($"  * Classe   : {sIndent}{oException.TargetSite?.DeclaringType?.Name}", sb);
                AppendLog($"  * Metodo   : {sIndent}{oException.TargetSite?.Name}", sb);
                AppendLog($"  * Namespace: {sIndent}{oException.TargetSite?.DeclaringType?.Namespace}", sb);
                AppendLog($"  * Stack    : {sIndent}" + oException.StackTrace, sb);

                //Successiva
                iInnerCount++;
                oException = oException.InnerException;
                iIndentEx += 4;
            }

            AppendLog(Const.LOG_SEPARATOR, sb);

            //Accoda a log senza format
            WriteLog(sb);
        }

        /// <summary>
        /// Alloca nuovo log buffer e scrive su stdout
        /// </summary>
        /// <param name="text"></param>
        private static void FlushLog()
        {
            //Crea nuovo buffer e lo assegna ritornando il vecchio da scrivere
            var _oldSb = (StringBuilder)Interlocked.Exchange(ref _logSb, new StringBuilder(1024 * 1024));

            Console.Out.Write(_oldSb);
            _oldSb.Length = 0;
            _oldSb.Capacity = 0;
        }


        #endregion




        private static void internalInit()
        {
            //Avvia job di scrittura console log
            JobManager.AddJob(() => FlushLog(), s => s.ToRunEvery(5).Seconds());

            //Scrive log base
            WriteLog("");
            WriteLog(Const.LOG_SEPARATOR);
            WriteLog($"HTTP File Server v{ApplicationInfo.FileVersion} - avvio applicazione");
            WriteLog(Const.LOG_SEPARATOR);
            WriteLog("");

            try
            {
                //Crea VFS e dati globali
                HfsData.Vfs = new VfsHandler(HfsData.VfsFilePath);
                HfsData.Stats = new HfsStatsCollectorEX();
                HfsData.TempDirUserFiles = System.IO.Path.Combine(HfsData.TempDir, @"users");
                HfsData.TempDirRemoteFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\remote");
                HfsData.TempDirSharedFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\shared");

                //Crea cartelle
                System.IO.Directory.CreateDirectory(HfsData.TempDir);
                System.IO.Directory.CreateDirectory(HfsData.TempDirRemoteFiles);
                System.IO.Directory.CreateDirectory(HfsData.TempDirSharedFiles);

                //Avvia task pulizia temporanei ad intervalli
                JobManager.AddJob(() =>
                {
                    //Log avvio
                    HfsData.WriteLog("Avvio pulizia temporanei..");

                    //Pulisce
                    DateTime dtExpired = DateTime.Today.AddDays(1).AddSeconds(-1).AddDays(-HfsData.TempFilesKeepDays);
                    Utility.CleanDirectory(HfsData.TempDir, @"*", dtExpired, true);

                    //Log fine
                    HfsData.WriteLog("Fine pulizia temporanei");
                }, s => s.ToRunEvery(1).Days().At(23, 50));

                //Carica e aggiunge
                HfsData.Vfs.Load();
            }
            catch (Exception ex)
            {
                //Logga eventuale eccezione
                WriteException(ex);
            }
        }


        public static void Stop()
        {
            JobManager.StopAndBlock();
        }


    }
}