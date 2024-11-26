using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ARM7.VFP;

using Processors;

namespace ARM7
{
    public partial class ARM7
    {
        //register source/destination masks
        private const uint rn_mask = 0x000f0000;
        private const uint rd_mask = 0x0000f000;
        private const uint rs_mask = 0x00000f00;
        private const uint rm_mask = 0x0000000f;

        private const uint s_mask = 0x00100000;

        private const uint op2_mask = 0x00000fff;
        private const uint up_mask = 0x00800000;        //up,down mask(sbhw opcodes)
        private const uint imm_hw_mask = 0x00400000;        //half word versions

        private const uint pre_mask = 0x01000000;
        private const uint load_mask = 0x00100000;
        private const uint write_back_mask = 0x00200000;

        private const uint byte_mask = 0x00400000;
        private const uint mode_mask = 0x0000001f;
        private const uint imm_mask = 0x02000000;            /* orginal word versions */

        private const uint link_mask = 0x01000000;
        private const uint branch_field = 0x00ffffff;
        private const uint branch_sign = 0x00800000;

        private const uint undef_mask = 0x0e000010;
        private const uint undef_code = 0x06000010;
        private const uint user_mask = 0x00400000;

        public bool TrapUnalignedMemoryAccess { get; set; }

        private static bool isSWIInstruction(uint opcode)
        {
            return (((opcode >> 24) & 0x0f) == 0x0f);
        }

        public uint ExecuteARMInstruction(uint opcode)
        {
            //Check condition code and execute it if the condition code allows
            if (!check_cc(opcode))
                return 1;

            uint cycleCount = 0;
            switch ((opcode >> 25) & 0x00000007)
            {
                case 0x00: cycleCount = data_op(opcode); break;		//includes load/store hw & sb
                case 0x01: cycleCount = data_op(opcode); break;		//data processing & MSR # */
                case 0x02: cycleCount = transfer(opcode); break;
                case 0x03: cycleCount = transfer(opcode); break;
                case 0X04: cycleCount = multiple(opcode); break;
                case 0x05: cycleCount = branch(opcode); break;
                case 0X06: cycleCount = FPP.Execute(opcode); break;
                case 0X07:
                    if (((opcode & 0X0F000000) == 0X0E000000)
                    || ((opcode & 0X0F000000) == 0X0C000000)
                    || ((opcode & 0X0F000000) == 0X0D000000))
                    {
                        cycleCount = FPP.Execute(opcode); break;
                    }
                    else
                    {
                        if (isSWIInstruction(opcode))
                            cycleCount = swi_op(opcode);
                        else
                            OnInvalidInstruction.Invoke(opcode, GPR.PC);
                        break;
                    }
                default: break;
            }//switch
            return cycleCount;
        }

        /// <summary>
        /// Check the condition code(cc) of the current instruction and determine if
        /// the instruction should be executed or not.
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        private bool check_cc(uint opcode)
        {
            var cpsr = CPSR;
            switch (opcode >> 28 & 0x0f)
            {
                case 0x00: return cpsr.zf;
                case 0x01: return !cpsr.zf;
                case 0x02: return cpsr.cf;
                case 0x03: return !cpsr.cf;
                case 0x04: return cpsr.nf;
                case 0x05: return !cpsr.nf;
                case 0x06: return cpsr.vf;
                case 0x07: return !cpsr.vf;

                //case 0X8: go = and(cf(cpsr), not(zf(cpsr)));                     break;
                case 0x08: return cpsr.cf && !cpsr.zf;

                //case 0X9: go = or(not(cf(cpsr)), zf(cpsr));                      break;
                case 0x09: return !cpsr.cf || cpsr.zf;

                //case 0XA: go = not(xor(nf(cpsr), vf(cpsr)));                     break;
                case 0x0a: return !(cpsr.nf ^ cpsr.vf);

                //case 0XB: go = xor(nf(cpsr), vf(cpsr));                          break;
                case 0x0b: return cpsr.nf ^ cpsr.vf;

                //case 0XC: go = and(not(zf(cpsr)), not(xor(nf(cpsr), vf(cpsr)))); break;
                case 0x0c: return !cpsr.zf && (!(cpsr.nf ^ cpsr.vf));

                //case 0XD: go = or(zf(cpsr), xor(nf(cpsr), vf(cpsr)));            break;
                case 0xd: return cpsr.zf || (cpsr.nf ^ cpsr.vf);

                case 0x0e: return true;
                case 0x0f: return false;

                default: return false;
            }//switch
        }//check_cc

