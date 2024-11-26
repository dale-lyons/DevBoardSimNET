using Preferences;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Design;
using System.Windows.Forms;

namespace ParserCa65
{
    public class Ca65ParserConfig : PreferencesBase
    {
        public static string Key = "Motorala 6502 Ca65 Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserCa65\ca65\ca65.exe";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = "-I common --listing {0}.lst -o {1}.o {2}";

        public override void Default()
        {
            AssemblerArguments = "-I common --listing {0}.lst -o {1}.o {2}";
            AssemblerPath = DefaultAssembler;
        }
    }
}