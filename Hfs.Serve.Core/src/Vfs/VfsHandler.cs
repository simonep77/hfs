using System;
using System.Text;
using System.Collections.Generic;
using Hfs.Server.Core.Common;
using System.Text.Json;
using System.Diagnostics;

namespace Hfs.Server.Core.Vfs
{
    /// <summary>
    /// Classe per la gestione di un Virtual File System
    /// </summary>
    public class VfsHandler
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private Dictionary<string, VfsUser> Users = new Dictionary<string, VfsUser>(10);
        private Dictionary<string, VfsPath> Paths = new Dictionary<string, VfsPath>(10);
        private string mVfsFilePath;
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
                if (!this._lock.TryEnterReadLock(Const.VFS_LOAD_WAIT_MSEC))
                    return false;

                try
                {
                    //Ritorna info
                    return this.mIsFullLoaded;
                }
                finally
                {
                    this._lock.ExitReadLock();
                }



            }
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Creazione vfs a partire da file
        /// </summary>
        /// <param name="xmlFilePath"></param>
        public VfsHandler(string xmlFilePath)
        {
            mVfsFilePath = xmlFilePath;
        }


        /// <summary>
        /// Carica Virtual File Systema da File 
        /// </summary>
        /// <param name="vfsFileName"></param>
        public void Load()
        {
            //Log avvio
            HfsData.WriteLog("Avvio caricamento vfs..");
            //Blocca altri
            this._lock.EnterWriteLock();
            try
            {
                //Svuota memoria corrente
                this.inizializeVfs();

                //Imposta Flag di caricamento
                this.mIsFullLoaded = false;

                VfsModelRoot? model = null;

                try
                {
                    model = JsonSerializer.Deserialize<VfsModelRoot>(File.ReadAllText(this.mVfsFilePath), new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip,
                    }) ?? throw new ArgumentException($"Il file {Path.GetFileName(this.mVfsFilePath)} non è un VFS valido");
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Errore caricamento VFS: " + ex.Message);
                }

                VfsUser oUser;
                VfsPath oPath;
                VfsAccess oAccess;
                VfsAccess oAdminAccess = new VfsAccess(this.GetUser(Const.VFS_USER_ADMIN), Const.VFS_ACCESS_ALL);

                //Carica elenco utenti globale
                foreach (var user in model.users)
                {
                    //Crea utente
                    oUser = new VfsUser(this)
                    {
                        Name = user.name,
                        Pass = user.pass,
                    };
                    //Aggiunge ad elenco globale
                    this.AddUser(oUser);
                }

                foreach (var path in model.paths)
                {
                    //Crea path
                    oPath = new VfsPath(this)
                    {
                        Virtual = path.virtualpath,
                        Physical = path.physicalpath,
                        PathType = Enum.Parse<EVfsPathType>(string.IsNullOrWhiteSpace(path.type) ? "local" : path.type, true),
                    };
                    //Aggiunge Admin
                    oPath.AddAccess(oAdminAccess);

                    //Unc non ammessi
                    if (path.physicalpath.StartsWith(@"//", StringComparison.Ordinal))
                    {
                        HfsData.WriteLog($"Percorso unc {oPath.Physical} non supportato. Montarlo esternamente");
                        continue;
                    }


                    //Crea la directory se non esiste
                    if (oPath.IsLocal)
                    {
                        try
                        {
                            System.IO.Directory.CreateDirectory(oPath.Physical);
                        }
                        catch (Exception ex)
                        {
                            HfsData.WriteLog($"Attenzione! impossibile creare directory fisica {oPath.Physical} per vpath {oPath.Virtual}");
                            HfsData.WriteLog(ex.Message);
                        }
                    }
                    //Aggiunge autorizzazioni
                    foreach (var auth in path.authorizations)
                    {
                        //Crea path
                        oAccess = new VfsAccess(this.GetUser(auth.name), auth.access);
                        //Aggiunge autorizzazione
                        oPath.AddAccess(oAccess);
                    }
                    //Aggiunge parametri
                    foreach (var par in path.parameters)
                    {
                        //Aggiunge parametro
                        oPath.Params.Add(par.key, par.value);
                    }
                    //Aggiunge alla lista
                    this.AddPath(oPath);
                }


                //Imposta
                this.mIsFullLoaded = true;
            }
            finally
            {
                //Sblocca altri
                this._lock.ExitWriteLock();
                //log fine
                HfsData.WriteLog("Fine caricamento vfs");
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
            if (!this._lock.TryEnterReadLock(Const.VFS_LOAD_WAIT_MSEC))
                throw new ApplicationException("Il sistema sta ricaricando la propria configurazione. Riprovare tra pochi secondi");
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

                //Calcola path virtuale e fisico
                oResp.VirtualPath = vpath;
                oResp.PhysicalPath = oResp.Path.Translate(oResp.VirtualPath, oResp.User);


                //Ritorna
                return oResp;
            }
            finally
            {
                this._lock.ExitReadLock();
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
            if (!this._lock.TryEnterReadLock(Const.VFS_LOAD_WAIT_MSEC))
                throw new ApplicationException("Il sistema sta ricaricando la propria configurazione. Riprovare tra pochi secondi");
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
                this._lock.ExitReadLock();
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
            oAdminUser.Pass = HfsData.AdminPass;
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


            //Aggiunge vpath vfs
            oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_VFS;
            oPath.Physical = HfsData.VfsFilePath;
            oPath.AddAccess(oAdminAccess);
            this.addPath(oPath, true);

            //Aggiunge vpath temp
            oPath = new VfsPath(this);
            oPath.Virtual = Const.VFS_PATH_TEMP;
            oPath.Physical = HfsData.TempDirUserFiles;
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
