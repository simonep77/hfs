using System;
using System.Collections.Generic;
using System.Web;

namespace Hfs.Server.CODICE.CLASSI
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
        public FileBlock Block = Const.BLOCK_DEFAULT;
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
            this.IP = context.Connection.RemoteIpAddress.ToString();

            //Legge headers
            this.readParams(context.Request.Headers.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.FirstOrDefault() ?? "")));

            //Se azione non riconosciuta e abilitata query string allora legge
            if (!this.Loaded)
            {
                //Check presenza link a prescindere dalla disattivazione della QS
                if (context.Request.Query[Const.QS_LINK].Any())
                {
                    this.readParams(Utility.DecodeReadLink(context.Request.Query[Const.QS_LINK].ToString()));
                    return;
                }

                //legge Querystring
                if (true)
                {
                    this.readParams(context.Request.Query.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.ToString())));
                    //if (!this.Loaded)
                    //    throw new ArgumentException("Nessun parametro fornito (via HTTP Header o Querystring)");
                }
   
            }


        }


        /// <summary>
        /// Legge richiesta
        /// </summary>
        /// <param name="reqparams"></param>
        private void readParams(IEnumerable<KeyValuePair<string, string>> input) 
        {
            foreach (var pair in input)
            {
                //Se non inizia con hfs skip
                if (!pair.Key.StartsWith(Const.QS_HEADER_PREFIX))
                    continue;

                switch (pair.Key)
                {
                    case Const.QS_ACTION:
                        this.ActionStr = Utility.GetValue(pair.Value).ToLower();
                        break;
                    case Const.QS_USER:
                        this.User = Utility.GetValue(pair.Value, Const.VFS_USER_GUEST);
                        break;
                    case Const.QS_PASS:
                        this.Pass = pair.Value;
                        break;
                    case Const.QS_VPATH:
                        this.VPath = Utility.NormalizeVirtualPath(pair.Value);
                        break;
                    case Const.QS_VPATH_DEST:
                        this.VPathDest = Utility.NormalizeVirtualPath(pair.Value);
                        break;
                    case Const.QS_VPATH_PATTERN:
                        this.Pattern = Utility.GetValue(pair.Value, @"*");
                        break;
                    case Const.QS_READ_BLOCK:
                        this.Block = Utility.DecodeQsRBlock(pair.Value);
                        break;
                    case Const.QS_MAIL_TO:
                        this.MailTo = Utility.GetValue(pair.Value);
                        break;
                    case Const.QS_MAIL_FROM:
                        this.MailFrom = Utility.GetValue(pair.Value);
                        break;
                    case Const.QS_MAIL_SUBJ:
                        this.MailSubj = Utility.GetValue(pair.Value);
                        break;
                    case Const.QS_MAIL_BODY:
                        this.MailBody = Utility.GetValue(pair.Value);
                        break;
                    case Const.QS_ATTR:
                        this.Attr = Utility.GetValue(pair.Value);
                        break;

                }
                //Imposta request come caricata (ovvero almeno un parametro hfs)
                this.Loaded = true;
            }

        }
    }
}