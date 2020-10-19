using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.CLASSI.FileHandling;
using Hfs.Server.CODICE.VFS;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandConvertDir: CommandBaseDir
    {

        public override string ActionKey { get; } = @"convertdir";
        public override VfsAction Action { get; } = VfsAction.Read;

        async protected override System.Threading.Tasks.Task CommandExecute()
        {
            //Verifica
            if (!FileConverterFactory.ExistConverter(this.Request.Attr))
                throw new HfsException(EStatusCode.GenericError, "Tipo di conversione '{0}' non implementata.", this.Request.Attr);

            //Crea cache dei file convertiti
            IFileHandler[] files = this.DirHandler.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].CanConvert())
                    files[i].Convert(true, true);
            }
        }
    }
}