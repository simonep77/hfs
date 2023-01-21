using Hfs.Server.CODICE.UTILITY;
using Hfs.Server.HfsCore.Commands;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Hfs.Server.CODICE.CLASSI
{
    public static class Utility
    {
        /// <summary>
        /// Ritorna valore normalizzato
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetValue(string value)
        {
            return (value == null) ? string.Empty : value.Trim();
        }

        /// <summary>
        /// Ritorna valore di default se value NULL
        /// </summary>
        /// <param name="value"></param>
        /// <param name="defaultvalue"></param>
        /// <returns></returns>
        public static string GetValue(string value, string defaultvalue)
        {
            return string.IsNullOrEmpty(value) ? defaultvalue : value.Trim();
        }




        /// <summary>
        /// Ritorna nuovo nome file convertito
        /// </summary>
        /// <param name="file"></param>
        /// <param name="vpathorig"></param>
        /// <param name="newext"></param>
        /// <returns></returns>
        public static string GetConvertedFileName(FileInfo file, string vpathorig, string newext)
        {
            string sHashFile = string.Concat(
                Symbiote.Core.Hashing.MurmurHash3.Hash(string.Concat(file.CreationTime.ToBinary().ToString(),
                    file.LastWriteTime.ToBinary().ToString(),
                    file.Length.ToString(),
                    vpathorig.ToLower())).ToString(), newext);

            return Path.Combine(HfsData.TempDirConvertedFiles, sHashFile);
            
        }



        /// <summary>
        /// Controlla che due path non siano il medesimo oppure il secondo sottodirectory del primo
        /// </summary>
        /// <param name="vpath"></param>
        /// <param name="vpathdest"></param>
        public static void CheckDirS(string vpath, string vpathdest)
        {
            //Controlla che non siano uguali i path
            if (vpath.ToLower() == vpathdest.ToLower())
            {
                throw new HfsException(EStatusCode.PathAreSame, "Il path sorgente non puo' coincidere con quello di destinazione");
            }
            //Ricrea path con / finale
            vpath = string.Concat(vpath.TrimEnd(Const.URI_SEPARATOR), Const.URI_SEPARATOR);

            //Controlla che path destinazione non sia una sottodirectory della cartella sorgente (ricorsione)
            if (vpathdest.ToLower().StartsWith(vpath.ToLower()))
            {
                throw new HfsException(EStatusCode.RecursiveDir, "Il path destinazione non puo' essere una sottodirectory del path sorgente");
            }
        }


        /// <summary>
        /// Controlla esistenza file, se non lo trova lancia eccezione
        /// </summary>
        /// <param name="file"></param>
        /// <param name="vpath">
        /// Path virtuale da visualizzare nel messaggio
        /// </param>
        /// <param name="spec">
        /// Specifica del file da inserire nel messaggio di errore
        /// </param>
        public static void CheckFileExist(string file, string vpath, string spec)
        {
            if (!System.IO.File.Exists(file))
            {
                throw new HfsException(EStatusCode.FileNotFound, "File {0} {1} non trovato", spec, vpath);
            }  
        }


        /// <summary>
        /// Controlla esistenza directory
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="vpath"></param>
        /// <param name="spec"></param>
        public static void CheckDirExist(string dir, string vpath, string spec)
        {
            if (!System.IO.Directory.Exists(dir))
            {
                throw new HfsException(EStatusCode.DirectoryNotFound, "Directory {0} {1} non trovata", spec, vpath);
            }
        }

        /// <summary>
        /// Rimpiazza backslash con slash (notazione uri)
        /// </summary>
        /// <param name="vpath"></param>
        /// <returns></returns>
        public static string NormalizeVirtualPath(string vpath)
        {
            return vpath.Replace(System.IO.Path.DirectorySeparatorChar, Const.URI_SEPARATOR).TrimEnd(Const.URI_SEPARATOR).Replace(@"//", Const.URI_SEPARATOR.ToString());
        }

        /// <summary>
        /// Combina path hfs
        /// </summary>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static string HfsCombine(params string[] parts)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i].Length == 0)
                    continue;

                sb.Append(parts[i].TrimEnd(Const.URI_SEPARATOR));
                sb.Append(Const.URI_SEPARATOR);
            }

            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }


        /// <summary>
        /// Controllo validità path virtuale
        /// </summary>
        /// <param name="vpath"></param>
        public static void CheckVirtualPath(string vpath, string spec)
        {
            //Verifica path virtuale
            if (string.IsNullOrEmpty(vpath))
            {
                throw new HfsException(EStatusCode.NoInputPath, "Il path {0} non e' valorizzato", spec);
            }
            if (vpath.Contains(@"./"))
            {
                throw new HfsException(EStatusCode.InvalidPath, "Il path {0} non puo' contenere ./", spec);
            }
        }

        /// <summary>
        /// Data una querystring con dati di blocco ritorna array con valori
        /// </summary>
        /// <param name="sblock"></param>
        /// <returns></returns>
        public static FileBlock DecodeQsRBlock(string sblock)
        {
            FileBlock block = new FileBlock();
            string[] values = sblock.Split(',');

            try
            {
                if (values.Length != 2)
                    throw new ArgumentException();

                block.Offset = Convert.ToInt64(values[0].Trim());
                block.Length = Convert.ToInt64(values[1].Trim());
            }
            catch (Exception)
            {
                throw new ArgumentException(string.Concat("I valori forniti per la dimensione del blocco '", sblock, "' non sono validi. Atteso block=pos,len dove pos e len sono entrambi numerici"));
            }
            
            return block;
        }



        /// <summary>
        /// Imposta rappresentazione xml delle informazioni di un file
        /// </summary>
        /// <param name="sbXml"></param>
        /// <param name="fullvpath"></param>
        /// <param name="file"></param>
        /// <param name="access"></param>
        public static void GetFileInfoXml(XmlWrite xw, string fullvpath, System.IO.FileInfo fInfo, string access)
        {
            Utility.GetFileInfoXml(xw, fullvpath, access, fInfo.Length, fInfo.CreationTime, fInfo.LastWriteTime, fInfo.LastAccessTime, fInfo.Attributes);
        }


        /// <summary>
        /// Scrive info base file
        /// </summary>
        /// <param name="sbXml"></param>
        /// <param name="fullvpath"></param>
        /// <param name="access"></param>
        /// <param name="len"></param>
        /// <param name="cdate"></param>
        /// <param name="wdate"></param>
        /// <param name="adate"></param>
        /// <param name="attr"></param>
        public static void GetFileInfoXml(XmlWrite xw, string fullvpath, string access, long len, DateTime cdate, DateTime wdate, DateTime adate, FileAttributes attr)
        {
            xw.WriteStartElement(@"file");
            xw.WriteElementString(@"name", fullvpath);
            xw.WriteElementString(@"access", access);
            xw.WriteElementString(@"len", len.ToString());
            xw.WriteElementString(@"ctime", cdate.ToString("yyyyMMddHHmmss"));
            xw.WriteElementString(@"wtime", wdate.ToString("yyyyMMddHHmmss"));
            xw.WriteElementString(@"attr", attr.ToString("D"));
            xw.WriteEndElement();
        }


        /// <summary>
        /// Imposta rappresentazione xml delle informazioni di una directory
        /// </summary>
        /// <param name="sbXml"></param>
        /// <param name="fullvpath"></param>
        /// <param name="dir"></param>
        /// <param name="access"></param>
        public static void GetDirectoryInfoXml(XmlWrite xw, string fullvpath, DateTime ctime, string access)
        {

            xw.WriteStartElement(@"dir");
            xw.WriteElementString(@"name", fullvpath);
            xw.WriteElementString(@"access", access);
            xw.WriteElementString(@"ctime", ctime.ToString("yyyyMMddHHmmss"));
            xw.WriteEndElement();

        }



        /// <summary>
        /// Carica xml con info file
        /// </summary>
        /// <param name="sbXml"></param>
        /// <param name="vpath"></param>
        /// <param name="dir"></param>
        /// <param name="access"></param>
        /// <param name="pattern"></param>
        /// <param name="recursive"></param>
        public static void SetFileListInfoXml(XmlWrite xw, string vpath, DirectoryInfo dInfo, string access, string pattern, bool recursive)
        {
            FileInfo[] files = dInfo.GetFiles(pattern);

            for (int i = 0; i < files.Length; i++)
            {
                GetFileInfoXml(xw, Utility.HfsCombine(vpath, files[i].Name), files[i], access);
            }

            if (recursive)
            {
                DirectoryInfo[] dirs = dInfo.GetDirectories();

                for (int i = 0; i < dirs.Length; i++)
                {
                    SetFileListInfoXml(xw, Utility.HfsCombine(vpath, dirs[i].Name) , dirs[i], access, pattern, recursive);
                }
            }

        }


        /// <summary>
        /// Imposta xml con info directory
        /// </summary>
        /// <param name="sbXml"></param>
        /// <param name="vpath"></param>
        /// <param name="dir"></param>
        /// <param name="access"></param>
        /// <param name="pattern"></param>
        /// <param name="recursive"></param>
        public static void SetDirListInfoXml(XmlWrite xw, string vpath, DirectoryInfo dInfo, string access, string pattern, bool recursive)
        {
            DirectoryInfo[] dirs = dInfo.GetDirectories(pattern);

            for (int i = 0; i < dirs.Length; i++)
            {
                string sFullVpath = Utility.HfsCombine(vpath, dirs[i].Name);
                GetDirectoryInfoXml(xw, sFullVpath, dirs[i].LastAccessTime, access);

                if (recursive)
                {
                    SetDirListInfoXml(xw, sFullVpath, dirs[i], access, pattern, recursive);
                }
            }
        }


        private static string _help;
        /// <summary>
        /// Ritorna html per help
        /// </summary>
        /// <returns></returns>
        public static string GetHelpHtml()
        {
            if (!string.IsNullOrEmpty(_help))
                return _help;

            //Carica info
            //Scrive xml con dati file
            StringBuilder sbHtml = new StringBuilder(1000);
           
            sbHtml.Append(@"<div>");
            sbHtml.Append(@"<p align='center' style='color: blue; font-family: arial; font-size: 17px; font-weight: bold;'>");
            sbHtml.Append(@"HTTP FILE SERVER");
            sbHtml.Append(@"</p>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<p style='font-family: arial; font-size: 17px;'>");
            sbHtml.Append(@"<b><span style='color: red;'>Parametri Header/Querystring (solo se attivato da config):</span></b>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-action </b>=[azione da eseguire]</b> - <u>Obbligatorio</u>");
            sbHtml.Append(@"<br />");

            sbHtml.Append(@"<ul>");
            sbHtml.Append(@" <b>per file:</b>");

            var fileCmds = from c in CommandFactory.AvailableCommands.Values
                           where c.Command.GetType().IsSubclassOf(typeof(CommandBaseFile))
                           select c;

            foreach (var item in fileCmds)
            {
                sbHtml.Append($"<li>{item.Command.ActionKey}</li>");
            }

            sbHtml.Append(@" <b>per directory:</b>");

            var dircmds = from c in CommandFactory.AvailableCommands.Values
                           where c.Command.GetType().IsSubclassOf(typeof(CommandBaseDir))
                           select c;

            foreach (var item in dircmds)
            {
                sbHtml.Append($"<li>{item.Command.ActionKey}</li>");
            }

            sbHtml.Append(@" <b>amministrative:</b>");

            var admcmds = from c in CommandFactory.AvailableCommands.Values
                          where c.Command.GetType().IsSubclassOf(typeof(CommandBaseAdmin))
                          select c;

            foreach (var item in admcmds)
            {
                sbHtml.Append($"<li>{item.Command.ActionKey}</li>");
            }

            sbHtml.Append(@" <b>utility</b>:");

            var utycmds = from c in CommandFactory.AvailableCommands.Values
                          where c.Command.GetType().IsSubclassOf(typeof(CommandBaseUty))
                          select c;

            foreach (var item in utycmds)
            {
                sbHtml.Append($"<li>{item.Command.ActionKey}</li>");
            }
            sbHtml.Append(@"</ul>");

            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-user </b>=[username]</b> - <u>Se non fornito utilizza il default 'guest'</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-pass </b>=[password]</b>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-vpath</b>=[es. /aaa.txt] - <u>Obbligatorio TUTTI action</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-vpathdest</b>=[es. /aaa.txt] - <u>Obbligatorio SOLO per hfs-action=[move, copy, mvdir, cpdir]</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-link</b>=[link ottenuto in precedenza da getlink]");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-attr</b>=[es. 256] - <u>Obbligatorio SOLO per hfs-action=setattr; Facoltativo per listf o listd (impostato a R fornisce liste ricorsive flat) e read (impostato swf effettua la conversione)</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-block</b>=[es. 0,1024] - <u>Facoltativo SOLO per hfs-action=read</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-pattern</b>=[es. *.pdf] - <u>Facoltativo SOLO per hfs-action=[listf,listd]</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-mailto</b>=[es. simone.prova@email.it,simone2@prova.it] - <u>Obbligatorio SOLO per hfs-action=email</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-mailfrom</b>=[es. test@email.it] - <u>Facoltativo SOLO per hfs-action=email</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-mailsubj</b>=[es. Prova] - <u>Facoltativo SOLO per hfs-action=email</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"<b>hfs-mailbody</b>=[es. Buon giorno] - <u>Facoltativo SOLO per hfs-action=email</u>");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"</div>");
            sbHtml.Append(@"<hr size='1' />");

            _help = sbHtml.ToString();
            return _help;
        }


        /// <summary>
        /// Ritorna html per messaggio di errore
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static string ErrorMessageHtml(string errMsg)
        {
            //Carica info
            //Scrive xml con dati file
            StringBuilder sbHtml = new StringBuilder(200);
            sbHtml.Append(@"<div>");
            sbHtml.Append(@"<p style='color: #990000; font-family: arial; font-size: 18px; font-weight: bold;'>");
            sbHtml.Append("*** ");
            sbHtml.Append("Si e' verificato il seguente errore: ");
            sbHtml.Append(errMsg);
            sbHtml.Append(" ***");
            sbHtml.Append(@"<br />");
            sbHtml.Append(@"</p>");
            sbHtml.Append(@"<hr size='1' />");
            sbHtml.Append(@"</div>");

            return sbHtml.ToString();
        }


        /// <summary>
        /// Invia file per email
        /// </summary>
        public static void SendByMail(string to, string from, string subj, string body, string file)
        {
            to = GetValue(to);
            from = GetValue(from);
            subj = GetValue(subj);
            body = GetValue(body);

            //Nessun destinatario
            if (string.IsNullOrEmpty(to))
                throw new HfsException(EStatusCode.EmptyToAddr, "E' necessario fornire un destinatario");

            using (MailMessage oMessage = new MailMessage())
            {
                //Crea messaggio
                oMessage.IsBodyHtml = true;
                oMessage.Subject = subj;
                oMessage.Body = body;
                oMessage.To.Add(to);
                if (!string.IsNullOrEmpty(from))
                {
                    oMessage.From = new MailAddress(from);
                }
                oMessage.Attachments.Add(new Attachment(file));

                //Invia
                SmtpClient oSmtp = new SmtpClient();
                oSmtp.Send(oMessage);
            }
        }


        /// <summary>
        /// Imposta attributi file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="attr"></param>
        public static void SetFileAttr(string file, string attr)
        {
            FileAttributes oAttr = FileAttributes.Normal;
            try
            {
                oAttr = (FileAttributes)Convert.ToInt32(GetValue(attr));
            }
            catch (Exception)
            {
                throw new HfsException(EStatusCode.AttrNotNumeric, "Parametro 'attr' non fornito o non numerico");
            }
            File.SetAttributes(file, oAttr);
        }


        /// <summary>
        /// Copia ricorsiva di directory
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it’s new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }


   
        /// <summary>
        /// Elimina un albero di cartelle
        /// </summary>
        /// <param name="dir"></param>
        public static async Task<bool> CleanDirectoryTree(string dir, bool remove)
        {
            //Se non esiste esce
            if (!System.IO.Directory.Exists(dir))
                return false;

            //Elimina tutti i file
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }

            //Va in ricorsione sulle sottocartelle
            string[] dirs = Directory.GetDirectories(dir);
            for (int i = 0; i < dirs.Length; i++)
            {
                await Utility.CleanDirectoryTree(dirs[i], true);
            }

            //Infine elimina la cartella corrente
            if (remove)
                System.IO.Directory.Delete(dir);

            return true;
        }


        /// <summary>
        /// Calcola dimensione files
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static long GetDirSize(string dir)
        {
            long lSize = 0L;
            FileInfo fInfo;
            //Elimina tutti i file
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                fInfo = new FileInfo(files[i]);
                lSize += fInfo.Length;
            }

            //Va in ricorsione sulle sottocartelle
            string[] dirs = Directory.GetDirectories(dir);
            for (int i = 0; i < dirs.Length; i++)
            {
                lSize += Utility.GetDirSize(dirs[i]);
            }

            //Infine ritorna
            return lSize;
        }



        /// <summary>
        /// Calcola hash SHA1 di un file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string CalculateSHA1(string file)
        {
            using (FileStream fs = File.OpenRead(file))
            {
                System.Security.Cryptography.SHA1CryptoServiceProvider sha1Prov = new System.Security.Cryptography.SHA1CryptoServiceProvider();

                return Convert.ToBase64String(sha1Prov.ComputeHash(fs));
            }
        }
       

        /// <summary>
        /// Crea link per accesso diretto in lettura a risorsa
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static string CreateReadLink(HttpContext context, HfsRequest req)
        {
            int iSeconds = Const.HFS_LINK_SECONDS;
            try
            {
                if (!string.IsNullOrEmpty(req.Attr))
                    iSeconds = Convert.ToInt32(req.Attr);
            }
            catch (Exception)
            {
                throw new ArgumentException("Il parametro 'hfs-attr', per la specifica dei secondi di validita', deve contenere un numero intero");
            }
            DateTime dtNow = DateTime.Now;
            DateTime dtExpire = dtNow.AddSeconds(iSeconds);

            String sToEncode = string.Concat(req.User.PadRight(Const.HFS_LINK_PADLEN, Const.HFS_LINK_PADCHAR),
                req.Pass.PadRight(Const.HFS_LINK_PADLEN, Const.HFS_LINK_PADCHAR),
                dtNow.ToString(Const.HFS_LINK_DATEFMT),
                dtExpire.ToString(Const.HFS_LINK_DATEFMT),
                req.VPath);

            return string.Concat(context.Request.Scheme,
                            "://", context.Request.Host,
                            context.Request.Path,
                            @"?", Const.QS_LINK, "=",
                            HttpUtility.UrlEncode(CryptoUtils.AesEncryptString(sToEncode, Const.HFS_ENC_KEY)));

        }


        /// <summary>
        /// Esegue decodifica del link se presente
        /// </summary>
        /// <param name="readlink"></param>
        /// <param name="req"></param>
        public static IEnumerable<KeyValuePair<string, string>> DecodeReadLink(string readlink)
        {
            try
            {
                //Decodifica
                readlink = CryptoUtils.AesDecryptString(readlink, Const.HFS_ENC_KEY);
            }
            catch (Exception)
            {
                throw new ApplicationException("Il link al file fornito non e' un link generato da una precedente chiamata 'getlink'");
            }
            

            //Verifica date
            DateTime dtNow = DateTime.Now; 
            DateTime dtStart = DateTime.ParseExact(
                readlink.Substring(Const.HFS_LINK_PADLEN + Const.HFS_LINK_PADLEN, Const.HFS_LINK_DATEFMT.Length),
                Const.HFS_LINK_DATEFMT, 
                System.Globalization.CultureInfo.CurrentCulture);

            DateTime dtExpire = DateTime.ParseExact(
                readlink.Substring(Const.HFS_LINK_PADLEN + Const.HFS_LINK_PADLEN + Const.HFS_LINK_DATEFMT.Length, Const.HFS_LINK_DATEFMT.Length),
                Const.HFS_LINK_DATEFMT,
                System.Globalization.CultureInfo.CurrentCulture);

            //Verifica scadenza
            if (!(dtNow > dtStart && dtNow < dtExpire))
                throw new ApplicationException("Il link al file fornito non e' valido. E' necessario richiederne un nuovo link");

            //OK, imposta richiesta
            var oRet = new List<KeyValuePair<string, string>>();
            oRet.Add(new KeyValuePair<string, string>(Const.QS_ACTION, "read"));
            oRet.Add(new KeyValuePair<string, string>(Const.QS_USER, readlink.Substring(0, Const.HFS_LINK_PADLEN).TrimEnd(Const.HFS_LINK_PADCHAR)));
            oRet.Add(new KeyValuePair<string, string>(Const.QS_PASS, readlink.Substring(Const.HFS_LINK_PADLEN, Const.HFS_LINK_PADLEN).TrimEnd(Const.HFS_LINK_PADCHAR)));
            oRet.Add(new KeyValuePair<string, string>(Const.QS_VPATH, readlink.Substring((Const.HFS_LINK_PADLEN * 2) + (Const.HFS_LINK_DATEFMT.Length * 2))));

            return oRet;
        }


        /// <summary>
        /// Esecuzione pulizia directory in base a pattern, data di ultima modifica con opzione di ricorsione.
        /// </summary>
        public static void CleanDirectory(string directory, string pattern, DateTime dtExpired, bool includeSubDirs)
        {
            try
            {
                FileInfo fInfo;

                //Per ogni file
                foreach (string sFile in Directory.GetFiles(directory, pattern))
                {
                    try
                    {
                        fInfo = new FileInfo(sFile);

                        if (fInfo.LastWriteTime < dtExpired && fInfo.Exists)
                            fInfo.Delete();
                    }
                    catch (Exception e)
                    {
                        HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"Errore durante la pulizia della directory {directory}");
                        HfsData.Logger.WriteException(ELogType.HfsGlobal, e);
                    }

                }

                //Se non richieste subdir esce
                if (!includeSubDirs)
                    return;

                //Per ogni sottocartella va in ricorsione
                foreach (string sDir in Directory.GetDirectories(directory))
                {
                    Utility.CleanDirectory(sDir, pattern, dtExpired, includeSubDirs);
                }
            }
            catch (Exception e)
            {
                HfsData.Logger.WriteException(ELogType.HfsGlobal, e);
            }
            
        }


        /// <summary>
        /// Dato un pattern hfs ritorna la regex per il confronto
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static Regex GetRegexFromPattern(string pattern)
        {
            pattern = $"^{Regex.Escape(pattern).Replace(@"\*", @".*")}$";
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }

    }
    

    
}
