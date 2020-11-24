using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Hfs.Client.Utility
{
    /// <summary>
    /// Override webclient per timeout gestito
    /// </summary>
    public class WebClientEx: WebClient
    {
        public int TimeoutMsec { get; set; } = 3 * 60 * 1000; //3 minuti

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = this.TimeoutMsec;
            return w;
        }

    }
}
