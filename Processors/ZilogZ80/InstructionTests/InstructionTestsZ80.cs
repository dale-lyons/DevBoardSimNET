using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Processors;
using Intel8085.InstructionTests;

namespace ZilogZ80.InstructionTests
{
    public static class InstructionTestsZ80
    {
        public static List<IInstructionTest> TestsZ80
        {
            get
            {
                var list =  Intel8085.InstructionTests.InstructionTests8085.Tests8085;
                list.Add(new RlcInstructionTest());
                list.Add(new RrcInstructionTest());
                list.Add(new RlInstructionTest());
                list.Add(new RrInstructionTest());
                list.Add(new SlaInstructionTest());
                list.Add(new SraInstructionTest());
                list.Add(new SrlInstructionTest());
                list.Add(new bitInstructionTest());
                list.Add(new resInstructionTest());
                list.Add(new setInstructionTest());
                list.Add(new AddInstructionTest());
                list.Add(new LdrrnnInstructionTest());
                list.Add(new LdnnrrInstructionTest());
                list.Add(new LdMnnInstructionTest());
                list.Add(new IncDcrrrInstructionTest("Inc", 0x23));
                list.Add(new IncDcrrrInstructionTest("Dcr", 0x2b));
                list.Add(new LdrrMnnInstructionTest());
                list.Add(new IncDcrMnnInstructionTest("IncM", 0x34));
                list.Add(new IncDcrMnnInstructionTest("DcrM", 0x35));
                list.Add(new LdrMInstructionTest());
                list.Add(new LdMrInstructionTest());
                list.Add(new LogicOperationTestInstruction("Adda", 0x86, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Suba", 0x96, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Anda", 0xa6, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Ora", 0xb6, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Adca", 0x8e, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Sbca", 0x8e, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Xora", 0x8e, RTestZ80.arithFlags));
                list.Add(new LogicOperationTestInstruction("Cpa", 0x8e, RTestZ80.arithFlags));
                list.Add(new PopMTestInstruction());
                list.Add(new ExMTestInstruction());
                list.Add(new PushMTestInstruction());
                list.Add(new RotateMInstructionTest("RlcM", 0x06));
                list.Add(new RotateMInstructionTest("RlM", 0x16));
                list.Add(new RotateMInstructionTest("SlaM", 0x26));
                list.Add(new RotateMInstructionTest("RrcM", 0x0e));
                list.Add(new RotateMInstructionTest("RrM", 0x1e));
                list.Add(new RotateMInstructionTest("SraM", 0x2e));
                list.Add(new RotateMInstructionTest("SrlM", 0x3e));
                list.Add(new RotateMInstructionTest("Bit0M", 0x46, false));
                list.Add(new RotateMInstructionTest("Bit1M", 0x4e, false));
                list.Add(new RotateMInstructionTest("Bit2M", 0x56, false));
                list.Add(new RotateMInstructionTest("Bit3M", 0x5e, false));
                list.Add(new RotateMInstructionTest("Bit4M", 0x66, false));
                list.Add(new RotateMInstructionTest("Bit5M", 0x6e, false));
                list.Add(new RotateMInstructionTest("Bit6M", 0x76, false));
                list.Add(new RotateMInstructionTest("Bit7M", 0x7e, false));
                list.Add(new RotateMInstructionTest("Res0M", 0x86, false));
                list.Add(new RotateMInstructionTest("Res1M", 0x8e, false));
                list.Add(new RotateMInstructionTest("Res2M", 0x96, false));
                list.Add(new RotateMInstructionTest("Res3M", 0x9e, false));
                list.Add(new RotateMInstructionTest("Res4M", 0xa6, false));
                list.Add(new RotateMInstructionTest("Res5M", 0xae, false));
                list.Add(new RotateMInstructionTest("Res6M", 0xb6, false));
                list.Add(new RotateMInstructionTest("Res7M", 0xbe, false));
                list.Add(new RotateMInstructionTest("Set0M", 0xc6, false));
                list.Add(new RotateMInstructionTest("Set1M", 0xce, false));
                list.Add(new RotateMInstructionTest("Set2M", 0xd6, false));
                list.Add(new RotateMInstructionTest("Set3M", 0xde, false));
                list.Add(new RotateMInstructionTest("Set4M", 0xe6, false));
                list.Add(new RotateMInstructionTest("Set5M", 0xee, false));
                list.Add(new RotateMInstructionTest("Set6M", 0xf6, false));
                list.Add(new RotateMInstructionTest("Set7M", 0xfe, false));
                return list;
            }
        }
    }
}