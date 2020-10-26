using System;
using System.Xml;
using System.Text;
using System.Collections.Generic;
using Hfs.Server.CODICE.CLASSI;
using Hfs.Server.CODICE.UTILITY;

namespace Hfs.Server.CODICE.VFS
{
    /// <summary>
    /// Classe per la gestione di un Virtual File System
    /// </summary>
    public class Vfs
    {
        private static System.Threading.ManualResetEvent _LoadConfLock = new System.Threading.ManualResetEvent(true);
        private static long _AccessCount;

        private Dictionary<string, VfsUser> Users = new Dictionary<string, VfsUser>(10);
        private Dictionary<string, VfsPath> Paths = new Dictionary<string, VfsPath>(10);
        private string mVfsXmlPath;
        private bool mIsFullLoaded;


        #region PROPERTIES

        /// <summary>
        /// Ritorna il numero di utenti definiti
        /// </summary>
        public int UserCount
        {
            get { return this.Users.Count - 4; }
        }

        /// <summary>
        /// Indica se il caricamento del VFS e' completo
        /// </summary>
        public bool IsFullLoaded
        {
            get 
            {
                //Verifica possibilita' di entrare (es durante un reload del file)
                if (!_LoadConfLock.WaitOne(Const.VFS_LOAD_WAIT_MSEC, false))
                {
                    return false;
                }

                //Incrementa numero accessi
                System.Threading.Interlocked.Increment(ref _AccessCount);
                try
                {
                    //Ritorna info
                    return this.mIsFullLoaded; 
                }
                finally
                {
                    //Decrementa numero accessi
                    System.Threading.Interlocked.Decrement(ref _AccessCount);
                }

                
            
            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Creazione vfs a partire da file
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public Vfs(string xmlFilePath)
        {
            mVfsXmlPath = xmlFilePath;
        }
        

        /// <summary>
        /// Carica Virtual File Systema da File Xml
        /// </summary>
        /// <param name="vfsFileName"></param>
        public void Load()
        {
            //Log avvio
            HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "Avvio caricamento vfs..");
            //Blocca altri
            _LoadConfLock.Reset();
            //Attende conteggio lettori a zero
            while (System.Threading.Interlocked.Read(ref _AccessCount) > 0)
            {
                System.Threading.Thread.Sleep(Const.VFS_LOAD_CHECK_MSEC);
            }
            
            try
            {
                //Svuota memoria corrente
                this.inizializeVfs();

                //Imposta Flag di caricamento
                this.mIsFullLoaded = false;

                XmlDocument oDoc = new XmlDocument();
                try
                {
                    oDoc.Load(this.mVfsXmlPath);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException(string.Format("Errore caricamento VFS: {0}. Errore XML: {1}", ex.Message, ex.Message));
                }

                VfsUser oUser;
                VfsPath oPath;
                VfsAccess oAccess;
                VfsAccess oAdminAccess = new VfsAccess(this.GetUser(Const.VFS_USER_ADMIN), Const.VFS_ACCESS_ALL);

                //Carica elenco utenti globale
                foreach (XmlElement oNode in oDoc.SelectNodes(@"root/users/user"))
                {
                    try
                    {
                        //Crea utente
                        oUser = new VfsUser(this);
                        oUser.Name = oNode.GetAttribute("name");
                        oUser.Pass = oNode.GetAttribute("pass");
                        //Aggiunge ad elenco globale
                        this.AddUser(oUser);
                    }
                    catch (Exception ex)
                    {
                        
                        throw new ApplicationException(string.Format("Errore caricamento VFS: {0}. Nodo XML: {1}", ex.Message, oNode.OuterXml));
                    }
                    
                }

                //Aggiunge i path Utente
                foreach (XmlElement oNode in oDoc.SelectNodes("root/paths/path"))
                {
                    //Crea path
                    oPath = new VfsPath(this);
                    try
                    {
                        oPath.Virtual = oNode.GetAttribute("virtual");
                        oPath.Physical = oNode.GetAttribute("physical");

                        //Se presente attributo type allora lo imposta
                        var sType = oNode.GetAttribute("type");
                        if (!string.IsNullOrEmpty(sType))
                            oPath.PathType = (EVfsPathType)Enum.Parse(typeof(EVfsPathType), sType, true);

                        //Aggiunge Admin
                        oPath.AddAccess(oAdminAccess);

                        //Crea la directory se non esiste
                        if (oPath.IsLocal && !oPath.IsUnc)
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(oPath.Physical);
                            }
                            catch (Exception ex)
                            {
                                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, $"Attenzione! impossibile creare drectory fisica {oPath.Physical} per vpath {oPath.Virtual}");
                                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, ex.Message);
                            }
                            
                        }

                        //Aggiunge alla lista
                        this.AddPath(oPath);
                    }
                    catch (Exception ex)
                    {
                        throw new ApplicationException(string.Format("Errore caricamento VFS: {0}. Nodo XML: {1}", ex.Message, oNode.OuterXml));
                    }
                    

