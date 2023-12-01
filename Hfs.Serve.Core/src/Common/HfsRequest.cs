using Hfs.Server.Core.FileHandling;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Web;

namespace Hfs.Server.Core.Common
{
    /// <summary>
    /// Parametri richiesta hfs
    /// </summary>
    public class HfsRequest
    {
        public bool Loaded;
        public string IP = string.Empty;
        public String ReqId = Guid.NewGuid().ToString();
        public string ActionStr = string.Empty;
        public string User = Const.VFS_USER_GUEST;
        public string Pass = string.Empty;
        public string Pattern = @"*";
        public string VPath = string.Empty;
        public string VPathDest = string.Empty;
        public string MailFrom = string.Empty;
        public string MailTo = string.Empty;
        public string MailSubj = string.Empty;
        public string MailBody = string.Empty;
        public string Attr = string.Empty;

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="context"></param>
        public void ReadFromContext(HttpContext context)
        {
            //Imposta IP
            this.IP = context.Connection.RemoteIpAddress?.ToString() ?? "localhost";

            //Legge headers
            this.readParams(context.Request.Headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.FirstOrDefault())));

            //Se azione non riconosciuta e abilitata query string allora legge
            if (!this.Loaded)
            {
                //Check presenza link a prescindere dalla disattivazione della QS
                if (!string.IsNullOrEmpty(context.Request.Query[Const.QS_LINK]))
                {
                    this.readParams(Utility.DecodeReadLink(context.Request.Query[Const.QS_LINK].FirstOrDefault()));
                    return;
                }

                //legge Querystring
                if (HfsData.AllowQueryStringParams)
                {
                    this.readParams(context.Request.Query.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.FirstOrDefault())));
                    //if (!this.Loaded)
                    //    throw new ArgumentException("Nessun parametro fornito (via HTTP Header o Querystring)");
                }
   
            }


        }


        /// <summary>
        /// Legge richiesta
        /// </summary>
        /// <param name="reqparams"></param>
        private void readParams(IEnumerable<KeyValuePair<string, String>> reqparams) 
        {
            foreach (var sKey in reqparams)
            {
                //Se non inizia con hfs skip
                if (sKey.Key == null || !sKey.Key.StartsWith(Const.QS_HEADER_PREFIX))
                    continue;

                string value = sKey.Value;

                switch (sKey.Key)
                {
                    case Const.QS_ACTION:
                        this.ActionStr = Utility.GetValue(value).ToLower();
                        break;
                    case Const.QS_USER:
                        this.User = Utility.GetValue(value, Const.VFS_USER_GUEST);
                        break;
                    case Const.QS_PASS:
                        this.Pass = value;
                        break;
                    case Const.QS_VPATH:
                        this.VPath = Utility.NormalizeVirtualPath(value);
                        break;
                    case Const.QS_VPATH_DEST:
                        this.VPathDest = Utility.NormalizeVirtualPath(value);
                        break;
                    case Const.QS_VPATH_PATTERN:
                        this.Pattern = Utility.GetValue(value, @"*");
                        break;
                    case Const.QS_MAIL_TO:
                        this.MailTo = Utility.GetValue(value);
                        break;
                    case Const.QS_MAIL_FROM:
                        this.MailFrom = Utility.GetValue(value);
                        break;
                    case Const.QS_MAIL_SUBJ:
                        this.MailSubj = Utility.GetValue(value);
                        break;
                    case Const.QS_MAIL_BODY:
                        this.MailBody = Utility.GetValue(value);
                        break;
                    case Const.QS_ATTR:
                        this.Attr = Utility.GetValue(value);
                        break;

                }
                //Imposta request come caricata (ovvero almeno un parametro hfs)
                this.Loaded = true;
            }

        }
    }
}