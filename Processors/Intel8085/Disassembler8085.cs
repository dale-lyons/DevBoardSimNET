using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace Intel8085
{
    public class Disassembler8085 : IDisassembler
    {
        private delegate string ExecuteInstruction(ushort pc, ISystemMemory code, byte opcode, out int count);

        private IProcessor mProcesor;
        public Disassembler8085(IProcessor procesor)
        {
            mProcesor = procesor;
        }

        private static ExecuteInstruction[] mOpcodeFunctions = new ExecuteInstruction[256]
            {
                Nop,                       //nop            0x00
                Lxi,                       //lxi b,data
                LdStax,                    //stax b
                DoubleRegister,            //inx b
                SingleRegDst,                 //inr b
                SingleRegDst,                 //dcr b
                Mvi,                       //mvi b,data
                NoParams,                  //rlc
                Nop,
                DoubleRegister,            //dad b
                LdStax,                      //ldax b
                DoubleRegister,            //dcx b
                SingleRegDst,                 //inr c
                SingleRegDst,                 //dcr c
                Mvi,                       //mvi c,data
                NoParams,                  //rrc

                Nop,                                          //0x10
                Lxi,                       //lxi d,data
                LdStax,            //stax d
                DoubleRegister,            //inx d
                SingleRegDst,                 //inr d
                SingleRegDst,                 //dcr d
                Mvi,                       //mvi d,data
                NoParams,                  //ral
                Nop,
                DoubleRegister,            //dad d
                LdStax,            //ldax d
                DoubleRegister,            //dcx d
                SingleRegDst,                 //inr e
                SingleRegDst,                 //dcr e
                Mvi,                       //mvi e,data
                NoParams,                  //rar

                NoParams,                  //rim            0x20
                Lxi,                       //lxi h,data
                Data16,                    //shld addr
                DoubleRegister,            //inx h
                SingleRegDst,                 //inr h
                SingleRegDst,                 //dcr h
                Mvi,                       //mvi h,data
                NoParams,                  //daa
                Nop,
                DoubleRegister,            //dad h
                Data16,                    //lhld addr
                DoubleRegister,            //dcx h
                SingleRegDst,                 //inr l
                SingleRegDst,                 //dcr l
                Mvi,                       //mvi l,data
                NoParams,                  //cma

                NoParams,                  //sim            0x30
            	Lxi,                       //lxi sp,data
            	Data16,                    //sta addr
            	DoubleRegister,            //inx sp
            	SingleRegDst,                 //inr m
            	SingleRegDst,                 //dcr m
            	Mvi,                       //mvi m,data
            	NoParams,                  //stc
            	Nop,
                DoubleRegister,            //dad sp
            	Data16,                    //lda addr
            	DoubleRegister,            //dcx sp
            	SingleRegDst,                 //inr a
            	SingleRegDst,                 //dcr a
            	Mvi,                       //mvi a,data
               	NoParams,                  //cmc

                Mov,                       //mov b,b          0x40
            	Mov,                       //mov b,c
            	Mov,                       //mov b,d
            	Mov,                       //mov b,e
            	Mov,                       //mov b,h
            	Mov,                       //mov b,l
            	Mov,                       //mov b,m
            	Mov,                       //mov b,a
            	Mov,                       //mov c,b
            	Mov,                       //mov c,c
            	Mov,                       //mov c,d
            	Mov,                       //mov c,e
            	Mov,                       //mov c,h
            	Mov,                       //mov c,l
            	Mov,                       //mov c,m
            	Mov,                       //mov c,a

            	Mov,                       //mov d,b        0x50
            	Mov,                       //mov d,c
            	Mov,                       //mov d,d
            	Mov,                       //mov d,e
            	Mov,                       //mov d,h
            	Mov,                       //mov d,l
            	Mov,                       //mov d,m
            	Mov,                       //mov d,a
            	Mov,                       //mov e,b
            	Mov,                       //mov e,c
            	Mov,                       //mov e,d
            	Mov,                       //mov e,e
            	Mov,                       //mov e,h
            	Mov,                       //mov e,l
            	Mov,                       //mov e,m
            	Mov,                       //mov e,a

            	Mov,                       //mov h,b            0x60
            	Mov,                       //mov h,c
            	Mov,                       //mov h,d
            	Mov,                       //mov h,e
            	Mov,                       //mov h,h
            	Mov,                       //mov h,l
            	Mov,                       //mov h,m
            	Mov,                       //mov h,a
            	Mov,                       //mov l,b
            	Mov,                       //mov l,c
            	Mov,                       //mov l,d
            	Mov,                       //mov l,e
            	Mov,                       //mov l,h
            	Mov,                       //mov l,l
            	Mov,                       //mov l,m
            	Mov,                       //mov l,a

                Mov,                       //mov m,b        0x70
            	Mov,                       //mov m,c
            	Mov,                       //mov m,d
            	Mov,                       //mov m,e
            	Mov,                       //mov m,h
            	Mov,                       //mov m,l
            	NoParams,                  //hlt
            	Mov,                       //mov m,a
            	Mov,                       //mov a,b
            	Mov,                       //mov a,c
            	Mov,                       //mov a,d
            	Mov,                       //mov a,e
            	Mov,                       //mov a,h
            	Mov,                       //mov a,l
            	Mov,                       //mov a,m
            	Mov,                       //mov a,a

            	SingleRegSrc,                 //add b   0x80
            	SingleRegSrc,                 //add c
            	SingleRegSrc,                 //add d
            	SingleRegSrc,                 //add e
            	SingleRegSrc,                 //add h
            	SingleRegSrc,                 //add l
            	SingleRegSrc,                 //add m
            	SingleRegSrc,                 //add a
            	SingleRegSrc,                 //adc b
            	SingleRegSrc,                 //adc c
            	SingleRegSrc,                 //adc d
            	SingleRegSrc,                 //adc e
            	SingleRegSrc,                 //adc h
            	SingleRegSrc,                 //adc l
            	SingleRegSrc,                 //adc m
            	SingleRegSrc,                 //adc a

            	SingleRegSrc,                 //sub b   0x90
            	SingleRegSrc,                 //sub c
            	SingleRegSrc,                 //sub d
            	SingleRegSrc,                 //sub e
            	SingleRegSrc,                 //sub h
            	SingleRegSrc,                 //sub l
            	SingleRegSrc,                 //sub m
            	SingleRegSrc,                 //sub a
            	SingleRegSrc,                 //sbb b
            	SingleRegSrc,                 //sbb c
            	SingleRegSrc,                 //sbb d
            	SingleRegSrc,                 //sbb e
            	SingleRegSrc,                 //sbb h
            	SingleRegSrc,                 //sbb l
            	SingleRegSrc,                 //sbb m
            	SingleRegSrc,                 //sbb a

            	SingleRegSrc,                 //ana b   0xa0
            	SingleRegSrc,                 //ana c
            	SingleRegSrc,                 //ana d
            	SingleRegSrc,                 //ana e
            	SingleRegSrc,                 //ana h
            	SingleRegSrc,                 //ana l
            	SingleRegSrc,                 //ana m
            	SingleRegSrc,                 //ana a
            	SingleRegSrc,                 //xra b
            	SingleRegSrc,                 //xra c
            	SingleRegSrc,                 //xra d
            	SingleRegSrc,                 //xra e
            	SingleRegSrc,                 //xra h
            	SingleRegSrc,                 //xra l
            	SingleRegSrc,                 //xra m
            	SingleRegSrc,                 //xra a

            	SingleRegSrc,                 //ora b   0xb0
            	SingleRegSrc,                 //ora c
            	SingleRegSrc,                 //ora d
            	SingleRegSrc,                 //ora e
            	SingleRegSrc,                 //ora h
            	SingleRegSrc,                 //ora l
            	SingleRegSrc,                 //ora m
            	SingleRegSrc,                 //ora a
            	SingleRegSrc,                 //cmp b
            	SingleRegSrc,                 //cmp c
            	SingleRegSrc,                 //cmp d
            	SingleRegSrc,                 //cmp e
            	SingleRegSrc,                 //cmp h
            	SingleRegSrc,                 //cmp l
            	SingleRegSrc,                 //cmp m
            	SingleRegSrc,                 //cmp a

                CJR,                      //rnz         0xc0
            	PushPop,                  //pop b  
            	CJR,                   //jnz addr
            	CJR,                   //jmp addr
            	CJR,                   //cnz addr
            	PushPop,                  //push b
            	Data8,                    //adi data
            	Rst,                      //rst 0
            	CJR,                  //rz
            	CJR,                  //ret
            	CJR,                   //jz addr
            	Nop,
                CJR,                   //cz addr
            	CJR,                   //call addr
            	Data8,                    //aci data
            	Rst,                      //rst 1

                CJR,                      //rnc         0xd0
            	PushPop,                  //pop d
            	CJR,                   //jnc addr
            	Data8,                    //out data
            	CJR,                   //cnc addr
            	PushPop,                  //push d
            	Data8,                    //sui data
            	Rst,                      //rst 2
            	CJR,                  //rc
            	Nop,
                CJR,                   //jc addr
            	Data8,                    //in data
            	CJR,                   //cc addr
            	Nop,
                Data8,                    //sbi data
            	Rst,                      //rst 3

                CJR,                      //rpo         0xe0
            	PushPop,                  //pop h
            	CJR,                   //jpo addr
            	NoParamXCHGRegs,          //xthl
            	CJR,                   //cpo addr
            	PushPop,                  //push h
            	Data8,                    //ani data
            	Rst,                      //rst 4
            	CJR,                  //rpe
            	NoParamXCHGRegs,          //pchl
            	CJR,                   //jpe addr
            	NoParamXCHGRegs,          //xchg
            	CJR,                   //cpe addr
            	Nop,
                Data8,                    //xri data
            	Rst,                      //rst 5

                CJR,                      //rp          0xf0
            	PushPop,                  //pop psw
            	CJR,                   //jp addr
            	NoParams,                 //di
            	CJR,                   //cp addr
            	PushPop,                  //push psw
            	Data8,                    //ori data
            	Rst,                      //rst 6
            	CJR,                  //rm
            	NoParamXCHGRegs,          //sphl
            	CJR,                   //jm addr
            	NoParams,                 //ei
            	CJR,                   //cm addr
            	Nop,
                Data8,                    //cpi data
            	Rst                      //rst 7
            };

        private static ushort getCodeWord(uint addr, ISystemMemory code)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)code.GetMemory(addr, WordSize.OneByte, false);
            bytes[1] = (byte)code.GetMemory(addr+1, WordSize.OneByte, false);
            return BitConverter.ToUInt16(bytes, 0);

        }
        private static string Nop(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            return "nop";
        }
        //lxi h,0x1234
        private static string Lxi(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 3;
            DoubleRegisterEnums reg = (DoubleRegisterEnums)((opcode >> 4) & 0x03);
            ushort data = getCodeWord((ushort)(pc + 1), code);
            return string.Format("{0,-10} {1},{2}h", "lxi", reg.ToString(), data.ToString("x4"), pc, 3);
        }
        //private static ushort Stax(ushort pc, byte opcode)
        //{
        //    int reg = ((opcode >> 4) & 0x01),
        //    string Reg = (reg == 0x00) ? Scanner.DoubleRegisterTypes.b.ToString() : Scanner.DoubleRegisterTypes.d.ToString();
        //    mLines.Add(new GUI.Code.CodeLine(Code, string.Format("{0,-10} {0}", "stax", Reg), pc, 2));
        //    return 2;
        //}
        private static string DoubleRegister(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            //00RP0011
            int reg = (opcode >> 4) & 0x03;
            string Reg = ((DoubleRegisterEnums)reg).ToString();

            string inst = string.Empty;
            switch (opcode & 0x0f)
            {
                case 0x09:                                  //dad reg
                    inst = "dad"; break;
                case 0x03:                                  //inx reg
                    inst = "inx"; break;
                case 0x0b:                                  //dcx reg
                    inst = "dcx"; break;
            }
            return string.Format("{0,-10} {1}", inst, Reg, pc, 1);
        }

        private static string SingleRegDst(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int reg = ((opcode & 0x38) >> 3);
            string Reg;
            if (reg == 0x06)
                Reg = "m";
            else
                Reg = ((SingleRegisterEnums)reg).ToString();

            string inst = string.Empty;
            switch (opcode & 0xc7)
            {
                case 0x05:                      //dcr reg
                    inst = "dcr"; break;
                case 0x04:                      //inr reg
                    inst = "inr"; break;
            }
            return string.Format("{0,-10} {1}", inst, Reg, pc, 1);
        }

        private static string Mvi(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 2;
            //00DDD110;
            int reg = (opcode >> 3) & 0x07;
            byte data = (byte)code.GetMemory((uint)(pc + 1), WordSize.OneByte, false);
            string Reg = singleRegToString(reg);
            return string.Format("{0,-10} {1},{2}h", "mvi", Reg, data.ToString("x2"), pc, 2);
        }

        private static string Mov(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int dreg = ((opcode >> 3) & 0x07);
            string DReg = singleRegToString(dreg);
            int sreg = (opcode & 0x07);
            string SReg = singleRegToString(sreg);

            return string.Format("{0,-10} {1},{2}", "mov", DReg, SReg, pc, 1);
        }

        private static string NoParams(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            string inst = string.Empty;
            switch (opcode)
            {
                case 0x17:                  //ral
                    inst = "ral"; break;
                case 0x1f:                  //rar
                    inst = "rar"; break;
                case 0x07:                  //rlc
                    inst = "rlc"; break;
                case 0x0f:                  //rrc
                    inst = "rrc"; break;
                case 0x20:                  //rim
                    inst = "rim"; break;
                case 0x30:                  //sim
                    inst = "sim"; break;
                case 0x37:                  //stc
                    inst = "stc"; break;
                case 0x27:                  //daa
                    inst = "daa"; break;
                case 0x2f:                  //cma
                    inst = "cma"; break;
                case 0x3f:                  //cmc
                    inst = "cmc"; break;
                case 0x76:                  //hlt
                    inst = "hlt"; break;
                case 0xf3:                  //di
                    inst = "di"; break;
                case 0xfb:                  //ei
                    inst = "ei"; break;
            }
            return inst;
        }

        // sta 0x1234
        private static string Data16(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 3;
            ushort addr = getCodeWord((ushort)(pc + 1), code);
            string inst = string.Empty;
            switch (opcode)
            {
                case 0x32:                //sta addrr
                    inst = "sta"; break;
                case 0x3a:                //lda addrr
                    inst = "lda"; break;
                case 0x2a:                //lhld addr
                    inst = "lhld"; break;
                case 0x22:                //shld addr
                    inst = "shld"; break;
            }//switch
            return string.Format("{0,-10} {1}h", inst, addr.ToString("x4"), pc, 3);
        }

        private static string LdStax(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int reg = (opcode >> 4) & 0x01;
            int opType = (opcode >> 6) & 0x03;
            string Reg = doubleRegToString(reg, opType);
            string inst = string.Empty;
            if ((opcode & 0x0f) == 0x0a)
                inst = "ldax";
            else
                inst = "stax";

            return string.Format("{0,-10} {1}", inst, Reg, pc, 2);
        }

        private static string SingleRegSrc(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int reg = (opcode & 0x07);
            string Reg = singleRegToString(reg);
            string inst = string.Empty;
            switch (opcode & 0xf8)
            {
                case 0xb8:                      //cmp reg
                    inst = "cmp"; break;
                case 0xb0:                      //ora reg
                    inst = "ora"; break;
                case 0xa8:                      //xra reg
                    inst = "xra"; break;
                case 0xa0:                      //ana reg
                    inst = "ana"; break;
                case 0x80:                      //add reg
                    inst = "add"; break;
                case 0x90:                      //sub reg
                    inst = "sub"; break;
                case 0x88:                      //adc reg
                    inst = "adc"; break;
                case 0x98:                      //sbb reg
                    inst = "sbb"; break;
            }
            return string.Format("{0,-10} {1}", inst, Reg, pc, 1);
        }

        private static string CJR(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            bool unconditional = ((opcode & 0x01) == 0x01);
            byte instType = (byte)((opcode >> 1) & 0x03);
            string inst = string.Empty;

            if (instType == 0x00)
            {//return
                count = 1;
                if (unconditional)
                    return "ret";
                else
                    return ("r" + cpuFlagToString(opcode));
            }
            else if (instType == 0x01)
            {
                if (unconditional)
                    inst = "jmp";
                else
                    inst = "j" + cpuFlagToString(opcode);
            }
            else if (instType == 0x02)
            {
                if (unconditional)
                    inst = "call";
                else
                    inst = "c" + cpuFlagToString(opcode);
            }
            else
                System.Diagnostics.Debug.Assert(false);

            count = 3;
            ushort addr = getCodeWord((ushort)(pc + 1), code);
            return string.Format("{0,-10} {1}h", inst, addr.ToString("x4"), pc, 3);
        }

        //public enum CPUFlags : byte
        //{
        //    Zero = 0x0c,
        //    Carry = 0x0d,
        //    Parity = 0x0e,
        //    Sign = 0x0f
        //}
        private static string cpuFlagToString(int code)
        {
            code = (((code & 0x3c) >> 3) & 0x07);
            switch (code)
            {
                case 0x00:
                case 0x01:
                    return ((code & 0x01) == 0x01) ? "z" : "nz";
                case 0x02:
                case 0x03:
                    return ((code & 0x01) == 0x01) ? "c" : "nc";
                case 0x04:
                case 0x05:
                    return ((code & 0x01) == 0x01) ? "pe" : "po";
                case 0x06:
                case 0x07:
                    return ((code & 0x01) == 0x01) ? "m" : "p";
            }
            return string.Empty;
        }

        private static string PushPop(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int reg = (opcode >> 4) & 0x03;
            int opType = (opcode >> 6) & 0x03;
            string Reg = doubleRegToString(reg, opType);

            string inst = string.Empty;
            if ((opcode & 0x0f) == 0x01)
                inst = "pop";
            else
                inst = "push";

            return string.Format("{0,-10} {1}", inst, Reg, pc, 1);
        }

        private static string Data8(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 2;
            byte data = (byte)code.GetMemory((uint)(pc + 1), WordSize.OneByte, true);
            string inst = string.Empty;
            switch (opcode)
            {
                case 0xdb:                          //in data
                    inst = "in"; break;
                case 0xd3:                         //out data
                    inst = "out"; break;
                case 0xf6:                          //ori data
                    inst = "ori"; break;
                case 0xee:                           //xri data
                    inst = "xri"; break;
                case 0xfe:                         //cpi data
                    inst = "cpi"; break;
                case 0xc6:                                                  //adi data
                    inst = "adi"; break;
                case 0xd6:                                                  //sui data
                    inst = "sui"; break;
                case 0xce:                                                  //aci data
                    inst = "aci"; break;
                case 0xe6:                                                  //ani data
                    inst = "ani"; break;
                case 0xde:                                                  //sbi data
                    inst = "sbi"; break;
            }
            return string.Format("{0,-10} {1}h", inst, data.ToString("x2"), pc, 2);
        }

        private static string Rst(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            int level = ((opcode >> 3) & 0x07);
            return string.Format("{0,-10} {1}", "rst", level.ToString(), pc, 1);
        }

        private static string NoParamXCHGRegs(ushort pc, ISystemMemory code, byte opcode, out int count)
        {
            count = 1;
            string inst = string.Empty;
            switch (opcode)
            {
                case 0xe9:                  //pchl
                    inst = "pchl"; break;
                case 0xf9:                  //sphl
                    inst = "sphl"; break;
                case 0xeb:                  //xchg
                    inst = "xchg"; break;
                case 0xe3:                  //xthl
                    inst = "xthl"; break;
            }
            return inst;
        }

        private static string singleRegToString(int reg)
        {
            if (reg == 0x06)
                return "m";
            else
                return ((SingleRegisterEnums)reg).ToString();
        }
        private static string doubleRegToString(int reg, int opType)
        {
            if (reg <= 2)
                return ((DoubleRegisterEnums)reg).ToString();
            else
            {
                if (opType == 0)
                    return DoubleRegisterEnums.sp.ToString();
                else
                    return DoubleRegisterEnums.psw.ToString();
            }
        }

        public int Align(uint pos, ISystemMemory code)
        {
            //uint alignedPC = pos;
            //if (TestAlignment(alignedPC, code, pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, pc))
            //    return alignedPC;

            //alignedPC--;
            //if (TestAlignment(alignedPC, code, pc))
            //    return alignedPC;

            return 0;
        }

        public static bool TestAlignment(uint startAddr, ISystemMemory code, uint pc)
        {
            if (startAddr < pc)
            {
                uint addr = startAddr;
                while (addr < pc)
                {
                    byte opcode = (byte)code.GetMemory(addr, WordSize.OneByte, false);
                    int count = 0;
                    mOpcodeFunctions[opcode]((ushort)pc, code, opcode, out count);
                    addr += (ushort)count;
                }
                return (addr == pc);
            }
            else
            {
                uint addr = pc;
                while (addr < startAddr)
                {
                    byte opcode = (byte)code.GetMemory(addr, WordSize.OneByte, false);
                    int count = 0;
                    mOpcodeFunctions[opcode]((ushort)pc, code, opcode, out count);
                    addr += (ushort)count;
                }
                return (addr == startAddr);
            }
        }

        public CodeLine ProcessInstruction(uint addr, ISystemMemory code, out uint newAddr)
        {
            byte opcode = (byte)code.GetMemory(addr, WordSize.OneByte, true);
            int count;
            string opcodeStr = mOpcodeFunctions[opcode]((ushort)addr, code, opcode, out count);
            newAddr = (uint)(addr + count);
            var line = new CodeLine(opcodeStr, addr, count);
            line.CodeLineType = CodeLine.CodeLineTypes.Code;
            return line;
        }

        public IList<CodeLine> ProcessCode(uint startAddr, uint endAddr, ISystemMemory code)
        {
            var lines = new List<CodeLine>();
            while (startAddr < endAddr)
                lines.Add(ProcessInstruction(startAddr, code, out startAddr));

            return lines;
        }
    }
}