using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing.Design;
using System.Windows.Forms;
//using System.ComponentModel;
using Preferences;

namespace ParserTasm
{
    public class TasmParserConfig : PreferencesBase
    {
        public static string Key = "Tasm Z80 Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserTasm\tasm\tasm.exe";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = "-t80 {0}";

        public override void Default()
        {
            AssemblerArguments = "-t80 {0}";
            AssemblerPath = DefaultAssembler;
        }
    }
}