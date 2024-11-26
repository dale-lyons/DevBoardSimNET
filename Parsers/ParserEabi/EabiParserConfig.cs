using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Design;
using System.Windows.Forms;

using Preferences;

namespace ParserEabi
{
    public class EabiParserConfig : PreferencesBase
    {
        public static string Key = "ARM7 EABI Parser Settings";
        public static string DefaultAssembler = @"C:\Projects\DevBoardSimNET\Parsers\ParserEabi\eabi\arm-none-eabi-as.exe";
        public static string DefaultAssemblerArguments = @"-algn -march=armv7-a -mfpu=vfp -o {0} {1}";

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Location and name of assembler for this processor")]
        [DisplayName("AssemblerPath")]
        public string AssemblerPath { get; set; } = DefaultAssembler;

        [Description("Assembler arguments pattern for Assembler")]
        [DisplayName("AssemblerArguments")]
        public string AssemblerArguments { get; set; } = DefaultAssemblerArguments;

        [Description("Entry Point of Program")]
        [DisplayName("EntryPoint")]
        public string EntryPoint { get; set; } = "_start:";

        [Description(".text area Address")]
        [DisplayName("TextAreaAddress")]
        public uint TextAreaAddress { get; set; } = 0x1000;
        public uint? DataAreaAddress { get; set; } = null;

        public override void Default()
        {
            AssemblerArguments = DefaultAssemblerArguments;
            AssemblerPath = DefaultAssembler;
            EntryPoint = "_start:";
            TextAreaAddress = 0x1000;
        }
    }
}