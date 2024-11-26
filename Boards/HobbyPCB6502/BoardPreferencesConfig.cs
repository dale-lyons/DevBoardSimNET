using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HobbyPCB6502
{
    [DataContract]
    public class BoardPreferencesConfig : Preferences.PreferencesBase
    {
        public static string Key = "HobbySBCs 6502 Kit";

        [Description("Indicates use of custom ROM image.")]
        [DisplayName("Allow the use of a customr ROM Image.")]
        public bool UseCustomBootROM { get; set; } = true;

        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
        [Description("Image file to load into ROM.")]
        [DisplayName("Allow the use of an Image file.")]
        public string RomImage { get; set; } = @"C:\Projects\DevBoardSimNET\Boards\HobbyPCB6502\ROM\PBW65C02CEGMON.bin";

        public override void Default()
        {
            UseCustomBootROM = true;
            RomImage = @"C:\Projects\DevBoardSimNET\Boards\HobbyPCB6502\ROM\PBW65C02CEGMON.bin";
        }
    }
}
