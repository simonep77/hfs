using System;
using Hfs.Server.CODICE.CLASSI;


namespace Hfs.Server.CODICE.VFS
{
    /// <summary>
    /// Identifica un utente VFS
    /// </summary>
    public class VfsUser
    {
        private Vfs mVfs;
        private string mName = string.Empty;
        private string mPass = string.Empty;

        public string Name
        {
            get { return this.mName; }
            set { this.mName = (value != null) ? value.Trim().ToLower() : string.Empty; }
        }

        public string Pass
        {
            get { return this.mPass; }
            set { this.mPass = (value != null) ? value : string.Empty; }
        }

        /// <summary>
        /// Indica se l'utente e' di sistema
        /// </summary>
        public bool IsSystem
        {
            get 
            { 
                switch (this.mName)
	            {
                    case Const.VFS_USER_ADMIN:
                    case Const.VFS_USER_GUEST:
                    case Const.VFS_USER_ANY:
                    case Const.VFS_USER_AUTH:
                        return true;
		            default:
                        return false;
	            }
            }
        }

        /// <summary>
        /// Indica se utente e' admin
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return this.mName.Equals(Const.VFS_USER_ADMIN);
            }
        }

        /// <summary>
        /// Indica se utente e' guest
        /// </summary>
        public bool IsGuest
        {
            get
            {
                return this.mName.Equals(Const.VFS_USER_GUEST);
            }
        }

        /// <summary>
        /// Indica utente speciale * o ?
        /// </summary>
        public bool IsSpecial
        {
            get
            {
                return (this.mName.Equals(Const.VFS_USER_ANY) || this.mName.Equals(Const.VFS_USER_AUTH));
            }
        }


        /// <summary>
        /// Costruttore
        /// </summary>
        /// <param name="vfs"></param>
        public VfsUser(Vfs vfs)
        {
            this.mVfs = vfs;
        }

    }
}
