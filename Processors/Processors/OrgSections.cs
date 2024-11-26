using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Processors
{
    public class OrgSections
    {
        public IList<OrgSection> Sections { get; } = new List<OrgSection> { new OrgSection(0) };

        public OrgSection StartSection { get { return Sections[0]; } }

        public OrgSection TopSection { get { return Sections[Sections.Count - 1]; } }

        public OrgSection NewSection(ushort addr)
        {
            var os = new OrgSection(addr);
            Sections.Add(os);
            return os;
        }

        public uint StartAddress
        {
            get
            {
                uint start = uint.MaxValue;
                for (int ii = Sections.Count - 1; ii >= 0; ii--)
                {
                    var os = Sections[ii];
                    if (os.IsDataDefined && os.StartAddress < start)
                        start = os.StartAddress;
                }
                return start;
            }
        }

        public uint EndAddress
        {
            get
            {
                uint end = 0;
                for (int ii = 0; ii < Sections.Count; ii++)
                {
                    var os = Sections[ii];
                    if (os.IsDataDefined && os.EndAddress > end)
                        end = os.EndAddress;
                }
                return end;
            }
        }
    }

    public class OrgSection
    {
        public bool IsDataDefined { get; set; }
        public uint StartAddress { get; set; }
        public uint EndAddress { get; set; }
        public OrgSection(uint addr)
        {
            StartAddress = addr;
        }
    }
}