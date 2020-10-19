using System;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.CODICE.VFS
{
    /// <summary>
    /// Definizione accesso a risorsa
    /// </summary>
    public class VfsAccess
    {
        private VfsUser mUser;
        private VfsAction mAccess = VfsAction.None;

        /// <summary>
        /// Utente associato
        /// </summary>
        public VfsUser User
        {
            get { return this.mUser; } 
        }


        public VfsAccess(VfsUser user, string access)
        {
            this.mUser = user;

            for (int i = 0; i < access.Length; i++)
            {
                switch (access[i])
                {
                    case Const.VFS_ACCESS_LIST:
                        this.mAccess |= VfsAction.List;
                        break;
                    case Const.VFS_ACCESS_READ:
                        this.mAccess |= VfsAction.Read;
                        break;
                    case Const.VFS_ACCESS_WRITE:
                        this.mAccess |= VfsAction.Write;
                        break;
                    case Const.VFS_ACCESS_DELETE:
                        this.mAccess |= VfsAction.Delete;
                        break;
                    default:
                        throw new ArgumentException(string.Format("Carattere '{0}' non riconosciuto come valido identificativo di accesso", access[i]));
                }
            }
        }

        /// <summary>
        /// Verifica ACL
        /// </summary>
        /// <param name="act"></param>
        /// <returns></returns>
        public bool Check(VfsAction act)
        {
            return (this.mAccess & act) == act;
        }

        /// <summary>
        /// Ritorna rappresentazione in stringa
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string sBase = string.Empty;
            if (this.Check(VfsAction.List))
                sBase += Const.VFS_ACCESS_LIST;
            if (this.Check(VfsAction.Read))
                sBase += Const.VFS_ACCESS_READ;
            if (this.Check(VfsAction.Write))
                sBase += Const.VFS_ACCESS_WRITE;
            if (this.Check(VfsAction.Delete))
                sBase += Const.VFS_ACCESS_DELETE;

            return sBase;
        }


    }
}