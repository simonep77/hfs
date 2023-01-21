
namespace Hfs.Server.CODICE.CLASSI
{

    /// <summary>
    /// Costanti varie
    /// </summary>
    public class Const
    {
        /*
         Costanti generiche
         */
        public const int HFS_LINK_SECONDS = 600;
        public const int HFS_LINK_PADLEN = 20;
        public const char HFS_LINK_PADCHAR = '$';
        public const string HFS_LINK_DATEFMT = @"yyyyMMddHHmmss";
        public const string HFS_ENC_KEY = @"31178uS8Lh9MgIO7786RsUKX6sUns8qC";

        public const long BLOCK_LENGHT_MAX = -1;
        public const long BLOCK_OFFSET_ZERO = 0;
        public static FileBlock BLOCK_DEFAULT = new FileBlock();
        public const int BUFFER_LEN = 128 * 1024;

        /*
         Costanti LOG
         */
        public const string LOG_SEPARATOR = @"=========================================================================";
        public const char URI_SEPARATOR = '/';
        public const string LOG_GLOBAL_FILENAME_FMT = @"hfs_global_{0:yyyy_MM_dd}.log";
        public const string LOG_ACCESS_FILENAME_FMT = @"hfs_access_{0:yyyy_MM_dd}.log";
        public const string LOG_PATTERN = @"hfs_*.log";
        public const string LOG_ACCESS_MSG_FMT = @"Accesso: {0,-6} - Azione: {1,-12} - User: {2,-12} - IP: {3,-15} - Risorsa: {4}";


        /*
         Costanti Header HFS
         */
        public const string HEADER_STATUS_CODE = @"HFS-STATUS-CODE";
        public const string HEADER_STATUS_MSG = @"HFS-STATUS-MSG";
        public const string HEADER_CONTENT_LEN = @"Content-Length";
        public const string HEADER_CONTENT_TYPE_BIN = @"binary/octetstream";
        public const string HEADER_CONTENT_TYPE_TEXT = @"text/html";
        public const string HEADER_CONTENT_DISP = @"Content-Disposition";
        public const string HEADER_CONTENT_DISP_VAL = @"inline;filename={0}";


        /*
         Costanti Parametri
         */
        public const string QS_HEADER_PREFIX = @"hfs-";
        public const string QS_LINK = @"hfs-link";
        public const string QS_USER = @"hfs-user";
        public const string QS_PASS = @"hfs-pass";
        public const string QS_VPATH = @"hfs-vpath";
        public const string QS_VPATH_DEST = @"hfs-vpathdest";
        public const string QS_VPATH_PATTERN = @"hfs-pattern";
        public const string QS_ACTION = @"hfs-action";
        public const string QS_READ_BLOCK = @"hfs-block";
        public const string QS_MAIL_TO = @"hfs-mailto";
        public const string QS_MAIL_FROM = @"hfs-mailfrom";
        public const string QS_MAIL_SUBJ = @"hfs-mailsubj";
        public const string QS_MAIL_BODY = @"hfs-mailbody";
        public const string QS_ATTR = @"hfs-attr";

        /*
         Costanti Response
         */
        public const string RESP_TEXT_TRUE = @"1";
        public const string RESP_TEXT_FALSE = @"0";

        /*
         Costanti VFS
         */
        public const string VFS_PATH_ADMIN = @"/hfs-sys-adm";
        public const string VFS_PATH_LOG = @"/hfs-log";
        public const string VFS_PATH_VFS = @"/hfs-vfs";

        public const string VFS_PATH_TEMP = @"/temp";
        public const string VFS_USER_ADMIN = @"admin";
        public const string VFS_USER_GUEST = @"guest";
        public const string VFS_USER_ANY = @"*";
        public const string VFS_USER_AUTH = @"?";
        public const int VFS_LOAD_WAIT_MSEC = 5000;
        public const int VFS_LOAD_CHECK_MSEC = 20;

        public const string VFS_ACCESS_ALL = @"RWDL";
        public const char VFS_ACCESS_READ = 'R';
        public const char VFS_ACCESS_WRITE = 'W';
        public const char VFS_ACCESS_DELETE = 'D';
        public const char VFS_ACCESS_LIST = 'L';

        /* URI */

        /// <summary>
        /// Schema uri hfs
        /// </summary>
        public const string URI_HFS = @"hfs";

        /// <summary>
        /// Schema uri hfs su ssl
        /// </summary>
        public const string URI_HFS_SECURE = @"hfss";


        /// <summary>
        /// Tipo conversione swf
        /// </summary>
        public const string HFS_CONVERT_TYPE_SWF = @"swf";

        /// <summary>
        /// Tipo conversione swf
        /// </summary>
        public const string HFS_CONVERT_TYPE_PDF = @"pdf";


        /// <summary>
        /// Costanti relative alla gestione SFTP
        /// </summary>
        public static class SFTP_File_Handling
        {
            public const string PATH_PARAM_HOST = @"SFTP_HOST";
            public const string PATH_PARAM_PORT = @"SFTP_PORT";
            public const string PATH_PARAM_USER = @"SFTP_USER";
            public const string PATH_PARAM_PASS = @"SFTP_PASS";
            public const string PATH_PARAM_CURRDIR = @"SFTP_CURRDIR";
            public const string PATH_PARAM_KEYBASE64 = @"SFTP_KEYBASE64";
        }

        /// <summary>
        /// Costanti relative alla gestione FTP
        /// </summary>
        public static class FTP_File_Handling
        {
            public const string PATH_PARAM_HOST = @"FTP_HOST";
            public const string PATH_PARAM_PORT = @"FTP_PORT";
            public const string PATH_PARAM_USER = @"FTP_USER";
            public const string PATH_PARAM_PASS = @"FTP_PASS";
            public const string PATH_PARAM_CURRDIR = @"FTP_CURRDIR";
            public const string PATH_PARAM_PASSIVE = @"FTP_PASSIVE";

        }

        public static class SMB_File_Handling
        {
            public const string PATH_PARAM_USER = @"SMB_USER";
            public const string PATH_PARAM_PASS = @"SMB_PASS";

        }


    }
}
