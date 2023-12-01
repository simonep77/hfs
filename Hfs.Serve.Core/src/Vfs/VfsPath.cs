using System;
using System.IO;
using System.Collections.Generic;
using Hfs.Server.Core.Common;

namespace Hfs.Server.Core.Vfs
{
    /// <summary>
    /// Identifica un path
    /// </summary>
    public class VfsPath
    {
        private VfsHandler mVfs;
        private string mVirtual = string.Empty;
        private string mPhysical = string.Empty;
        private EVfsPathType mPathType = EVfsPathType.Local;
        private Dictionary<string, VfsAccess> UserAuth = new Dictionary<string, VfsAccess>(10);
        private NullableStringDictionary mParams = new NullableStringDictionary();

        public string Virtual
        {
            get { return this.mVirtual; }
            set { this.mVirtual = (value != null) ? value.Trim().ToLower() : string.Empty; }
        }

        public string Physical
        {
            get { return this.mPhysical; }
            set 
            { 
                this.mPhysical = (value != null) ? value.Trim() : string.Empty;
            }
        }

        /// <summary>
        /// Indica il tipo di path
        /// </summary>
        public EVfsPathType PathType
        {
            get { return this.mPathType; }
            set
            {
                this.mPathType = value;
            }
        }


        public bool IsLocal
        {
            get { return this.mPathType == EVfsPathType.Local; }
        }


        /// <summary>
        /// Indica se network share remota
        /// </summary>
        public bool IsUnc
        {
            get 
            {
                return this.IsLocal
                  && this.Physical.StartsWith(@"\\", StringComparison.Ordinal);  
            }
        }


        /// <summary>
        /// Indica che trattasi di network share con credenziali specifiche
        /// </summary>
        public bool IsUncWithCredentials
        {
            get { return this.IsUnc 
                    && !string.IsNullOrEmpty(this.Params[Const.SMB_File_Handling.PATH_PARAM_USER]); }
        }

        /// <summary>
        /// Indica se il path e' di sistema
        /// </summary>
        public bool IsSystem
        {
            get
            {
                switch (this.mVirtual)
                {
                    case Const.VFS_PATH_ADMIN:
                    case Const.VFS_PATH_TEMP:
                    case Const.VFS_PATH_VFS:
                    case Const.VFS_PATH_LOG:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public NullableStringDictionary Params
        {
            get { return this.mParams; }
        }

        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="vfs"></param>
        public VfsPath(VfsHandler vfs)
        {
            this.mVfs = vfs;
        }

        /// <summary>
        /// Dato un path virtuale ritorna il path fisico
        /// </summary>
        /// <param name="vPath"></param>
        /// <returns>Se stringa vuota significa che non c'e' coincidenza</returns>
        public string Translate(string vPath, VfsUser user)
        {
            //Se uri allora ritorna il virtuale
            if (!this.IsLocal)
                return vPath;

            //Ulteriore test
            if (vPath.ToLower().IndexOf(this.mVirtual) < 0)
                throw new ArgumentException(string.Format("Il path '{0}' fornito non appartiene al virtual path corrente '{1}'", vPath, this.mVirtual));

            string sPhysical = this.mPhysical;
            //Se temp allora ritorna una rappresentazione per utente
            if (this.mVirtual.Equals(Const.VFS_PATH_TEMP))
            {
                sPhysical = Path.Combine(sPhysical, user.Name);
                Directory.CreateDirectory(sPhysical);
            }
            //Ritorna path fisico
            return Path.Combine(sPhysical, vPath.Remove(0, this.mVirtual.Length).Trim(Const.URI_SEPARATOR).Replace(Const.URI_SEPARATOR, System.IO.Path.DirectorySeparatorChar));

        }

        /// <summary>
        /// Verifica permessi utente
        /// </summary>
        /// <param name="user"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public void CheckAccess(VfsUser user, VfsAction action)
        {
            if (action == VfsAction.None)
                return;

            if (this.GetAccess(user)?.Check(action) ?? false)
                return;

            //Accesso non consentito
            throw new HfsException(EStatusCode.NotAllowed ,$"Utente '{user.Name}' non autorizzato all'operazione '{action}' su '{this.Virtual}'");
        }

        /// <summary>
        /// Ritorna permessi utente
        /// </summary>
        /// <param name="user"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public VfsAccess GetAccess(VfsUser user)
        {
            VfsAccess oAccess = null;

            if (UserAuth.TryGetValue(user.Name, out oAccess))
            {
                //Nel caso ritorna false
                return oAccess;
            }
            else
            {
                //Se non siamo ne caso any
                if (!user.IsSpecial)
                {
                    //Verifico ANY in ricorsione
                    oAccess = this.GetAccess(this.mVfs.GetUser(Const.VFS_USER_ANY));
                    if (oAccess != null)
                        return oAccess;

                    //Se non e' guest allora verifico AUTH
                    if (!user.Name.Equals(Const.VFS_USER_GUEST))
                    {
                        oAccess = this.GetAccess(this.mVfs.GetUser(Const.VFS_USER_AUTH));
                        if (oAccess != null)
                            return oAccess;
                    }
                }
            }

            //In qualunque altro caso
            return null;
        }



        /// <summary>
        /// Aggiunge accesso
        /// </summary>
        /// <param name="acc"></param>
        public void AddAccess(VfsAccess access)
        {

            //Verifica preventiva
            if (this.UserAuth.ContainsKey(access.User.Name))
            {
                throw new ApplicationException(string.Format("Il Path Virtuale '{0}' contine gia' una definizione di accesso per l'utente '{1}'", this.mVirtual, access.User.Name));
            }

            //Aggiunge 
            this.UserAuth.Add(access.User.Name, access);
        }

        /// <summary>
        /// Rimuove accesso
        /// </summary>
        /// <param name="user"></param>
        public void RemoveAccess(VfsUser user)
        {
            //Verifica preventiva
            if (this.UserAuth.ContainsKey(user.Name))
            {
                if (this.IsSystem && user.IsSystem)
                    throw new ArgumentException(string.Format("Impossibile rimuovere i permessi per il path di sistema '{0}'", this.mVirtual));

                if (user.IsAdmin)
                    throw new ArgumentException(string.Format("Impossibile rimuovere i permessi per l'utente '{0}'", user.Name));

                //OK, elimina
                this.UserAuth.Remove(user.Name);
            }
        }

    }
}
