using System;
using System.Collections.Generic;
using System.Web;

namespace Hfs.Server.CODICE.CLASSI
{
    /// <summary>
    /// Identifica una posizione all'interno di un file
    /// </summary>
    public class FileBlock
    {
        public long Offset = Const.BLOCK_OFFSET_ZERO;
        public long Length = Const.BLOCK_LENGHT_MAX;
    }
}