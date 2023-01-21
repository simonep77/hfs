using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hfs.Server.CODICE.CLASSI;

namespace Hfs.Server.CODICE.CLASSI.FileHandling
{
    /// <summary>
    /// File handler con funzioni base
    /// </summary>
    internal static class FileDirHandlerFactory
    {
        static FileDirHandlerFactory()
        {
            //Registra nuovi uri hfs
            UriParser.Register(new HttpStyleUriParser(), Const.URI_HFS, 80);
            UriParser.Register(new HttpStyleUriParser(), Const.URI_HFS_SECURE, 443);
        }

        /// <summary>
        /// Ritorna oggetto file
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static async Task<IFileHandler> GetFileFromHfsResp(HfsResponseVfs resp)
        {
            //Gestione File locale
            switch (resp.Path.PathType)
            {
                case EVfsPathType.Local:
                    return new FileHandlerLocal(resp);
                case EVfsPathType.Hfs:
                    Uri u = new Uri(resp.Path.Physical);
                    return new FileHandlerHfs(resp, u);
                case EVfsPathType.SFtp:
                    return new FileHandlerSFtp(resp);
                case EVfsPathType.Ftp:
                    return new FileHandlerFtp(resp);
                default:
                    throw new ApplicationException("Tipo di path non gestito.");
            }

        }


        /// <summary>
        /// Ritorna oggetto Directory
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static async Task<IDirHandler> GetDirFromHfsResp(HfsResponseVfs resp)
        {
            
            switch (resp.Path.PathType)
            {
                case EVfsPathType.Local:
                    return new DirHandlerLocal(resp);
                case EVfsPathType.Hfs:
                    return new DirHandlerHfs(resp);
                case EVfsPathType.SFtp:
                    return new DirHandlerSFtp(resp);
                case EVfsPathType.Ftp:
                    return new DirHandlerFtp(resp);
                default:
                    throw new ApplicationException("Tipo di path non gestito.");
            }

        }
        
    }
}