        /// <summary>
        /// Compute the number of clock cycles required for a multiply operation
        /// based on one of the operands.
        /// </summary>
        /// <param name="RsData"></param>
        /// <returns></returns>
        private static int calculateMultM(uint RsData)
        {
            //replaced the code below with this. Simpler
            //and safer - dale

            if (RsData < 0x00000100)        //256
                return 1;
            else if (RsData < 0x00010000)   //65535
                return 2;
            else if (RsData < 0x01000000)   //16777216
                return 3;
            else
                return 4;

            //int data = (int)RsData;
            //if (data < 0)
            //    data = ~data;

            //if (data < 256)
            //    return 1;
            //else if (data < 65536)
            //    return 2;
            //else if (data < 16777216)
            //    return 3;
            //else
            //    return 4;
        }//calculateMultM

        private const uint mul_long_bit = 0x00800000;
        private const uint mul_acc_bit = 0x00200000;

        /// <summary>
        /// Perform the ARM instruction MUL
        /// </summary>
        /// <param name="op_code"></param>
        /// <returns></returns>
        private uint multiply(uint opcode)
        {
            uint Rs = ((opcode & rs_mask) >> 8);
            uint Rm = (opcode & rm_mask);
            uint Rd = ((opcode & rd_mask) >> 12);
            uint Rn = ((opcode & rn_mask) >> 16);

            uint RsData = get_reg(Rs);
            uint RmData = get_reg(Rm);
            int M = calculateMultM(RsData);

            if ((opcode & mul_long_bit) == 0)
            {//Normal:mla,mul
                uint cycles = 1;
                uint acc = (RmData * RsData);
                if ((opcode & mul_acc_bit) != 0)
                {
                    acc += get_reg(Rd);
                    ++cycles;
                }
                GPR[Rn] = acc;

                if ((opcode & s_mask) != 0)
                    CPSR.set_NZ(acc);//flags

                return (uint)(cycles + M);
            }//if
            else
            {//Long:xMLAL,xMULL
                uint cycles = 2;
                bool sign = false;

                if ((opcode & 0x00400000) != 0)
                {//Signed
                    if (Utils.msb(RmData))
                    {
                        RmData = ~RmData + 1;
                        sign = true;
                    }
                    if (Utils.msb(RsData))
                    {
                        RsData = ~RsData + 1;
                        sign = !sign;
                    }
                }//if

                //Everything now `positive
                uint tl = (RmData & 0x0000ffff) * (RsData & 0x0000ffff);
                uint th = ((RmData >> 16) & 0X0000ffff) * ((RsData >> 16) & 0X0000ffff);
                uint tm = ((RmData >> 16) & 0X0000ffff) * (RsData & 0X0000ffff);

                RmData = ((RsData >> 16) & 0X0000ffff) * (RmData & 0X0000ffff);  /* Rm no longer needed */
                tm = tm + RmData;
                if (tm < RmData) th = th + 0X00010000;                       /* Propagate carry */
                tl = tl + (tm << 16);
                if (tl < (tm << 16)) th = th + 1;
                th = th + ((tm >> 16) & 0X0000ffff);

                if (sign)
                {
                    th = ~th;
                    tl = ~tl + 1;
                    if (tl == 0) th = th + 1;
                }

                if ((opcode & mul_acc_bit) != 0)
                {
                    ++cycles;
                    tm = tl + get_reg(Rd);
                    if (tm < tl)
                        th = th + 1;//Propagate carry
                    tl = tm;
                    th += get_reg(Rn);
                }

                GPR[Rd] = tl;
                GPR[Rn] = th;

                if ((opcode & s_mask) != 0)
                {
                    CPSR.set_NZ(th | (((tl >> 16) | tl) & 0x0000ffff));
                }
                return (uint)(cycles + M);
            }//else

        }//multiply

        private uint get_reg(uint reg)
        {
            if (reg == GeneralPurposeRegisters.PCRegisterIndex) return (GPR.PC + (CPSR.tf ? (uint)2 : (uint)4));
            return GPR[reg];
        }

