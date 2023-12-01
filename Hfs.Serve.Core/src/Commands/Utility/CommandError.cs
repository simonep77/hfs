using Hfs.Server.Core.Common;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandError: CommandBaseUty
    {

        public override string ActionKey { get; } = @"error";

        public EStatusCode Code { get; set; }


        public string Msg { get; set; }

        public string Content { get; set; }



        async protected override Task CommandExecute()
        {
            var msgFinal = this.Msg ?? string.Empty;
            this.SetResponseHeaders(this.Code, msgFinal);

            await this.WriteResponseText(Utility.ErrorMessageHtml(msgFinal) + this.Content);
        }
    }
}