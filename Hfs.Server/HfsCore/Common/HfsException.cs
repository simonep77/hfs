using System;

namespace Hfs.Server.CODICE.CLASSI
{
    public class HfsException: ApplicationException
    {
        private EStatusCode mStatusCode;

        public EStatusCode StatusCode
        {
            get
            {
                return this.mStatusCode;
            }
        }

        public HfsException(EStatusCode code, string msgFmt, params object[] args)
            :base(string.Format(msgFmt, args))
        {
            this.mStatusCode = code;
        }
    }
}