        /// <summary>
        /// Determine if this is a signed byte half word instruction
        /// </summary>
        /// <param name="op_code"></param>
        /// <returns></returns>
        private static bool is_it_sbhw(uint op_code)
        {
            if (((op_code & 0x0e000090) == 0x00000090) && ((op_code & 0x00000060) != 0x00000000)	//No multiplies
                    && ((op_code & 0x00100040) != 0x00000040))										//No signed stores
                return ((op_code & 0x00400000) != 0) || ((op_code & 0x00000f00) == 0);
            return false;
        }//is_it_sbhw

        /// <summary>
        /// Logical Shift Left 1 position
        /// </summary>
        /// <param name="val"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint lsl(uint val, ref bool cf)
        {
            cf = Utils.msb(val);
            return val << 1;
        }//lsl

        /// <summary>
        /// Logical Shift Left n positions
        /// </summary>
        /// <param name="val"></param>
        /// <param name="distance"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint lsl(uint val, uint distance, ref bool cf)
        {
            for (uint ii = 0; ii < distance; ii++)
            {
                val = lsl(val, ref cf);
            }
            return val;
        }//lsl

        /// <summary>
        /// Logical Shift Right 1 position
        /// </summary>
        /// <param name="val"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint lsr(uint val, ref bool cf)
        {
            cf = Utils.lsb(val);
            return val >> 1;
        }//lsr

        /// <summary>
        /// Logical Shift Right n positions
        /// </summary>
        /// <param name="val"></param>
        /// <param name="distance"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint lsr(uint val, uint distance, ref bool cf)
        {
            for (uint ii = 0; ii < distance; ii++)
            {
                val = lsr(val, ref cf);
            }
            return val;
        }//lsr

        /// <summary>
        /// Arithmetic Shift Right 1 position
        /// </summary>
        /// <param name="val"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint asr(uint val, ref bool cf)
        {
            uint sign = val & Utils.bit_31;
            return (lsr(val, ref cf) | sign);
        }//asr

        /// <summary>
        /// Arithmetic Shift Right n positions
        /// </summary>
        /// <param name="val"></param>
        /// <param name="distance"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint asr(uint val, uint distance, ref bool cf)
        {
            for (uint ii = 0; ii < distance; ii++)
            {
                val = asr(val, ref cf);
            }
            return val;
        }//asr

        /// <summary>
        /// Rotate Right 1 position
        /// </summary>
        /// <param name="val"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint ror(uint val, ref bool cf)
        {
            cf = Utils.lsb(val);
            uint sign = Utils.lsb(val) ? Utils.bit_31 : 0;
            return (val >> 1) | sign;
        }//ror

        /// <summary>
        /// Rotate Right n positions
        /// </summary>
        /// <param name="val"></param>
        /// <param name="distance"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private static uint ror(uint val, uint distance, ref bool cf)
        {
            for (uint ii = 0; ii < distance; ii++)
            {
                val = ror(val, ref cf);
            }
            return val;
        }//ror

        /// <summary>
        /// shift type: 00 = LSL, 01 = LSR, 10 = ASR, 11 = ROR
        /// </summary>
        /// <param name="op2"></param>
        /// <param name="cf"></param>
        /// <returns></returns>
        private uint b_reg(uint op2, ref bool cf)
        {
            uint reg = get_reg((byte)(op2 & 0x00f));                         /* Register */
            uint shift_type = (op2 & 0x060) >> 5;                             /* Type of shift */
            uint distance = 0;

            if ((op2 & 0x010) == 0)
            {                                                        /* Immediate value */
                distance = (op2 & 0Xf80) >> 7;
                if (distance == 0)                                         /* Special cases */
                {
                    if (shift_type == 3)
                    {
                        shift_type = 4;                                                  /* RRX */
                        distance = 1;                                     /* Something non-zero */
                    }//if
                    else if (shift_type != 0)
                        distance = 32;                  /* LSL excluded */
                }//if
            }//if
            else
                distance = (get_reg((byte)((op2 & 0XF00) >> 8)) & 0xff);
            /* Register value */

            uint result = 0;
            cf = (CPSR.cf);                              /* Previous carry */
            switch (shift_type)
            {
                case 0x0: result = lsl(reg, distance, ref cf); break;                    /* LSL */
                case 0x1: result = lsr(reg, distance, ref cf); break;                    /* LSR */
                case 0x2: result = asr(reg, distance, ref cf); break;                    /* ASR */
                case 0x3: result = ror(reg, distance, ref cf); break;                    /* ROR */
                case 0x4:                                                         /* RRX #1 */
                    result = reg >> 1;
                    if (!CPSR.cf)
                        result &= ~Utils.bit_31;
                    else
                        result |= Utils.bit_31;

                    cf = Utils.lsb(reg);
                    break;
            }

            return result;
        }//b_reg

