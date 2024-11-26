using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Preferences;

namespace Embest
{
    public class EmbestBoardConfig : PreferencesBase
    {
        public static string Key { get { return "EmbestBoardConfig"; } }

        public bool HighVectors { get; set; }

        public override void Default()
        {
            HighVectors = false;
        }

    }
}
