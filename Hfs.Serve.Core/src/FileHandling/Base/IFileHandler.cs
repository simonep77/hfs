using System;
using System.Collections.Generic;
using System.IO;


namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// Interfaccia di gestione file
    /// </summary>
    public interface IFileHandler: IDisposable
    {
        string FullName { get; }
        
        string Name { get; }

        string Extension { get; }

        bool Exist();

        void Delete();

        string GetInfoXml();

        void MoveTo(IFileHandler file);

        void CopyTo(IFileHandler file);

        void SetAttribute(FileAttributes attr);

        void SendByMail(string mailTo, string mailFrom, string mailSubj, string mailBody);

        string GetSHA1();

        Stream OpenRead();

        Stream OpenWrite(bool overwrite);

    }
}
