using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandRead: CommandBaseFile
    {

        public override string ActionKey { get; } = @"read";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            //Conversione
            if (this.FileHandler.CanConvert())
                this.FileHandler.Convert(false, false);
            //Scrive
            await this.WriteResponseFile(this.FileHandler, this.Request.Block);
        }
    }
}