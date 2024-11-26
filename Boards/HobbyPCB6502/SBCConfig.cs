using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace HobbyPCB6502
{
    [DataContract]
    public class SBCConfig
    {
        public static string Key = "HobbySBCs 6502 Kit";

        public bool Use65c02Intstructions { get; set; } = false;

        public bool UseCustomBootROM { get; set; } = true;
        public string RomImage { get; set; } = @"C:\Projects\DevBoardSimNET\Boards\HobbyPCB6502\ROM\PBW65C02CEGMON.bin";
    }
}
