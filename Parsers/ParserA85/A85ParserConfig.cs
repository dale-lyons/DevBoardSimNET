using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Design;
using System.Windows.Forms;

using Preferences;

namespace ParserA85
{
    public class A85ParserConfig : PreferencesBase
    {
        public static string Key = "Intel 8085 Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserA85\A85\x64\Debug\a85.exe";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = "-L {0}.lst -O {1}.hex {2}";

        public override void Default()
        {
            AssemblerArguments = "-L {0}.lst -O {1}.hex {2}";
            AssemblerPath = DefaultAssembler;
        }
    }
}