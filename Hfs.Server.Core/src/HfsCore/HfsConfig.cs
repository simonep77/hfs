using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.VFS;
using System;

namespace Hfs.Server
{
    public class HfsConfig
    {

        public static void Init(IConfiguration config)
        {
            //Il logger viene creato immdiatamente
            HfsData.AdminPass = config["Hfs:AdminPass"];
            HfsData.LogDir = config["Hfs:LogDirectory"];
            HfsData.VfsFilePath = config["Hfs:VfsFilePath"];
            HfsData.LogKeepDays = Convert.ToInt32(config["Hfs:LogKeepDays"]);
            HfsData.TempFilesKeepDays = Convert.ToInt32(config["Hfs:TempFilesKeepDays"]);
            HfsData.TempDir = config["Hfs:TempDirectory"];

            //inizializza log vari e avvia
            HfsData.Logger = new HfsAsyncLogger();
            HfsData.Logger.InitLog(ELogType.HfsGlobal, System.IO.Path.Combine(HfsData.LogDir, string.Format(Const.LOG_GLOBAL_FILENAME_FMT, DateTime.Now)));
            HfsData.Logger.InitLog(ELogType.HfsAccess, System.IO.Path.Combine(HfsData.LogDir, string.Format(Const.LOG_ACCESS_FILENAME_FMT, DateTime.Now)));
            HfsData.Logger.Start();

            //Scrive log base
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "");
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"HTTP File Server v{ApplicationInfo.FileVersion} - avvio applicazione");
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "");

            try
            {
                //Crea VFS e dati globali
                HfsData.Vfs = new Vfs(HfsData.VfsFilePath);
                HfsData.Stats = new HfsStatsCollectorEX();
                HfsData.TempDirUserFiles = System.IO.Path.Combine(HfsData.TempDir, @"users");
                HfsData.TempDirRemoteFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\remote");
                HfsData.TempDirConvertedFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\convert");
                HfsData.TempDirSharedFiles = System.IO.Path.Combine(HfsData.TempDir, @"system\shared");

                //Crea cartelle
                System.IO.Directory.CreateDirectory(HfsData.TempDir);
                System.IO.Directory.CreateDirectory(HfsData.TempDirRemoteFiles);
                System.IO.Directory.CreateDirectory(HfsData.TempDirConvertedFiles);
                System.IO.Directory.CreateDirectory(HfsData.TempDirSharedFiles);
                System.IO.Directory.CreateDirectory(HfsData.LogDir);

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
            HfsData.Logger.Stop();
            HfsData.Stats.Stop();
        }

    }
}