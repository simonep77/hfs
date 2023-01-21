using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using System.Threading;
using Hfs.Server.HfsCore.Commands;
using System.Reflection;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Classe per la collezione di statistiche di utilizzo
    /// </summary>
    public class HfsStatsCollectorEX : IDisposable
    {
        private const int I_SAVE_TIMER = 30000;
        private const string S_XPATH_STAT_FMT = @"//item[name='{0}']/value";
        private const string S_DATE_FMT = @"dd/MM/yyyy HH:mm:ss";
        private const string S_TAG_SENT = @"TotalBytesSent";
        private const string S_TAG_RECV = @"TotalBytesReceived";
        private const string S_TAG_START = @"DateStartStats";
        private const string S_TAG_UPD = @"DateLastUpdate";

        private Dictionary<string, StatItem> mStats;

        private long mBytesReceived = 0L;
        private long mBytesSent = 0L;
        private long mChanges = 0L;
        private string mStatFile;
        private DateTime mDtStartStats = DateTime.Now;
        private Timer mTimer;

        /// <summary>
        /// Classe interna per la gestione delle statistiche
        /// </summary>
        private class StatItem
        {
            public string Action;
            public long Value;
        }

        public HfsStatsCollectorEX()
        {


            this.mStats = new Dictionary<string, StatItem>(CommandFactory.AvailableCommands.Count);
            this.mStatFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "App_Data", @"hfs_stats.xml");

            //Carica le enum base
            foreach (var tpl in CommandFactory.AvailableCommands)
            {
                StatItem item = new StatItem();
                item.Action = tpl.Key;
                this.mStats.Add(tpl.Key, item);
            }

            //Se abilitato carica da file
            try
            {
                this.loadXmlStats();

                //Crea thread di salvataggio
                this.mTimer = new Timer((s) => timerSaveStats(),
                                                null,
                                                TimeSpan.FromSeconds(I_SAVE_TIMER),
                                                TimeSpan.FromSeconds(I_SAVE_TIMER));
            }
            catch (Exception e)
            {
                //Catturata
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"Errore nell'avvio delle statistiche: {e.Message}");
            }


        }

        /// <summary>
        /// Segnala esecuzione azione
        /// </summary>
        /// <param name="action"></param>
        public void ReportAction(ICommand cmd)
        {
            //Aggiorna dati
            Interlocked.Add(ref this.mBytesReceived, cmd.BytesReceived);
            Interlocked.Add(ref this.mBytesSent, cmd.BytesSent);
            Interlocked.Increment(ref this.mStats[cmd.ActionKey].Value);
            Interlocked.Increment(ref this.mChanges);
        }


        /// <summary>
        /// Esegue reset delle statistiche (anche dello storico)
        /// </summary>
        public void ResetStats()
        {
            //Carica le enum base
            foreach (var item in this.mStats)
            {
                Interlocked.Exchange(ref item.Value.Value, 0L);
            }

            Interlocked.Exchange(ref this.mBytesReceived, 0L);
            Interlocked.Exchange(ref this.mBytesSent, 0L);
            Interlocked.Exchange(ref this.mChanges, 0L);

            this.saveXmlStats();
        }

        /// <summary>
        /// Ritorna stringa con statistiche
        /// </summary>
        /// <returns></returns>
        public string GetXmlStats()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(1000);
            sb.Append("<stats>");

            foreach (var pair in this.mStats)
            {
                sb.Append(@"<item><name>");
                sb.Append(pair.Value.Action);
                sb.Append(@"</name><value>");
                sb.Append(Interlocked.Read(ref pair.Value.Value).ToString());
                sb.Append(@"</value></item>");
            }
            sb.Append(@"<item><name>");
            sb.Append(S_TAG_RECV);
            sb.Append(@"</name><value>");
            sb.Append(Interlocked.Read(ref this.mBytesReceived).ToString());
            sb.Append(@"</value></item>");
            sb.Append(@"<item><name>");
            sb.Append(S_TAG_SENT);
            sb.Append(@"</name><value>");
            sb.Append(Interlocked.Read(ref this.mBytesSent).ToString());
            sb.Append(@"</value></item>");
            sb.Append(@"<item><name>");
            sb.Append(S_TAG_START);
            sb.Append(@"</name><value>");
            sb.Append(this.mDtStartStats.ToString(S_DATE_FMT));
            sb.Append(@"</value></item>");
            sb.Append(@"<item><name>");
            sb.Append(S_TAG_UPD);
            sb.Append(@"</name><value>");
            sb.Append(DateTime.Now.ToString(S_DATE_FMT));
            sb.Append(@"</value></item>");

            sb.Append("</stats>");

            return sb.ToString();
        }

        /// <summary>
        /// Carica i dati dal file xml
        /// </summary>
        private void loadXmlStats()
        {
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "Inzio caricamento statistiche..");
            try
            {
                //Se il file non esiste esce
                if (!File.Exists(this.mStatFile))
                    return;

                //Carica xml
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(this.mStatFile);

                //Carica in base al nome azione
                foreach (var stat in this.mStats)
                {
                    XmlNode xNode = xDoc.DocumentElement.SelectSingleNode(string.Format(S_XPATH_STAT_FMT, stat.Key));

                    if (xNode == null)
                        continue;

                    Interlocked.Exchange(ref stat.Value.Value, Convert.ToInt64(xNode.InnerText));
                }

                //Carica i bytes
                XmlNode xNode2 = xDoc.DocumentElement.SelectSingleNode(string.Format(S_XPATH_STAT_FMT, S_TAG_RECV));
                if (xNode2 != null)
                    Interlocked.Exchange(ref this.mBytesReceived, Convert.ToInt64(xNode2.InnerText));

                xNode2 = xDoc.DocumentElement.SelectSingleNode(string.Format(S_XPATH_STAT_FMT, S_TAG_SENT));
                if (xNode2 != null)
                    Interlocked.Exchange(ref this.mBytesSent, Convert.ToInt64(xNode2.InnerText));

                xNode2 = xDoc.DocumentElement.SelectSingleNode(string.Format(S_XPATH_STAT_FMT, S_TAG_START));
                if (xNode2 != null)
                {
                    this.mDtStartStats = DateTime.ParseExact(xNode2.InnerText, S_DATE_FMT, new System.Globalization.CultureInfo("it-IT"));
                }
                else
                {
                    this.mDtStartStats = DateTime.Now;
                }

                xDoc = null;

                //Imposta flag di dati NON modificati
                Interlocked.Exchange(ref this.mChanges, 0L);
            }
            finally
            {
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "Fine caricamento statistiche");
            }

        }


        /// <summary>
        /// Salva le statistiche su file
        /// </summary>
        private void saveXmlStats()
        {
            if (Interlocked.Read(ref this.mChanges) == 0L)
                return;

            File.WriteAllText(this.mStatFile, this.GetXmlStats());

            //Imposta flag di dati NON modificati
            Interlocked.Exchange(ref this.mChanges, 0L);

        }


        /// <summary>
        /// Procedura eseguita dal thread di salvataggio
        /// </summary>
        private void timerSaveStats()
        {
            try
            {
                this.saveXmlStats();
            }
            catch (ThreadAbortException)
            {
                //Non fa nulla: thread terminato
            }

        }


        public void Stop()
        {
            this.mTimer?.Dispose();
            //Salva i dati
            this.saveXmlStats();
        }


        #region IDisposable Membri di

        public void Dispose()
        {
            this.Stop();
        }

        #endregion
    }
}