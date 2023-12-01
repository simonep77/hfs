using Hfs.Server.Core.Common;
using Hfs.Server.HfsCore.Commands;
using System.Text;

namespace Hfs.Serve.Core.src.Handler
{

    //public class HfsParams
    //{
    //    public string Hfs_User { get; set; }
    //    public string Hfs_Pass { get; set; }
    //    public string Hfs_Action { get; set; }
    //    public string Hfs_Vpath { get; set; }
    //    public string Hfs_VpathDest { get; set; }
    //    public string Hfs_Link { get; set; }
    //    public string Hfs_Block { get; set; }
    //    public string Hfs_Pattern { get; set; }
    //    public string Hfs_MailTo { get; set; }
    //    public string Hfs_MailFrom { get; set; }
    //    public string Hfs_MailSubj { get; set; }
    //}

    public class HfsHandler
    {
        /// <summary>
        /// Metodo base gestione hfs
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async static Task Handle(HttpContext context)
        {
            try
            {
                //Variabili di utilizzo comune
                using (ICommand cmd = CommandFactory.Create(context))
                {
                    try
                    {
                        //Esegue
                        await cmd.Execute();

                        //Debug richiesta
                        if (HfsData.Debug)
                            Utility.LogDump(context, "DEBUG Richiesta", 0, string.Empty, cmd.Request, cmd.Response);
                    }
                    catch (Exception? ex)
                    {
                        //Imposta codice generico
                        HfsException? e1 = ex as HfsException;
                        EStatusCode code = e1?.StatusCode ?? EStatusCode.GenericError;

                        var sbMessage = new StringBuilder();

                        while (ex != null)
                        {
                            sbMessage.Append(ex.Message);
                            sbMessage.Append(". ");
                            ex = ex.InnerException;
                        }

                        ICommand cmdErr = new CommandError() { Code = code, Msg = sbMessage.ToString(), Content = Utility.GetHelpHtml() };
                        cmdErr.Init(context, cmd.Request);
                        await cmdErr.Execute();

                        //Logga eccezione
                        Utility.LogDump(context, "Richiesta terminata con ERRORE", (int)code, sbMessage.ToString(), cmd.Request, cmd.Response);

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
                cmdErr.Init(context, new HfsRequest());
                await cmdErr.Execute();
            }

        }

    }
}
