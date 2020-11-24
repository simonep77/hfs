using System;
using System.Collections.Generic;
using System.Text;

namespace Hfs.Client
{

    /// <summary>
    ///     ''' Classe contenente permessi HFS
    ///     ''' </summary>
    public sealed class FSPermissionHFS : IFSPermission
    {

        public string Access { get; }



        internal FSPermissionHFS(string access)
        {
            this.Access = access;
        }

        public bool CanDelete
        {
            get
            {
                return this.Access.Contains("D");
            }
        }

        public bool CanList
        {
            get
            {
                return this.Access.Contains("L");
            }
        }

        public bool CanRead
        {
            get
            {
                return this.Access.Contains("R");
            }
        }

        public bool CanWrite
        {
            get
            {
                return this.Access.Contains("W");
            }
        }

        public override string ToString()
        {
            return this.Access;
        }
    }

}