        /// <summary>
        /// 
        /// </summary>
        /// <param name="op2"></param>
        /// <param name="add"></param>
        /// <param name="imm"></param>
        /// <param name="sbhw"></param>
        /// <returns></returns>
        private int transfer_offset(uint op2, bool add, bool imm, bool sbhw)
        {//add and imm are zero/non-zero Booleans
            int offset;
            bool cf = false;                                                    /* dummy parameter */

            if (!sbhw)					//Addressing mode 2 */
            {
                if (imm)
                    offset = (int)b_reg(op2, ref cf);              /* bit(25) = 1 -> reg */
                else
                    offset = (int)(op2 & 0x0fff);
            }//if
            else                                                     /* Addressing mode 3 */
            {
                if (imm)
                    offset = (int)(((op2 & 0xf00) >> 4) | (op2 & 0x00f));
                else
                    offset = (int)b_reg(op2, ref cf);
            }//else

            if (!add) offset = -offset;

            return offset;
        }

        //----------------------------------------------------------------------------*/
        private uint transfer_sbhw(uint op_code)
        {
            WordSize ws;
            switch (op_code & 0x00000060)
            {
                case 0x20: ws = WordSize.FourByte; break;					//H
                case 0x40: ws = WordSize.OneByte; break;						//SB
                case 0x60: ws = WordSize.TwoByte; break;					//SH
                default: return 0;
            }

            uint Rd = ((op_code & rd_mask) >> 12);
            uint Rn = ((op_code & rn_mask) >> 16);

            uint address = get_reg(Rn);
            int offset = transfer_offset((op_code & op2_mask), ((op_code & up_mask) != 0), ((op_code & imm_hw_mask) != 0), true);

            if ((op_code & pre_mask) != 0)
                address = (uint)((int)address + offset);//pre-index

            if (TrapUnalignedMemoryAccess)
            {
                if ((ws == WordSize.FourByte && ((address & 0x03) != 0))
                    || (ws == WordSize.TwoByte && ((address & 0x01) != 0)))
                {
                    unalignedAccess(address);
                }
            }

            uint cycles;
            if ((op_code & load_mask) == 0)
            {//store
                SystemMemory.SetMemory(address, ws, get_reg(Rd));
                cycles = 2;
            }
            else
            {//load
                cycles = 3;
                uint data = SystemMemory.GetMemory(address, ws);
                if (Rd == GeneralPurposeRegisters.PCRegisterIndex)
                {//We are loading into the PC, must check if thumb mode or not
                    cycles = 5;
                    if ((data & 0x01) != 0)
                    {//Entering Thumb mode. Make sure address is HWord aligned
                        CPSR.tf = true;
                        data = (data & 0xfffffffe);
                    }
                    else
                    {//Entering ARM mode. Make sure address is Word aligned
                        CPSR.tf = false;
                        data = (data & 0xfffffffc);
                    }//else
                }//if

                //update target register
                GPR[Rd] = data;
            }//else(Load)

            //post index
            if ((op_code & pre_mask) == 0)//post index with writeback
                GPR[Rn] = (uint)((int)address + offset);
            else if ((op_code & write_back_mask) != 0)
                GPR[Rn] = address;

            return cycles;
        }

        private void unalignedAccess(uint addr)
        {
            string str = "Access to unaligned memory location, bad address = " + addr.ToString("x8");
            //WriteLineConsole(str);
            System.Diagnostics.Debug.WriteLine(str);
            //StopSimulation();
            return;
        }

        //----------------------------------------------------------------------------
        private uint swap(uint op_code)
        {
            uint address = get_reg((byte)((op_code & rn_mask) >> 16));
            WordSize ws = ((op_code & byte_mask) != 0) ? WordSize.OneByte : WordSize.FourByte;

            uint data = SystemMemory.GetMemory(address, ws);
            SystemMemory.SetMemory(address, ws, get_reg(op_code & rm_mask));
            GPR[((op_code & rd_mask) >> 12)] = data;
            return 4;
        }

