using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parsers
{
    public sealed class ParseSettings
    {
        public string Path { get; set; } = System.Windows.Forms.Application.CommonAppDataPath;
        public bool Listing { get; set; } = false;
        public bool Hex { get;  set; } = false;
        public bool Bin { get;  set; } = false;
        public bool Debug { get;  set; } = false;
        
        public string[] IncludeDirs { get; set; }

    }
}
