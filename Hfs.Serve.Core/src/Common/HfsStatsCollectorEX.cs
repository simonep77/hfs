using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Xml;
using System.Threading;
using Hfs.Server.HfsCore.Commands;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Classe per la collezione di statistiche di utilizzo
    /// </summary>
    public class HfsStatsCollectorEX
    {
        private const string S_DATE_FMT = @"dd/MM/yyyy HH:mm:ss";
        private const string S_TAG_SENT = @"TotalBytesSent";

        private Dictionary<string, StatItem> mStats;
        private long mBytesReceived = 0L;
        private long mBytesSent = 0L;

        /// <summary>
        /// Classe interna per la gestione delle statistiche
        /// </summary>
        private class StatItem
        {
            public string? Action;
            public long Value;
        }


        public HfsStatsCollectorEX()
        {
            this.mStats = new Dictionary<string, StatItem>(CommandFactory.AvailableCommands.Count);

            //Carica le enum base
            foreach (var tpl in CommandFactory.AvailableCommands)
            {
                this.mStats.Add(tpl.Key, new StatItem { Action = tpl.Key });
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


        }


        /// <summary>
        /// Ritorna stringa con statistiche
        /// </summary>
        /// <returns></returns>
        public string GetXmlStats()
        {
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                Commands = this.mStats.Values.Select(x => new { x.Action, x.Value}),
                TotBytesReceived = this.mBytesReceived,
                TotBytesSent = this.mBytesSent,
            });

            
        }


    }
}