        //Move PSR to general purpose register
        private uint mrs(uint op_code)
        {
            if ((op_code & 0X00400000) == 0)
            {
                //CPSR register, move into destination reg
                GPR[((op_code & rd_mask) >> 12)] = this.CPSR.Flags;
            }//if
            else
            {
                //get the SPSR for the current cpu mode and set into destination reg
                GPR[((op_code & rd_mask) >> 12)] = this.CPSR.SPSR;
            }//else
            return 1;
        }//mrs

        /*----------------------------------------------------------------------------*/
        private uint msr(uint opcode)
        {
            uint mask;
            switch (opcode & 0x00090000)
            {
                //bottom 8 bits are the control bits(I,F,T(ignored), 5 mode bits).
                case 0x00010000: mask = 0x000000ff; break;

                //top 4 bits are the status bits(N,Z,C,V)
                case 0x00080000: mask = 0xf0000000; break;

                //combination of top 4 and bottom 8 bits
                case 0x00090000: mask = 0xf00000ff; break;

                //all other fields are off limits
                default: mask = 0; break;
            }//switch

            if (CPSR.Mode == CPSR.CPUModeEnum.User)
            {
                //if we are in User mode, non privileged mode, cannot touch the non status bits
                //can only change top 4 bits only
                mask &= 0xf0000000;
            }

            uint source;
            if ((opcode & imm_mask) == 0)           //Test applies for both cases
            {
                //get new value from register
                source = (get_reg(opcode & rm_mask) & mask);
            }
            else
            {
                //otherwise, get new value from immmediate operand in opcode
                uint x = opcode & 0x0ff;                                   //Immediate value
                uint y = (opcode & 0xf00) >> 7;                            //Number of rotates

                bool dummy = false;
                source = lsr(x, y, ref dummy);
                source |= (lsl(x, 32 - y, ref dummy) & mask);

            }//else

            if ((opcode & 0x00400000) == 0)
            {
                //update cpsr
                CPSR.Flags = (CPSR.Flags & ~mask) | source;
            }//if
            else
            {
                CPSR.SPSR = (CPSR.SPSR & ~mask) | source;
            }
            return 1;
        }//msr

        //----------------------------------------------------------------------------
        //Count Leading Zeros
        private uint clz(uint op_code)
        {
            uint j = get_reg((byte)(op_code & rm_mask));
            uint count = 32;
            while (j != 0)
            {
                j = j >> 1;
                count--;
            }//while
            GPR[((op_code & rd_mask) >> 12)] = count;
            return 1;
        }//clz

        //----------------------------------------------------------------------------
        private uint b_immediate(uint op2, ref bool cf)
        {
            uint x = op2 & 0X0FF;                                          /* Immediate value */
            uint y = (op2 & 0XF00) >> 7;                                 /* Number of rotates */
            if (y == 0)
                cf = (CPSR.cf);                 /* Previous carry */
            else
                lsr(x, y, ref cf);

            bool dummy = false;
            return ror(x, y, ref dummy);                               /* Circular rotation */
        }

