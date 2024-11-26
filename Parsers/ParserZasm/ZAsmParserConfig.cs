using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Design;
using System.Windows.Forms;
using System.ComponentModel;
using Preferences;

namespace ParserZasm
{
    public class ZAsmParserConfig : PreferencesBase
    {
        public static string Key = "Zasm Z80 Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserZasm\zasm\zasm.exe";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = "-uxb {0}";

        public override void Default()
        {
            AssemblerArguments = "-uxb {0}";
            AssemblerPath = DefaultAssembler;
        }
    }
}