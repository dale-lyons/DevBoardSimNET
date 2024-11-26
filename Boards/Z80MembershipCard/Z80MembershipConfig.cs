using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Preferences;

namespace Z80MembershipCard
{
    public class Z80MembershipConfig : PreferencesBase
    {
        public static string Key = "Z80 Membership Card Config";

        [Description("Indicates use of custom ROM image.")]
        [DisplayName("Allow the use of a customr ROM Image.")]
        public bool UseCustomBootROM { get; set; } = false;

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Image file to load into ROM.")]
        [DisplayName("Allow the use of an Image file.")]
        public string RomImage { get; set; } = @"C:\Projects\DevBoardSimNET\Boards\HobbyPCB6502\ROM\PBW65C02CEGMON.bin";

        public override void Default()
        {
            UseCustomBootROM = false;
        }
    }
}