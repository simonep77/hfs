using System;
using System.Collections.Generic;
using System.IO;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// Directory handler con funzioni base
    /// </summary>
    internal abstract class DirHandlerBase: IDirHandler
    {

        #region IDirHandler Membri di

        public abstract string FullName { get; }

        public abstract string Name { get; }

        public abstract bool Exist();

        public abstract void Create();

        public abstract void Delete();

        public abstract string GetFilesXml(string pattern, bool recursive);

        public abstract string GetDirsXml(string pattern, bool recursive);

        public abstract long GetSize();

        public void MoveTo(IDirHandler dir)
        {
            this.CopyTo(dir);
            this.Delete();
        }

        public void CopyTo(IDirHandler dir)
        {
            //Crea corrispondente
            dir.Create();

            //files
            IFileHandler[] files = this.GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                files[i].CopyTo(dir.GetFileHandler(files[i].Name));
            }

            //sub dir
            IDirHandler[] dirs = this.GetDirs();
            for (int i = 0; i < dirs.Length; i++)
            {
                dirs[i].CopyTo(dir.GetDirHandler(dirs[i].Name));
            }
        }

        public abstract IFileHandler GetFileHandler(string name);

        public abstract IDirHandler GetDirHandler(string name);

        public abstract IFileHandler[] GetFiles();

        public abstract IFileHandler[] GetFiles(string pattern);

        public abstract IDirHandler[] GetDirs();

        public abstract IDirHandler[] GetDirs(string pattern);

        public virtual void Dispose()
        { }

        #endregion
    }
}