        //----------------------------------------------------------------------------
        private uint normal_data_op(uint op_code, uint operation)
        {
            //extract the S bit value from the opcode
            bool SBit = ((op_code & s_mask) != 0);

            //extract destination register from opcode
            uint Rd = ((op_code & rd_mask) >> 12);

            //extract operand register and get first operand
            uint Rn = ((op_code & rn_mask) >> 16);
            uint a = get_reg(Rn);

            bool shift_carry = false;

            uint b;
            if ((op_code & imm_mask) == 0)
                b = b_reg(op_code & op2_mask, ref shift_carry);
            else
                b = b_immediate(op_code & op2_mask, ref shift_carry);

            uint rd = 0;
            switch (operation)                                               /* R15s @@?! */
            {
                case 0x0: rd = a & b; break;					//AND
                case 0x1: rd = a ^ b; break;					//EOR
                case 0x2: rd = a - b; break;					//SUB
                case 0x3: rd = b - a; break;					//RSB
                case 0x4: rd = a + b; break;					//ADD
                case 0x5:
                    rd = a + b;							//ADC
                    if (CPSR.cf) ++rd;
                    break;
                case 0x6:
                    rd = a - b - 1;						//SBC
                    if (CPSR.cf) ++rd;
                    break;

                case 0x7:
                    rd = b - a - 1;						//RSC
                    if (CPSR.cf) ++rd;
                    break;

                case 0x8: rd = a & b; break;					//TST
                case 0x9: rd = a ^ b; break;					//TEQ
                case 0xa: rd = a - b; break;					//CMP
                case 0xb: rd = a + b; break;					//CMN
                case 0xc: rd = a | b; break;					//ORR
                case 0xd: rd = b; break;					//MOV
                case 0xe: rd = a & ~b; break;					//BIC
                case 0xf: rd = ~b; break;					//MVN
            }//switch

            if ((operation & 0xc) != 0x8)							//Return result unless a compare
            {
                //write result into destination register
                GPR[Rd] = rd;

                //if S bit is set and if the destination register is r15(pc) then this instruction has
                //special meaning. We are returning from a non-user mode to user-mode.
                //ie  movs pc,r14
                if (SBit && (Rd == GeneralPurposeRegisters.PCRegisterIndex))
                {
                    CPSR.RestoreCPUMode();
                    return 1;
                }//if
            }//if

            //if S bit is not set, we do not need to set any cpu flags, so we are done here
            if (!SBit) return 1;

            switch (operation)
            {                                                           //LOGICALs
                case 0x0:                                               //AND
                case 0x1:                                               //EOR
                case 0x8:                                               //TST
                case 0x9:                                               //TEQ
                case 0xc:                                               //ORR
                case 0xd:                                               //MOV
                case 0xe:                                               //BIC
                case 0xf:                                               //MVN
                    CPSR.set_NZ(rd);
                    CPSR.cf = shift_carry;
                    break;

                case 0x2:                                               //SUB
                case 0xa:                                               //CMP
                    CPSR.set_flags_sub(a, b, rd, true);
                    break;

                case 0x6:                                               //SBC
                    CPSR.set_flags_sub(a, b, rd, CPSR.cf);
                    break;

                case 0x3:                                               //RSB
                    CPSR.set_flags_sub(b, a, rd, true);
                    break;

                case 0x7:                                               //RSC
                    CPSR.set_flags_sub(b, a, rd, CPSR.cf);
                    break;

                case 0x4:                                               //ADD
                case 0xb:                                               //CMN
                    CPSR.set_flags_add(a, b, rd, false);
                    break;

                case 0x5:                                               //ADC
                    CPSR.set_flags_add(a, b, rd, CPSR.cf);
                    break;
                default: break;
            }//switch
            return 1;
        }//normal_data_op

        /*----------------------------------------------------------------------------*/
        private uint bx(uint Rm, bool link) /* Link is performed if "link" is NON-ZERO */
        {
            uint dest = get_reg(Rm);
            if ((dest & 0x1) != 0)
            {// entry into Thumb mode of execution
                CPSR.tf = true;
                dest = dest & 0xfffffffe;
            }
            else
            {
                CPSR.tf = false;
                dest = dest & 0xfffffffc;
            }

            //uint offset = Utils.valid_address(dest);
            if (link)
            {
                GPR.LR = GPR.PC;
            }
            GPR.PC = dest;
            return 3;
        }

        //----------------------------------------------------------------------------
        private uint data_op(uint op_code)
        {
            //check if Arithmetic instruction extension
            //first test is for:mul,muls,mla,mlas
            //second test:umull,umulls,umlal,umlals,smull,smulls,smlal,smlals
            if (((op_code & 0x0fc000f0) == 0x00000090) || ((op_code & 0x0f8000f0) == 0x00800090))
            {
                return multiply(op_code);
            }
            else if (is_it_sbhw(op_code))
            {
                return transfer_sbhw(op_code);
            }
            else if ((op_code & 0x0fb00ff0) == 0x01000090)
            {
                return swap(op_code);
            }
            else
            {
                //TST, TEQ, CMP, CMN - all lie in following range, but have S set
                if ((op_code & 0x01900000) == 0x01000000)
                {//PSR transfers OR BX
                    if ((op_code & 0x0fbf0fff) == 0x010f0000)
                    {
                        return mrs(op_code);
                    }

                    //else if (((op_code & 0x0db6f000) == 0x0120f000) && ((op_code & 0x02000010) != 0x00000010))
                    //fixed a bug in above code. Test was only allowing field mask==0x9, which is really all you need,
                    //but sometines code can be compiled in CPSR_cxsf, which is field mask==0xf, which is legal.
                    else if (((op_code & 0x0db0f000) == 0x0120f000) && ((op_code & 0x02000010) != 0x00000010))
                    {
                        return msr(op_code);
                    }
                    else if ((op_code & 0x0fffffd0) == 0x012fff10)
                    {
                        return bx((op_code & rm_mask), ((op_code & 0x00000020) != 0));
                    }
                    else if ((op_code & 0x0fff0ff0) == 0x016f0f10)
                    {
                        return clz(op_code);
                    }
                }//if
                else
                {//All data processing operations
                    uint operation = (op_code & 0x01e00000) >> 21;
                    return normal_data_op(op_code, operation);
                }//else
                return 0;
            }//else
        }

