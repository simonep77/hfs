using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Web;

namespace Hfs.Server.Core.Common
{

    /// <summary>
    /// Metodiper gestione Mime
    /// </summary>
    public class MimeHelper
	{
        private static FileExtensionContentTypeProvider _provider = new FileExtensionContentTypeProvider();
        
        /// <summary>
        /// Ritorna mime type da nome di file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string GetMimeFromFilename(string filename)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(filename, out var contentType);
            return contentType ?? "application/octet-stream";

        }


        /// <summary>
        /// Ritorna mime da estensione
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static string GetMimeFromExtension(string ext)
        {
            return GetMimeFromFilename($"a{ext}");
        }
	}
}