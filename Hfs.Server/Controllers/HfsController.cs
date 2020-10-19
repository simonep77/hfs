using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.CLASSI.FileHandling;
using Hfs.Server.CODICE.VFS;
using Hfs.Server.HfsCore.Commands;

namespace Hfs.Server.Controllers
{
    public class HfsController : Controller
    {

        // GET: Hfs
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task Index()
        {

           

            try
            {
                //Variabili di utilizzo comune
                using (ICommand cmd = CommandFactory.Create(this))
                {
                    try
                    {
                        //Esegue
                        await cmd.Execute();

                        //Debug richiesta
                        if (Properties.Settings.Default.Debug)
                            this.logDump(this.HttpContext, "DEBUG Richiesta", 0, string.Empty, cmd.Request, cmd.Response);
                    }
                    catch (Exception ex)
                    {
                        //Imposta codice generico
                        HfsException e1 = ex as HfsException;
                        EStatusCode code = e1 == null ? EStatusCode.GenericError : e1.StatusCode;

                        ICommand cmdErr = new CommandError() { Code = code, Msg = ex.Message, Content = Utility.GetHelpHtml() };
                        cmdErr.Init(this, cmd.Request);
                        await cmdErr.Execute();

                        //Logga eccezione
                        this.logDump(this.HttpContext, "Richiesta terminata con ERRORE", (int)code, ex.Message, cmd.Request, cmd.Response);
                    }
                    finally
                    {
                        //Aggiorna statistiche
                        HfsData.Stats.ReportAction(cmd);
                    }
                }
            }
            catch (Exception ex)
            {
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"Errore generato al di fuori del contesto command: {ex.Message}");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, ex.StackTrace);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);

                ICommand cmdErr = new CommandError() { Code = EStatusCode.GenericError, Msg = ex.Message, Content = @"Errore generato al di fuori del contesto command" };
                cmdErr.Init(this, new HfsRequest());
                await cmdErr.Execute();
            }


        }



        /// <summary>
        /// Esegue un dump del contesto
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="sPhysicalPath"></param>
        private void logDump(HttpContextBase context, string title, int code, string msg, HfsRequest req, HfsResponseVfs resp)
        {
            //Logga eccezione
            HfsData.Logger.BeginWrite(ELogType.HfsGlobal);
            try
            {
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, title);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" RequestID: {0}", req.ReqId);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri HFS-HEADER");

                for (int i = 0; i < context.Request.Headers.Count; i++)
                {
                    if (context.Request.Headers.Keys[i].StartsWith(Const.QS_HEADER_PREFIX))
                    {
                        HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - {0}: '{1}'", context.Request.Headers.Keys[i], context.Request.Headers[i]);
                    }
                }
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri HFS-QUERYSTRING");

                for (int i = 0; i < context.Request.QueryString.Count; i++)
                {
                    if (context.Request.QueryString.Keys[i] != null && context.Request.QueryString.Keys[i].StartsWith(Const.QS_HEADER_PREFIX))
                    {
                        HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - {0}: '{1}'", context.Request.QueryString.Keys[i], context.Request.QueryString[i]);
                    }
                }

                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri AMBIENTE");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - file/dir: '{0}'", (resp != null) ? resp.PhysicalPath : @"unknown");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - clientHost: {0}", context.Request.UserHostName);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - clientIp: {0}", context.Request.UserHostAddress);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - clientAgent: {0}", context.Request.UserAgent);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - browser: {0}", context.Request.Browser.Browser);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - datalen: '{0}'", context.Request.ContentLength.ToString());
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri RISPOSTA");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - code: {0}", code.ToString("D"));
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @"   - msg: {0}", msg);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            }
            finally
            {
                HfsData.Logger.EndWrite(ELogType.HfsGlobal);
            }

        }

        


    }
}