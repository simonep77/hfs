using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.HfsCore.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace Hfs.Server.Core.Controllers
{
    public class HfsController : Controller
    {
        [HttpGet, HttpPost]
        public async void Index()
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

                        ////Debug richiesta
                        //if (HfsConfig.)
                        //    this.logDump(this.HttpContext, "DEBUG Richiesta", 0, string.Empty, cmd.Request, cmd.Response);
                    }
                    catch (Exception ex)
                    {
                        //Imposta codice generico
                        EStatusCode code = (ex as HfsException)?.StatusCode ?? EStatusCode.GenericError;

                        var sbMessage = new StringBuilder();

                        while (ex != null)
                        {
                            sbMessage.Append(ex.Message);
                            sbMessage.Append(". ");
                            ex = ex.InnerException;
                        }

                        ICommand cmdErr = new CommandError() { Code = code, Msg = sbMessage.ToString(), Content = Utility.GetHelpHtml() };
                        cmdErr.Init(this, cmd.Request);
                        await cmdErr.Execute();

                        //Logga eccezione
                        this.logDump(this.Request, "Richiesta terminata con ERRORE", (int)code, sbMessage.ToString(), cmd.Request, cmd.Response);

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
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, ex.StackTrace ?? string.Empty);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);

                ICommand cmdErr = new CommandError() { Code = EStatusCode.GenericError, Msg = ex.Message, Content = @"Errore generato al di fuori del contesto command" };
                cmdErr.Init(this, new HfsRequest());
                await cmdErr.Execute();
            }
            finally
            {
                await this.Response.CompleteAsync();
            }

        }





        /// <summary>
        /// Esegue un dump del contesto
        /// </summary>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <param name="msg"></param>
        /// <param name="sPhysicalPath"></param>
        private void logDump(HttpRequest httpReq, string title, int code, string msg, HfsRequest req, HfsResponseVfs resp)
        {
            
            //Logga eccezione
            HfsData.Logger.BeginWrite(ELogType.HfsGlobal);
            try
            {
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, title);
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $@" RequestID: {req.ReqId}");

                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri HFS-HEADER");
                foreach (var item in httpReq.Headers.Where(x => x.Key.StartsWith(Const.QS_HEADER_PREFIX, StringComparison.Ordinal)))
                {
                    HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - {item.Key}: '{item.Value}'");
                }

                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, @" Parametri HFS-QUERYSTRING");
                foreach (var item in httpReq.Query.Where(x => x.Key.StartsWith(Const.QS_HEADER_PREFIX, StringComparison.Ordinal)))
                {
                    HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - {item.Key}: '{item.Value}'");
                }

                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $" Parametri AMBIENTE");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - file/dir: '{resp?.PhysicalPath ?? @"unknown"}'");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - clientIp: {httpReq.HttpContext.Connection.RemoteIpAddress?.ToString()}");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - clientAgent: {httpReq.Headers[HeaderNames.UserAgent]}");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - datalen: '{httpReq.ContentLength}'");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $" Parametri RISPOSTA");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - code: {code}");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"   - msg: {msg}");
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, Const.LOG_SEPARATOR);
            }
            finally
            {
                HfsData.Logger.EndWrite(ELogType.HfsGlobal);
            }

        }


    }
}
