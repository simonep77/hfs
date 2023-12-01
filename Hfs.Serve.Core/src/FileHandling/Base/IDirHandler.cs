using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// Interfaccia di gestione Directory
    /// </summary>
    public interface IDirHandler: IDisposable 
    {
        string FullName { get; }
        
        string Name { get; }

        bool Exist();

        void Create();

        void Delete();

        string GetFilesXml(string pattern, bool recursive);

        string GetDirsXml(string pattern, bool recursive);

        long GetSize();

        void MoveTo(IDirHandler dir);

        void CopyTo(IDirHandler dir);

        IFileHandler GetFileHandler(string name);

        IDirHandler GetDirHandler(string name);

        IFileHandler[] GetFiles();

        IFileHandler[] GetFiles(string pattern);

        IDirHandler[] GetDirs();

        IDirHandler[] GetDirs(string pattern);
    }
}
