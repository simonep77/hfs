using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// 
    /// </summary>
    internal class FileConverterPDF: FileConverterBase
    {
        /// <summary>
        /// Classe contenente un item con le info di conversione
        /// </summary>
        private class ConverterItem
        {
            public string Name;
            //public string Exe;
            //public string ArgsFmt;
        }

        private static Dictionary<string, ConverterItem> _Converters;

        /// <summary>
        /// Costruttore statico che inizializza i dati per la conversione
        /// </summary>
        static FileConverterPDF()
        {
            //Aggiunge le definizioni dei converter
            _Converters = new Dictionary<string, ConverterItem>(5);

            //PDF
            ConverterItem cnv = new ConverterItem() { Name = "PDF"};
            _Converters.Add(@".jpg", cnv);
            _Converters.Add(@".jpeg", cnv);
            _Converters.Add(@".png", cnv);
            _Converters.Add(@".gif", cnv);
            _Converters.Add(@".tiff", cnv);
        }

        /// <summary>
        /// Verifica se una estensione e' supportata
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static bool IsSupported(string ext)
        { 
            return _Converters.ContainsKey(ext);
        }

        public FileConverterPDF(FileInfo src, FileInfo dest)
            : base(src, dest)
        { }

        /// <summary>
        /// Esegue conversione
        /// </summary>
        protected override void doConvert()
        {
            string sExt = this.Source.Extension.ToLower();
            ConverterItem cnv = null;

            //Carica il giusto converter
            if (!_Converters.TryGetValue(sExt, out cnv))
                throw new ArgumentException("Estensione '" + sExt + "' non supportata");


            //converte
            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(this.Source.FullName, true);
            using (FileStream fs = new FileStream(this.Dest.FullName, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (Document doc = new Document(image))
                {
                    using (PdfWriter writer = PdfWriter.GetInstance(doc, fs))
                    {
                        doc.Open();
                        image.SetAbsolutePosition(0, 0);
                        writer.DirectContent.AddImage(image);
                        doc.Close();
                    }
                }
            }

            this.Dest.Refresh();
        }
    }
}