        private uint transfer(uint op_code)
        {
            if ((op_code & undef_mask) == undef_code) return 0;

            WordSize ws =
                ((op_code & byte_mask) == 0) ? WordSize.FourByte : WordSize.OneByte;
            uint Rd = (op_code & rd_mask) >> 12;
            uint Rn = (op_code & rn_mask) >> 16;
            uint address = get_reg(Rn);
            int offset = transfer_offset((op_code & op2_mask), ((op_code & up_mask) != 0), ((op_code & imm_mask) != 0), false);	//bit(25) = 1 -> reg

            if ((op_code & pre_mask) != 0)
                //Pre-index
                address = (uint)((int)address + offset);

            if (TrapUnalignedMemoryAccess)
            {
                if ((ws == WordSize.FourByte && ((address & 0x03) != 0))
                    || (ws == WordSize.TwoByte && ((address & 0x01) != 0)))
                {
                    unalignedAccess(address);
                }
            }

            uint cycles;
            if ((op_code & load_mask) == 0)
            {
                SystemMemory.SetMemory(address, ws, get_reg(Rd));
                cycles = 2;
            }
            else
            {
                GPR[Rd] = SystemMemory.GetMemory(address, ws);
                cycles = (Rd == GeneralPurposeRegisters.PCRegisterIndex) ? (uint)5 : (uint)3;
            }

            if ((op_code & pre_mask) == 0)//Post-index
                //Post index write-back
                GPR[Rn] = (uint)((int)address + offset);
            else if ((op_code & write_back_mask) != 0)
                //Pre index write-back
                GPR[Rn] = address;

            return cycles;
        }

        //----------------------------------------------------------------------------
        private uint branch(uint op_code)
        {
            if (((op_code & link_mask) != 0) || ((op_code & 0xf0000000) == 0xf0000000))
                GPR.LR = GPR.PC;

            uint offset = (op_code & branch_field) << 2;
            if ((op_code & branch_sign) != 0)
                offset |= 0xfc000000;

            GPR.PC += (offset + 4);
            return 3;
        }

        private static uint bit_count(uint source, out int first)
        {
            uint count = 0;
            int reg = 0;
            first = -1;

            while (source != 0)
            {
                if (Utils.lsb(source))
                {
                    ++count;
                    if (first < 0) first = reg;
                }
                source = source >> 1;
                ++reg;
            }
            return count;
        }

        /// <summary>
        /// STM - perform a multi-register store to memory
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="Rn"></param>
        /// <param name="reg_list"></param>
        /// <param name="write_back"></param>
        /// <returns></returns>
        private uint stm(uint mode, uint Rn, uint reg_list, bool write_back)
        {
            uint address = Utils.valid_address(get_reg(Rn));//Bottom 2 bits ignored in address

            int first_reg;
            uint count = bit_count(reg_list, out first_reg);

            uint new_base;
            switch (mode)
            {
                case 0: new_base = address - 4 * count; address = new_base + 4; break;
                case 1: new_base = address + 4 * count; break;
                case 2: new_base = address - 4 * count; address = new_base; break;
                case 3: new_base = address + 4 * count; address = address + 4; break;
                default: return 0;
            }//switch

            bool special = false;
            if (write_back)
            {
                if (Rn == first_reg)
                    special = true;
                else
                    GPR[Rn] = new_base;
            }//if

            uint reg = 0;
            while (reg_list != 0)
            {
                if (Utils.lsb(reg_list))
                {
                    SystemMemory.SetMemory(address, WordSize.FourByte, get_reg(reg));
                    address += 4;
                }//if
                reg_list >>= 1;
                ++reg;
            }//while
            if (special)
                GPR[Rn] = new_base;

            return (1 + count);
        }//stm

