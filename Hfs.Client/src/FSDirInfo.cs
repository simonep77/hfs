using System;
using System.Collections.Generic;
using System.Text;

namespace Hfs.Client
{


        /// <summary>
        ///     ''' Classe contenente info su directory remota
        ///     ''' </summary>
        public sealed class FSDirInfo
        {
            public string Name { get; }
        public string FullName { get; }
        public DateTime CreationTime { get; }
        public IFSPermission Permission { get; }


        internal FSDirInfo(string name, DateTime ctime, IFSPermission permission)
            {
                this.FullName = name;
                this.Name = System.IO.Path.GetFileName(name);
                this.CreationTime = ctime;
                this.Permission = permission;
            }
        }

}
