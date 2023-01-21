using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandWrite: CommandBaseFile
    {
        public override string ActionKey { get; } = @"write";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            await this.SaveRequestFile(this.FileHandler, false);

            //Se richiesta conversione viene gestita in maniera asincrona
            if (this.FileHandler.CanConvert())
                this.FileHandler.Convert(true, true);
        }
    }
}