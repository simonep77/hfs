using System;
using System.Threading;

namespace Hfs.Server.Core.Common
{
    public class HfsTaskHandler
    {

        /// <summary>
        /// Avvia tutti i task definiti
        /// </summary>
        public static void Start()
        {
            //File Temporanei
            new Thread(new ParameterizedThreadStart(handleTempFiles)).Start(null);
            //File di log
            new Thread(new ParameterizedThreadStart(handleLogFiles)).Start(null);
        }

        /// <summary>
        /// Gestione file temporanei
        /// </summary>
        private static void handleTempFiles(object arg)
        {
            //Log avvio
            HfsData.WriteLog("Avvio pulizia temporanei..");

            //Pulisce
            DateTime dtNow = DateTime.Now;
            DateTime dtExpired = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 23, 59, 59).AddDays(-HfsData.TempFilesKeepDays);
            Utility.CleanDirectory(HfsData.TempDir, @"*", dtExpired, true);

            //Log fine
            HfsData.WriteLog("Fine pulizia temporanei");
        }

        /// <summary>
        /// Gestione file di log
        /// </summary>
        /// <param name="stateInfo"></param>
        private static void handleLogFiles(object arg)
        {
            //Log avvio
            HfsData.WriteLog("Avvio pulizia log..");

            //Pulisce
            DateTime dtNow = DateTime.Now;
            DateTime dtExpired = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day, 23, 59, 59).AddDays(-HfsData.LogKeepDays);
            Utility.CleanDirectory(HfsData.LogDir, Const.LOG_PATTERN, dtExpired, false);

            //Log fine
            HfsData.WriteLog("Fine pulizia log");
        }


    }
}