                    //Aggiunge autorizzazioni specifiche
                    foreach (XmlElement oUNode in oNode.SelectNodes(@"auth/user"))
                    {
                        try
                        {
                            //Crea path
                            oAccess = new VfsAccess(this.GetUser(oUNode.GetAttribute("name")), oUNode.GetAttribute("access"));
                            //Aggiunge autorizzazione
                            oPath.AddAccess(oAccess);
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(string.Format("Errore caricamento VFS: {0}. Nodo XML: {1}", ex.Message, oUNode.OuterXml));
                        }
                    }

                    //Aggiunge eventuali parametri
                    foreach (XmlElement oUNode in oNode.SelectNodes(@"params/param"))
                    {
                        try
                        {
                            //Aggiunge parametro
                            oPath.Params.Add(oUNode.GetAttribute("key"), oUNode.GetAttribute("value"));
                        }
                        catch (Exception ex)
                        {
                            throw new ApplicationException(string.Format("Errore caricamento VFS: {0}. Nodo XML: {1}", ex.Message, oUNode.OuterXml));
                        }
                    }

                }

                //Imposta
                this.mIsFullLoaded = true;
            }
            finally
            {
                //Sblocca altri
                _LoadConfLock.Set();
                //log fine
                HfsData.Logger.WriteMessage(ELogType.HfsGlobal, "Fine caricamento vfs");
            }
            
        }


        /// <summary>
        /// Ricerca il path fornito
        /// </summary>
        /// <param name="vpath"></param>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public HfsResponseVfs GetResponse(HfsRequest req, bool checkDest)
        {
            //Verifica possibilita' di entrare (es durante un reload del file)
            if (!_LoadConfLock.WaitOne(Const.VFS_LOAD_WAIT_MSEC, false))
                throw new ApplicationException("Il sistema sta ricaricando la propria configurazione. Riprovare tra pochi secondi");

            //Incrementa numero accessi
            System.Threading.Interlocked.Increment(ref _AccessCount);
            try
            {
                string vpath = req.VPath;

                if (checkDest)
                    vpath = req.VPathDest;

                HfsResponseVfs oResp = new HfsResponseVfs();

                //Esecuzione
                oResp.User = this.GetUser(req.User);

                //Utenti speciali non ammessi
                if (oResp.User.IsSpecial)
                    throw new ArgumentException("Utente non valido");

                //Identifica utente
                if (!oResp.User.Pass.Equals(req.Pass))
                    throw new ApplicationException("Password non valida");

                //Trova path
                string sVPathLow = vpath.ToLower();

                //NEW
                int iChar = 0;
                foreach (var item in this.Paths)
                {
                    if (sVPathLow.StartsWith(item.Key, StringComparison.Ordinal) && item.Key.Length > iChar)
                    {
                        oResp.Path = item.Value;
                        iChar = item.Key.Length;
                    }
                }

                //END

                //Se non trovato esce
                if (oResp.Path == null)
                    throw new ApplicationException($"Path fornito non trovato ({req.VPath})");

                //Carica accesso
                oResp.Access = oResp.Path.GetAccess(oResp.User);

                //if (oResp.Access == null)
                //    throw new ApplicationException($"Utente '{req.User}' non configurato per la risorsa");

                //Calcola path virtuale e fisico
                oResp.VirtualPath = vpath;
                //if (oResp.Path.IsLocal)
                //    oResp.VirtualPath = vpath;
                //else
                //    oResp.VirtualPath = sVPathLow.Remove(0, Math.Min(sVPathLow.Length, oResp.Path.Virtual.Length + 1));

                oResp.PhysicalPath = oResp.Path.Translate(oResp.VirtualPath, oResp.User);
                oResp.FileConvertType = req.Attr.ToLower();
                

                //Ritorna
                return oResp;
            }
            finally
            {
                //Decrementa
                System.Threading.Interlocked.Decrement(ref _AccessCount);
            }

        }


        /// <summary>
        /// Ritorna XML con elenco path virtuali a cui l'utente ha accesso
        /// </summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        public string GetUserPathsXml(string user, string pass)
        {
            //Verifica possibilita' di entrare (es durante un reload del file)
            if (!_LoadConfLock.WaitOne(Const.VFS_LOAD_WAIT_MSEC, false))
                throw new ApplicationException("Il sistema sta ricaricando la propria configurazione. Riprovare tra pochi secondi");

            //Incrementa numero lettori
            System.Threading.Interlocked.Increment(ref _AccessCount);
            try
            {
                VfsUser oUser = this.GetUser(user);

                //Utenti speciali non ammessi
                if (oUser.IsSpecial)
                    throw new ArgumentException("Utente non valido");

                //Identifica utente
                if (!oUser.Pass.Equals(pass))
                    throw new ApplicationException("Password non valida");


                using (var xw = new XmlWrite())
                {
                    xw.WriteStartElement(@"root");
                    try
                    {
                        //OK, ritorna elenco path dell'utente
                        foreach (var path in this.Paths.Values)
                        {
                            VfsAccess oAccess = path.GetAccess(oUser);

                            if (oAccess == null || path.Virtual.Equals(Const.VFS_PATH_ADMIN))
                                continue;

                            //Scrive
                            xw.WriteStartElement(@"path");
                            try
                            {
                                xw.WriteElementString(@"name", path.Virtual);
                                xw.WriteElementString(@"type", path.PathType.ToString());
                                xw.WriteElementString(@"access", oAccess.ToString());
                            }
                            finally
                            {
                                xw.WriteEndElement();
                            }
                        }

                    }
                    finally
                    {
                        xw.WriteEndElement();
                    }

                    return xw.ToString();
                }

            }
            finally
            {
                //Decrementa
                System.Threading.Interlocked.Decrement(ref _AccessCount);
            }
        }


        /// <summary>
        /// Ritorna utente
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public VfsUser GetUser(string user)
        {
            VfsUser oUser = null;

            if (!this.Users.TryGetValue(user, out oUser))
                throw new ArgumentException(string.Format("L'utente '{0}' non esiste", user));

            return oUser;
        }


        /// <summary>
        /// Aggiunge utente
        /// </summary>
        /// <param name="user"></param>
        public void AddUser(VfsUser user)
        {
            this.addUser(user, false);
        }


        /// <summary>
        /// Elimina utente
        /// </summary>
        /// <param name="username"></param>
        public void DeleteUser(VfsUser user)
        {
            //Se di sistema errore
            if (user.IsSystem)
                throw new ArgumentException(string.Format("Impossibile eliminare l'utente '{0}' poiche' di sistema", user.Name));

            //Sgancia utente da vpaths
            foreach (KeyValuePair<string, VfsPath> pathPair in this.Paths)
            {
                pathPair.Value.RemoveAccess(user);
            }

            //Rimuove utente
            this.Users.Remove(user.Name);
            
        }


        /// <summary>
        /// Aggiunge path
        /// </summary>
        /// <param name="path"></param>
        public void AddPath(VfsPath path)
        {
            this.addPath(path, false);
        }

        
        #endregion

        #region PRIVATE

        /// <summary>
        /// Resetta virtual path
        /// </summary>
        private void inizializeVfs()
        {
            this.Users.Clear();
            this.Paths.Clear();

            //Crea oggetti VFS di default:
            //Aggiunge utente Admin
            VfsUser oAdminUser = new VfsUser(this);
            oAdminUser.Name = Const.VFS_USER_ADMIN;
            oAdminUser.Pass = Properties.Settings.Default.AdminPass;
            this.addUser(oAdminUser, true);
            //Crea istanza accesso admin con privilegi completi
            VfsAccess oAdminAccess = new VfsAccess(this.GetUser(Const.VFS_USER_ADMIN), Const.VFS_ACCESS_ALL);


            //Aggiunge utente Guest
            VfsUser oGuestUser = new VfsUser(this);
            oGuestUser.Name = Const.VFS_USER_GUEST;
            oGuestUser.Pass = string.Empty;
            this.addUser(oGuestUser, true);

            //Aggiunge utente Any
            VfsUser oAnyUser = new VfsUser(this);
            oAnyUser.Name = Const.VFS_USER_ANY;
            oAnyUser.Pass = string.Empty;
            this.addUser(oAnyUser, true);

            //Aggiunge utente Auth
            VfsUser oAuthUser = new VfsUser(this);
            oAuthUser.Name = Const.VFS_USER_AUTH;
            oAuthUser.Pass = string.Empty;
            this.addUser(oAuthUser, true);


            //Aggiunge vpath amministrazione
            VfsPath oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_ADMIN;
            oPath.Physical = "!_BAD_PATH_!";
            oPath.AddAccess(oAdminAccess);
            this.addPath(oPath, true);

            //Aggiunge vpath log
            oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_LOG;
            oPath.Physical = Utility.MapVirtualPath(Properties.Settings.Default.LogDirectory);
            oPath.AddAccess(oAdminAccess);
            this.addPath(oPath, true);

            //Aggiunge vpath vfs
            oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_VFS;
            oPath.Physical = System.IO.Path.GetDirectoryName(this.mVfsXmlPath);
            oPath.AddAccess(oAdminAccess);
            this.addPath(oPath, true);

            //Aggiunge vpath temp
            oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_TEMP;
            oPath.Physical = Utility.MapVirtualPath(HfsData.TempDirUserFiles);
            oPath.AddAccess(new VfsAccess(oAnyUser, Const.VFS_ACCESS_ALL));
            this.addPath(oPath, true);
        }


        /// <summary>
        /// Aggiunge utente
        /// </summary>
        /// <param name="user"></param>
        private void addUser(VfsUser user, bool addSystem)
        {
            //Verifica preventiva se di sistema
            if (user.IsSystem && !addSystem)
                throw new ArgumentException(string.Format("L'utente '{0}' e' un utente di sistema gia' definito", user.Name));

            //Verifica se gia' esiste
            if (this.Users.ContainsKey(user.Name))
                throw new ArgumentException(string.Format("L'utente '{0}' e' gia' definito", user.Name));

            //Aggiunge utente
            this.Users.Add(user.Name, user);
        }


        /// <summary>
        /// Metodo privato che consente aggiunta dei system
        /// </summary>
        /// <param name="path"></param>
        private void addPath(VfsPath path, bool addSystem)
        {
            //Verifica preventiva se di sistema
            if (path.IsSystem && !addSystem)
                throw new ArgumentException(string.Format("Il virtual path '{0}' e' un path di sistema gia' definito", path.Virtual));

            //Verifica se gia' esiste
            if (this.Paths.ContainsKey(path.Virtual))
                throw new ArgumentException(string.Format("Il Path Virtuale '{0}' e' gia' definito", path.Virtual));

            //Aggiunge utente
            this.Paths.Add(path.Virtual, path);
        }

        #endregion
    }
}
