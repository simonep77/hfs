using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Hfs.Server.CODICE.UTILITY
{
    public class NullableStringDictionary : Dictionary<string, string>
    {

        public new string this[string key]
        {
            get
            {
                string ret = null;

                this.TryGetValue(key, out ret);

                return ret;
            }
            set
            {
                base[key] = value;
            }
        }

    }
}