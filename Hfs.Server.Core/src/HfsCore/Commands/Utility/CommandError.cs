using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandError: CommandBaseUty
    {

        public override string ActionKey { get; } = @"error";

        public EStatusCode Code { get; set; }


        public string Msg { get; set; }

        public string Content { get; set; }



        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            var msgFinal = this.Msg ?? string.Empty;
            this.SetResponseHeaders(this.Code, msgFinal);

            await this.WriteResponseText(Utility.ErrorMessageHtml(msgFinal));

            if (!string.IsNullOrWhiteSpace(this.Content))
                await this.WriteResponseText(this.Content);
        }
    }
}