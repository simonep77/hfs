using Hfs.Server.Core.Vfs;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandEmail: CommandBaseFile
    {

        public override string ActionKey { get; } = @"email";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override Task CommandExecute()
        {
            this.FileHandler.SendByMail(this.Request.MailTo, this.Request.MailFrom, this.Request.MailSubj, this.Request.MailBody);
        }
    }
}