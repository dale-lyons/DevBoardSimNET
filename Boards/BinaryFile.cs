using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Processors;

namespace Boards
{
    public class BinaryFile
    {
        public static void Create(string binFilename, ISystemMemory sm, OrgSections sections)
        {
            uint start = sections.StartAddress;
            uint end = sections.EndAddress;

            var ms = new MemoryStream();

            for(uint ptr = start; ptr < end; ptr++)
                ms.WriteByte((byte)sm.GetMemory((uint)ptr, WordSize.OneByte, false));

            File.WriteAllBytes(binFilename, ms.ToArray());
        }
    }
}