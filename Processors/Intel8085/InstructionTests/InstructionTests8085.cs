using System;
using System.Collections.Generic;
using Processors;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace Intel8085.InstructionTests
{
    public static class InstructionTests8085
    {
        public static List<IInstructionTest> Tests8085
        {
            get
            {
                return new List<IInstructionTest>()
                {
                    { new OneIteration() },
                    //Arithmetic Group
                    { new AddInstructionTest() },
                    { new DadInstructionTest() },
                    { new InxInstructionTest() },
                    { new DcxInstructionTest() },
                    { new LxiInstructionTest() },
                    { new AdcInstructionTest() },
                    { new AdiInstructionTest() },
                    { new AciInstructionTest() },
                    { new SubInstructionTest() },
                    { new SbbInstructionTest() },
                    { new SuiInstructionTest() },
                    { new SbiInstructionTest() },
                    { new DaaInstructionTest() },
                    { new InrInstructionTest() },
                    { new DcrInstructionTest() },

                    //Logical Group
                    { new CmpInstructionTest() },
                    { new CpiInstructionTest() },
                    { new AnaInstructionTest() },
                    { new AniInstructionTest() },
                    { new XraInstructionTest() },
                    { new XriInstructionTest() },
                    { new OraInstructionTest() },
                    { new OriInstructionTest() },
                    { new RlcInstructionTest() },
                    { new RrcInstructionTest() },
                    { new RalInstructionTest() },
                    { new RarInstructionTest() },
                    { new CmaInstructionTest() },
                    { new CmcInstructionTest() },
                    { new StcInstructionTest() },

                    //Data Transfer Group
                    { new MovInstructionTest() },
                    { new MviInstructionTest() },
                    { new LdaInstructionTest() },
                    { new LdaxInstructionTest() },
                    { new LhldInstructionTest() },
                    { new StaInstructionTest() },
                    { new StaxInstructionTest() },
                    { new ShldInstructionTest() },
                    { new xchgInstructionTest() },
                    { new sphlInstructionTest() },
                    { new xthlInstructionTest() },
                    { new PushInstructionTest() },
                    { new PopInstructionTest() }
                };
            }
        }
    }

    public static class MyExtensions
    {
        public static byte High(this ushort b) { return (byte)((b >> 8) & 0xff); }
        public static byte Low(this ushort b) { return (byte)(b & 0xff); }
    }
}