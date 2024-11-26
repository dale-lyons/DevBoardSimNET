using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Preferences;

namespace ZilogZ80
{
    public class ZilogZ80ProcessorConfig : PreferencesBase
    {
        public static string Key = "Zilog Z80 Settings";
        public static string DefaultParser = "ParserZasm.ParserZasm";

        [Editor(typeof(ActiveParserEditor), typeof(UITypeEditor))]
        [Description("The Parser to use for this processor.")]
        [DisplayName("ActiveParser")]
        public string ActiveParser { get; set; } = DefaultParser;

        [Description("Allow the use of Undocumented Opcode instructions.")]
        [DisplayName("Allow the use of Undocumented Opcodes")]
        public bool AllowUndocumentedOpcodes { get; set; }

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]

        public override void Default()
        {
            AllowUndocumentedOpcodes = false;
            ActiveParser = DefaultParser;
        }
    }
}