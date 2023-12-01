using System;
using System.Collections.Generic;
using System.Web;
using System.Reflection;

namespace Hfs.Server.Core.Common
{
    static public class ApplicationInfo
    {
        /// <summary>
        /// Versione
        /// </summary>
        public static Version? Version => Assembly.GetCallingAssembly()?.GetName()?.Version; 


        public static string FileVersion => (Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false).FirstOrDefault() as AssemblyFileVersionAttribute)?.Version ?? "Unspecified"; 
       
       

    }
}