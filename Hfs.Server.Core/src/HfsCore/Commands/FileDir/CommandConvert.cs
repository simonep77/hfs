using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandConvert: CommandBaseFile
    {
        public override string ActionKey { get; } = @"convert";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            //Se richiesta conversione viene gestita in maniera asincrona
            if (this.FileHandler.Exist())
                this.FileHandler.Convert(true, true);

        }
    }
}