        /// <summary>
        /// LDM - perform a multi-register load from memory
        /// </summary>
        /// <param name="mode">Mode:the PU bits in the opcode</param>
        /// 00:DA (Decrement After)
        /// 01:IA (Increment After)
        /// 10:DB (Decrement Before)
        /// 11:IB (Increment Before)
        /// <param name="Rn">base register value</param>
        /// <param name="reg_list">list of registers to load(bottom 16 bits)</param>
        /// <param name="write_back">flag indicating if base register be updated</param>
        /// <param name="userModeLoad">flag indicating if values loaded into user mode registers</param>
        /// <returns>Number of clock cycles executed</returns>
        private uint ldm(uint mode, uint Rn, uint reg_list, bool write_back, bool userModeLoad)
        {
            uint address = Utils.valid_address(get_reg(Rn));  //make sure it is word aligned.
            int first_reg;
            uint count = bit_count(reg_list, out first_reg);
            bool includesPC = ((reg_list & 0x00008000) != 0);

            uint new_base;
            switch (mode)
            {
                case 0: new_base = address - 4 * count; address = new_base + 4; break;  //DA (Decrement After)
                case 1: new_base = address + 4 * count; break;                          //IA (Increment After)
                case 2: new_base = address - 4 * count; address = new_base; break;      //DB (Decrement Before)
                case 3: new_base = address + 4 * count; address = address + 4; break;   //IB (Increment Before)
                default: return 0;
            }//switch

            if (write_back)
                GPR[Rn] = new_base;

            uint reg = 0;
            uint extraCycles = 0;

            //check
            while (reg_list != 0)
            {
                if (Utils.lsb(reg_list))
                {
                    uint data = SystemMemory.GetMemory(address, WordSize.FourByte);
                    if (reg == GeneralPurposeRegisters.PCRegisterIndex)
                    {//We are loading a new value into the PC, check if entering Thumb or ARM mode
                        extraCycles = 2;

                        //if the user mode flag is set and r15 is being loaded, we are executing
                        //LDM(3) and returning from an exception. We may be returning to Thumb mode or
                        //ARM mode. In each case, force align the returning address.
                        if (userModeLoad)
                        {//we are returning from an exception, check if returning to ARM or Thumb mode
                            if ((CPSR.SPSR & 0x0020) != 0)//check thumb bit of saved CPSR
                                data = data & 0xfffffffe;   //returning to thumb mode, force align HWord
                            else
                                data = data & 0xfffffffc;   //returning to arm mode, force align Word
                        }
                        else if ((data & 0x01) != 0)
                        {//Entering Thumb mode. Make sure destination address is HWord aligned
                            CPSR.tf = true;
                            data = data & 0xfffffffe;
                        }//if
                        else
                        {//Entering ARM mode. Make sure destination address is Word aligned
                            CPSR.tf = false;
                            data = data & 0xfffffffc;
                        }//else

                    }//if

                    if (userModeLoad & !includesPC)
                        GPR.setUserModeRegister(reg, data);
                    else
                        GPR[reg] = data;

                    address += 4;
                }//if
                reg_list >>= 1;
                ++reg;
            }//while

            //if the PC was loaded during this instruction and the user mode load was specified
            //then we are returning from an exception.
            if (includesPC && userModeLoad)
                CPSR.RestoreCPUMode();

            return (2 + count + extraCycles);
        }//ldm

        /// <summary>
        /// LDM/STM instruction. Determine the parameters and call common code
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns>Number of clock cycles executed</returns>
        private uint multiple(uint opcode)
        {
            uint mode = (opcode & 0x01800000) >> 23;            //isolate PU bits
            uint Rn = (opcode & rn_mask) >> 16;                 //get base register
            uint reg_list = opcode & 0x0000ffff;                //get the register list
            bool writeback = (opcode & write_back_mask) != 0;   //determine writeback flag

            if ((opcode & load_mask) == 0)
                return stm(mode, Rn, reg_list, writeback);
            else
            {
                bool userModeLoad = ((opcode & 0x00400000) != 0);    //get user mode load flag
                return ldm(mode, Rn, reg_list, writeback, userModeLoad);
            }

        }//multiple

        private uint swi_op(uint op_code)
        {
            uint p1 = op_code & 0x00ffffff;
            this.FireInterupt("SWI");
            return 1;
        }

    }//ARMInstructions
}