using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Design;
using System.Drawing.Design;
using System.Windows.Forms;

using Preferences;

namespace ParserAsm11
{
    public class Asm11ParserConfig : PreferencesBase
    {
        public static string Key = "M68C11 Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserAsm11\asm11\asm11.exe";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = "-L {0}.lst -O {1}.hex {2}";

        public override void Default()
        {
            AssemblerPath = DefaultAssembler;
            AssemblerArguments = "-L {0}.lst -O {1}.hex {2}";
        }
    }
}