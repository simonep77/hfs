using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// Classe base per la conversione
    /// </summary>
    internal static class FileConverterFactory
    {
        static FileConverterFactory()
        {
            //Sprotegge i file letti
            iTextSharp.text.pdf.PdfReader.unethicalreading = true;
        }

        /// <summary>
        /// Crea il convertitore
        /// </summary>
        /// <param name="type"></param>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static FileConverterBase CreateConverter(string type, FileInfo src, FileInfo dest)
        {
            //PDF
            if (type.Equals(Const.HFS_CONVERT_TYPE_PDF))
                return new FileConverterPDF(src, dest);

            throw new ArgumentException("Tipo conversione fornito sconosciuto. Ammessi: swf, pdf");
        }

        /// <summary>
        /// Verifica tipo conversione
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool ExistConverter(string type)
        {
            if (type.Equals(Const.HFS_CONVERT_TYPE_SWF))
                return true;

            if (type.Equals(Const.HFS_CONVERT_TYPE_PDF))
                return true;

            return false;
        }


        /// <summary>
        /// Verifica se estensione supportata per tipo
        /// </summary>
        /// <param name="fileExt"></param>
        /// <returns></returns>
        public static bool IsSupportedExt(string type, string fileExt)
        {

            if (type.Equals(Const.HFS_CONVERT_TYPE_PDF))
                return FileConverterPDF.IsSupported(fileExt);

            return false;

        }


    }
}