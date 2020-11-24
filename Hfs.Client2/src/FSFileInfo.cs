using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Hfs.Client
{

    /// <summary>
    ///     ''' Classe contenente informazioni su file remoto
    ///     ''' </summary>
    public sealed class FSFileInfo
    {
        public string FullName { get; }
        public string Name { get; }
        public string Extension { get; }
        public string Directory { get; }
        public long Length { get; }
        public string MimeType { get; }
        public FileAttributes Attributes { get; }
        public DateTime CreationTime { get; }
        public DateTime LastWriteTime { get; }
        public IFSPermission Permission { get; }



        internal FSFileInfo(string fullname, long len, FileAttributes attr, DateTime ctime, DateTime wtime, IFSPermission permission)
        {
            this.FullName = fullname;
            this.Name = System.IO.Path.GetFileName(fullname);
            this.Extension = System.IO.Path.GetExtension(fullname);
            this.Directory = fullname.Remove(fullname.LastIndexOf(this.Name) - 1);
            this.Length = len;
            this.Attributes = attr;
            this.CreationTime = ctime;
            this.LastWriteTime = wtime;
            this.Permission = Permission;
            this.MimeType = MimeHelper.GetMimeFromFilename(this.Name);
        }
    }

}
