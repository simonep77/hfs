using System;

namespace Hfs.Server.Core.Common
{
    public class HfsException: ApplicationException
    {
        public EStatusCode StatusCode { get; }
      
        public HfsException(EStatusCode code, string text)
            :base(text)
        {
            this.StatusCode = code;
        }
    }
}
