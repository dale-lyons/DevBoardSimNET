using Preferences;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BenEater
{
    public class BenEaterProcessorConfig : PreferencesBase
    {
        public static string Key = "Ben Eater Processor Settings";

        public override void Default()
        {
        }
    }
}
