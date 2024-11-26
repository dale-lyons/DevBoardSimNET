using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace SDK_85
{
    [DataContract]
    public class SDKConfig
    {
        public static string Key = "SDK-85 Development Kit";

        public bool UseSecondROM { get; set; }
        public string SecondROMImage { get; set; }
        public bool SecondRAM { get; set; } = false;
    }
}
