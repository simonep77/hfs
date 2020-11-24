using System;
using System.Collections.Generic;
using System.IO;

namespace Hfs.Client
{

    public interface IFileSystem : IDisposable
    {

        /// <summary>
        ///         ''' Eventuale timeout per le operazioni
        ///         ''' </summary>
        ///         ''' <returns></returns>
        int TimeoutMSec { get; }

        /// <summary>
        ///         ''' Url Server
        ///         ''' </summary>
        string Url { get; }


        /// <summary>
        ///         ''' User name
        ///         ''' </summary>
        ///         ''' <value></value>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        string User { get; }


        /// <summary>
        ///         ''' Ultimo Codice Risposta
        ///         ''' </summary>
        int LastRespCode { get; }


        /// <summary>
        ///         ''' Ultimo Messaggio Risposta
        ///         ''' </summary>
        string LastRespMsg { get; }


        /// <summary>
        ///         ''' Directory corrente da cui partire per i comandi
        ///         ''' </summary>
        ///         ''' <value></value>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        string CurrentDirectory { get; set; }


        /// <summary>
        ///         ''' Controlla esistenza file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <returns></returns>
        bool FileExist(string vpath);


        /// <summary>
        ///         ''' Ritorna informazioni su file Immagine remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <returns></returns>
        string FileGetImageInfo(string vpath);

        /// <summary>
        ///         ''' Ritorna informazioni su file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <returns></returns>
        FSFileInfo FileGetInfo(string vpath);



        string FileGetLink(string vpath);


        /// <summary>
        ///         ''' Elimina file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileDelete(string vpath);


        /// <summary>
        ///         ''' Invia file remoto per email
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileEmail(string vpath, string to, string from, string subj, string body);


        /// <summary>
        ///         ''' legge file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        byte[] FileReadToBuffer(string vpath);

        /// <summary>
        ///         ''' legge file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="attr"></param>
        ///         ''' <returns></returns>
        byte[] FileReadToBuffer(string vpath, string attr);



        /// <summary>
        ///         ''' legge file remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileReadToDisk(string vpath, string localFile);


        /// <summary>
        ///         ''' Legge file su stream fornito in input
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="stream"></param>
        void FileReadToStream(string vpath, Stream stream);


        /// <summary>
        ///         ''' Scrive file con dati da buffer
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="buffer"></param>
        void FileWriteFromBuffer(string vpath, byte[] buffer);


        /// <summary>
        ///         ''' Scrive file partendo da file fisico
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="localFile"></param>
        void FileWriteFromDisk(string vpath, string localFile);


        /// <summary>
        ///         ''' Scrive file a partire da uno stream
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="stream">
        ///         ''' stream con posibilità di read e seek
        ///         ''' </param>
        void FileWriteFromStream(string vpath, Stream stream);


        /// <summary>
        ///         ''' Append file con dati da buffer
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="buffer"></param>
        void FileAppendFromBuffer(string vpath, byte[] buffer);


        /// <summary>
        ///         ''' Copia file remoto su altro remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileCopy(string vpath, string vpathdest);

        /// <summary>
        ///         ''' Sposta file remoto su altro remoto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileMove(string vpath, string vpathdest);


        /// <summary>
        ///         ''' Calcola l'hash SHA1 di un file remoto (da server)
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        string FileHashSHA1(string vpath);


        /// <summary>
        ///         ''' Calcola l'hash SHA1 di un file locale (client)
        ///         ''' </summary>
        ///         ''' <param name="localfile"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        string FileLocalHashSHA1(string localfile);


        /// <summary>
        ///         ''' Crea o azzera file
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void FileTouch(string vpath);


        /// <summary>
        ///         ''' Crea directory
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void DirectoryCreate(string vpath);


        /// <summary>
        ///         ''' Elimina directory e tutto il suo contenuto
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        void DirectoryDelete(string vpath);


        /// <summary>
        ///         ''' Sposta/Rinomina Directory
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="vpathdest"></param>
        void DirectoryMove(string vpath, string vpathdest);


        /// <summary>
        ///         ''' Copia Directory su altra
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <param name="vpathdest"></param>
        void DirectoryCopy(string vpath, string vpathdest);


        /// <summary>
        ///         ''' Elenca files presenti in directory remota con pattern
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        FSFileInfo[] DirectoryListFilesFilter(string vpath, string pattern, bool recursive);


        /// <summary>
        ///         ''' Elenca files presenti in directory remota
        ///         ''' </summary>
        FSFileInfo[] DirectoryListFiles(string vpath);


        /// <summary>
        ///         ''' Elenca files presenti in directory remota con pattern
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        FSDirInfo[] DirectoryListSubDirFilter(string vpath, string pattern, bool recursive);


        /// <summary>
        ///         ''' Elenca files presenti in directory remota 
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        FSDirInfo[] DirectoryListSubDir(string vpath);


        /// <summary>
        ///         ''' Verifica esistenza directory
        ///         ''' </summary>
        ///         ''' <param name="vpath"></param>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        bool DirectoryExist(string vpath);


        /// <summary>
        ///         ''' Ritorna elenco di directory ROOT
        ///         ''' </summary>
        ///         ''' <returns></returns>
        ///         ''' <remarks></remarks>
        FSDirInfo[] ListRootEntries();
    }
}

