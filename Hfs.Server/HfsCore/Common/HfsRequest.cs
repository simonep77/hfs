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
        public void ReadFromContext(HttpContextBase context)
        {
            //Imposta IP
            this.IP = context.Request.UserHostAddress;

            //Legge headers
            this.readParams(context.Request.Headers);

            //Se azione non riconosciuta e abilitata query string allora legge
            if (!this.Loaded)
            {
                //Check presenza link a prescindere dalla disattivazione della QS
                if (!string.IsNullOrEmpty(context.Request.QueryString[Const.QS_LINK]))
                {
                    this.readParams(Utility.DecodeReadLink(context.Request.QueryString[Const.QS_LINK]));
                    return;
                }

                //legge Querystring
                if (Properties.Settings.Default.AllowQueryStringParams)
                {
                    this.readParams(context.Request.QueryString);
                    //if (!this.Loaded)
                    //    throw new ArgumentException("Nessun parametro fornito (via HTTP Header o Querystring)");
                }
   
            }


        }


        /// <summary>
        /// Legge richiesta
        /// </summary>
        /// <param name="reqparams"></param>
        private void readParams(System.Collections.Specialized.NameValueCollection reqparams) 
        {
            foreach (string sKey in reqparams.AllKeys)
            {
                //Se non inizia con hfs skip
                if (sKey == null || !sKey.StartsWith(Const.QS_HEADER_PREFIX))
                    continue;

                switch (sKey)
                {
                    case Const.QS_ACTION:
                        this.ActionStr = Utility.GetValue(reqparams[Const.QS_ACTION]).ToLower();
                        break;
                    case Const.QS_USER:
                        this.User = Utility.GetValue(reqparams[Const.QS_USER], Const.VFS_USER_GUEST);
                        break;
                    case Const.QS_PASS:
                        this.Pass = reqparams[Const.QS_PASS];
                        break;
                    case Const.QS_VPATH:
                        this.VPath = Utility.NormalizeVirtualPath(reqparams[Const.QS_VPATH]);
                        break;
                    case Const.QS_VPATH_DEST:
                        this.VPathDest = Utility.NormalizeVirtualPath(reqparams[Const.QS_VPATH_DEST]);
                        break;
                    case Const.QS_VPATH_PATTERN:
                        this.Pattern = Utility.GetValue(reqparams[Const.QS_VPATH_PATTERN], @"*");
                        break;
                    case Const.QS_READ_BLOCK:
                        this.Block = Utility.DecodeQsRBlock(reqparams[Const.QS_READ_BLOCK]);
                        break;
                    case Const.QS_MAIL_TO:
                        this.MailTo = Utility.GetValue(reqparams[Const.QS_MAIL_TO]);
                        break;
                    case Const.QS_MAIL_FROM:
                        this.MailFrom = Utility.GetValue(reqparams[Const.QS_MAIL_FROM]);
                        break;
                    case Const.QS_MAIL_SUBJ:
                        this.MailSubj = Utility.GetValue(reqparams[Const.QS_MAIL_SUBJ]);
                        break;
                    case Const.QS_MAIL_BODY:
                        this.MailBody = Utility.GetValue(reqparams[Const.QS_MAIL_BODY]);
                        break;
                    case Const.QS_ATTR:
                        this.Attr = Utility.GetValue(reqparams[Const.QS_ATTR]);
                        break;

                }
                //Imposta request come caricata (ovvero almeno un parametro hfs)
                this.Loaded = true;
            }

        }
    }
}