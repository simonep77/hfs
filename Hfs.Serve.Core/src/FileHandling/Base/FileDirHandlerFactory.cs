using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;

namespace Hfs.Server.Core.FileHandling
{
    /// <summary>
    /// File handler con funzioni base
    /// </summary>
    internal static class FileDirHandlerFactory
    {
        static FileDirHandlerFactory()
        {
            
        }

        /// <summary>
        /// Ritorna oggetto file
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static IFileHandler GetFileFromHfsResp(HfsResponseVfs resp)
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
                case EVfsPathType.S3:
                    return new FileHandlerS3(resp);
                default:
                    throw new ApplicationException("Tipo di path non gestito.");
            }

        }


        /// <summary>
        /// Ritorna oggetto Directory
        /// </summary>
        /// <param name="resp"></param>
        /// <returns></returns>
        public static IDirHandler GetDirFromHfsResp(HfsResponseVfs resp)
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
                case EVfsPathType.S3:
                    return new DirHandlerS3(resp);
                default:
                    throw new ApplicationException("Tipo di path non gestito.");
            }

        }
        
    }
}