using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Preferences;

namespace Intel8085
{
    public class Intel8085ProcessorConfig : PreferencesBase
    {
        public static string Key = "Intel 8085 Processor Settings";
        public static string DefaultParser = "ParserAsm11.ParserAsm11";

        [Editor(typeof(ActiveParserEditor), typeof(UITypeEditor))]
        [Description("The Parser to use for this processor.")]
        [DisplayName("ActiveParser")]
        public string ActiveParser { get; set; } = DefaultParser;

        [Description("Allow the use of Undocumented Opcode instructions.")]
        [DisplayName("Allow the use of Undocumented Opcodes")]
        public bool AllowUndocumentedOpcodes { get; set; }

        public override void Default()
        {
            ActiveParser = DefaultParser;
            AllowUndocumentedOpcodes = false;
        }
    }
}