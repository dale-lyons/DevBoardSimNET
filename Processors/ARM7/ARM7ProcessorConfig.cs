using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Design;
using System.Design;
using Preferences;

namespace ARM7
{
    public class ARM7ProcessorConfig : Preferences.PreferencesBase
    {
        public static string Key { get { return "ARM7ProcessorConfig"; } }
        public static string DefaultParser = "ParserEabi.ParserEabi";

        [Editor(typeof(ActiveParserEditor), typeof(UITypeEditor))]
        [Description("The Parser to use for this processor.")]
        [DisplayName("ActiveParser")]
        public string ActiveParser { get; set; } = DefaultParser;

        [Description("Stop the simulation if a Word is accessed unaligned")]
        [DisplayName("TrapUnalignedMemoryAccess")]
        public bool TrapUnalignedMemoryAccess { get; set; }

        [Description("Stop the simulation if a illegal Opcode is encountered")]
        [DisplayName("TrapUnusedOpcode")]
        public bool TrapUnusedOpcode { get; set; }

        public override void Default()
        {
            TrapUnalignedMemoryAccess = false;
            TrapUnusedOpcode = false;
            ActiveParser = DefaultParser;
    }
}
}