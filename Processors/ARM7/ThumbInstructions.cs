﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Processors;

namespace ARM7
{
    public partial class ARM7
    {
        public uint ExecuteThumbInstruction(uint opcode)
        {
            uint cycleCount = 0;
            opcode = opcode & 0XFFFF;
            switch (opcode & 0XE000)
            {
                case 0X0000: cycleCount = data0(opcode); break;
                case 0X2000: cycleCount = data1(opcode); break;
                case 0X4000: cycleCount = data_transfer(opcode); break;
                case 0X6000: cycleCount = transfer0(opcode); break;
                case 0X8000: cycleCount = transfer1(opcode); break;
                case 0XA000: cycleCount = sp_pc(opcode); break;
                case 0XC000: cycleCount = lsm_b(opcode); break;
                case 0XE000: cycleCount = thumb_branch(opcode); break;
            }
            return cycleCount;
        }

        /// <summary>
        /// PC and SP modifying instructions
        /// </summary>
        /// <param name="opcode"></param>
        /// <returns></returns>
        private uint sp_pc(uint opcode)
        {
            if ((opcode & 0x1000) == 0)
            {//ADD SP or PC
                uint Rd = (opcode >> 8) & 0x07;
                uint data;
                if ((opcode & 0x0800) == 0)
                {//ADD(5) -PC
                    data = (get_reg(GeneralPurposeRegisters.PCRegisterIndex) & 0xfffffffc) + ((opcode & 0x00ff) << 2);//get_reg supplies PC + 2
                }
                else
                {//ADD(6) -SP
                    data = (GPR.SP + ((opcode & 0x00ff) << 2));
                }
                GPR[Rd] = data;
            }
            else
            {//Adjust SP
                switch (opcode & 0x0f00)
                {
                    case 0x0000:
                        {//ADD(7)-SP or SUB(4)-SP
                            uint sp;
                            uint offset = (opcode & 0x7f) << 2;
                            if ((opcode & 0x0080) == 0)
                            {//ADD(7) -SP
                                sp = GPR.SP + (offset);
                            }
                            else
                            {//SUB(4) -SP
                                sp = GPR.SP - (offset);
                            }
                            GPR.SP = sp;
                        }
                        break;

                    case 0X0400:
                    case 0X0500:
                    case 0X0C00:
                    case 0X0D00:
                        {
                            uint reg_list = opcode & 0x000000ff;

                            if ((opcode & 0x0800) == 0)
                            {//PUSH
                                if ((opcode & 0x0100) != 0)
                                    reg_list = reg_list | 0X4000;
                                stm(2, 13, reg_list, true);
                            }//if
                            else
                            {//POP
                                if ((opcode & 0x0100) != 0)
                                    reg_list = reg_list | 0x8000;
                                ldm(1, 13, reg_list, true, false);
                            }//else
                        }
                        break;

                    case 0X0E00:
                        {//Breakpoint
                            //todo what do we do here?
                        }
                        break;

                    case 0X0100:
                    case 0X0200:
                    case 0X0300:
                    case 0X0600:
                    case 0X0700:
                    case 0X0800:
                    case 0X0900:
                    case 0X0A00:
                    case 0X0B00:
                    case 0X0F00:
                        break;
                }//switch
            }
            return 1;
        }//sp_pc

        private uint lsm_b(uint opcode)
        {
            if ((opcode & 0x1000) == 0)
            {
                if ((opcode & 0x0800) == 0)
                {//STM (IA)
                    stm(1, (opcode >> 8) & 0x07, opcode & 0x00ff, true);
                }
                else
                {//LDM (IA)
                    ldm(1, (opcode >> 8) & 0x07, opcode & 0x00ff, true, false);
                }
            }
            else
            {//conditional BRANCH B(1)
                if ((opcode & 0x0f00) != 0x0f00)
                {//Branch, not a SWI
                    if (check_cc((byte)(opcode >> 8)))
                    {
                        uint offset = (opcode & 0x00ff) << 1;       //sign extend
                        if ((opcode & 0x0080) != 0)
                            offset = offset | 0xfffffe00;

                        GPR.PC = get_reg(GeneralPurposeRegisters.PCRegisterIndex) + offset;                 //get_reg supplies pc + 2
                    }//if
                }//if
            }
            return 1;
        }//lsm_b

        private uint transfer1(uint opcode)
        {
            uint Rd = (opcode & 0x07);
            uint Rn = ((opcode >> 3) & 0x07);

            switch (opcode & 0x1800)
            {
                case 0x0000:
                    {//STRH (1)
                        uint data = get_reg(Rd);
                        uint location = get_reg(Rn) + ((opcode >> 5) & 0x3e);      //x2 in shift
                        SystemMemory.SetMemory(location, WordSize.TwoByte, data);
                    }
                    break;

                case 0x0800:
                    {//LDRH (1)
                        uint location = get_reg(Rn) + ((opcode >> 5) & 0x3e);      //x2 in shift
                        uint data = SystemMemory.GetMemory(location, WordSize.TwoByte);
                        GPR[Rd] = data;
                    }
                    break;

                case 0x1000:
                    {//STR (3) -SP
                        uint data = get_reg(((opcode >> 8) & 0x07));
                        uint location = GPR.SP + ((opcode & 0x00ff) * 4);
                        SystemMemory.SetMemory(location, WordSize.FourByte, data);

                    }
                    break;

                case 0x1800:
                    {//LDR (4) -SP
                        uint location = GPR.SP + ((opcode & 0x00ff) * 4);                    //x2 in shift
                        uint data = SystemMemory.GetMemory(location, WordSize.FourByte);
                        GPR[(opcode >> 8) & 0x07] = data;
                    }
                    break;

            }//switch
            return 1;
        }//transfer1

        private uint transfer0(uint opcode)
        {
            uint Rd = (opcode & 0x07);
            uint Rn = ((opcode >> 3) & 0x07);

            if ((opcode & 0x0800) == 0)
            {//STR
                uint data = get_reg(Rd);
                if ((opcode & 0x1000) == 0)
                {//STR (1) 5-bit imm
                    uint location = get_reg(Rn) + ((opcode >> 4) & 0x07c);            //shift twice = *4
                    SystemMemory.SetMemory(location, WordSize.FourByte, data);
                }
                else
                {//STRB (1)
                    uint location = get_reg(Rn) + ((opcode >> 6) & 0x1f);
                    SystemMemory.SetMemory(location, WordSize.OneByte, data);
                }//else
            }//if
            else
            {//LDR (1)
                uint data;
                if ((opcode & 0x1000) == 0)
                {
                    uint location = (get_reg(Rn) + ((opcode >> 4) & 0x07c));       //shift twice = *4
                    data = SystemMemory.GetMemory(location, WordSize.FourByte);

                }
                else
                {//LDRB (1)
                    uint location = (get_reg(Rn) + ((opcode >> 6) & 0x1f));
                    data = SystemMemory.GetMemory(location, WordSize.OneByte);
                }//else
                GPR[Rd] = data;
            }//else
            return 1;
        }//transfer0

        private uint thumb_branch(uint opcode)
        {
            uint offset;
            switch (opcode & 0X1800)
            {
                case 0x0000:
                    //B -uncond. B(2)
                    offset = (opcode & 0X07FF) << 1;
                    if ((opcode & 0x0400) != 0)
                        offset = offset | 0xfffff000;//sign extend
                    GPR.PC = get_reg(GeneralPurposeRegisters.PCRegisterIndex) + offset;
                    break;

                case 0x0800:
                    //BLX
                    if ((opcode & 0X0001) == 0)
                        thumb_branch1(opcode, true);
                    break;

                case 0x1000:
                    {
                        //BL prefix
                        uint BL_prefix = opcode & 0x07ff;
                        offset = BL_prefix << 12;

                        if ((BL_prefix & 0x0400) != 0)
                            offset = offset | 0xff800000;//sign extend
                        offset = get_reg(GeneralPurposeRegisters.PCRegisterIndex) + offset;
                        GPR.LR = offset;
                    }
                    break;

                case 0x1800:
                    //BL
                    thumb_branch1(opcode, false);
                    break;
            }//switch
            return 1;
        }//thumb_branch

        private void thumb_branch1(uint opcode, bool exchange)
        {
            uint offset = GPR.LR + ((opcode & 0x07ff) << 1);//Retrieve first part of offset
            uint lr = get_reg(GeneralPurposeRegisters.PCRegisterIndex) - 2 + 1;       //+ 1 to indicate Thumb mode

            if (exchange)
            {
                CPSR.tf = false;       //Change to ARM mode
                offset = offset & 0xfffffffc;
            }

            GPR.PC = offset;
            GPR.LR = lr;
        }//thumb_branch1

        private uint data0(uint opcode)
        {
            if ((opcode & 0x1800) != 0x1800)
            {//Either LSL,LSR or ASR
                uint Rm = ((opcode >> 3) & 0x07);       //Source register
                uint Rd = (opcode & 0x07);              //Destination register
                uint shift = ((opcode >> 6) & 0x1f);    //Shift value

                uint result = 0;
                bool cf = (this.CPSR.cf);

                switch (opcode & 0X1800)
                {
                    case 0x0000:
                        //LSL(1)
                        result = lsl(get_reg(Rm), shift, ref cf);
                        break;
                    case 0x0800:
                        //LSR(1)
                        //A shift value of 0 means shift by 32
                        if (shift == 0)
                            shift = 32;
                        result = lsr(get_reg(Rm), shift, ref cf);
                        break;
                    case 0x1000:
                        //ASR(1)
                        //A shift value of 0 means shift by 32
                        if (shift == 0)
                            shift = 32;
                        result = asr(get_reg(Rm), shift, ref cf);
                        break;
                    default:
                        //Can't get here, but just in case
                        return 1;
                }//switch
                this.CPSR.cf = cf;
                this.CPSR.set_NZ(result);
                GPR[Rd] = result;
            }//if
            else
            {//Either ADD or SUB
                uint Rd = (opcode & 0x07);              //Destination register
                uint Rn = ((opcode >> 3) & 0x07);
                uint Rm = ((opcode >> 6) & 0x07);

                uint op2;
                if ((opcode & 0x0400) == 0)
                {//ADD(3) or SUB(3)
                    op2 = get_reg(Rm);
                }
                else
                {//ADD(1) or SUB(1)
                    op2 = Rm;
                }

                uint result;
                uint op1 = get_reg(Rn);
                if ((opcode & 0x0200) == 0)
                {
                    result = op1 + op2;
                    CPSR.set_flags_add(op1, op2, result, false);
                }
                else
                {
                    result = op1 - op2;
                    CPSR.set_flags_sub(op1, op2, result, true);
                }//else
                GPR[Rd] = result;
            }//else

            return 1;
        }//data0(Thumb)

        private uint data_transfer(uint opcode)
        {
            if ((opcode & 0x1000) == 0)
            {//NOT load/store
                if ((opcode & 0x0800) == 0)
                {//NOT load literal pool
                    if ((opcode & 0x0400) == 0)
                    {//Data processing

                        uint Rd = (opcode & 0x07);
                        uint Rm = ((opcode >> 3) & 7);

                        uint op1 = get_reg(Rd);
                        uint op2 = get_reg(Rm);
                        uint result;

                        //data processing opcode
                        switch (opcode & 0x03c0)
                        {
                            case 0x0000:
                                //AND
                                result = op1 & op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;

                            case 0x0040:
                                //EOR
                                result = op1 ^ op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;

                            case 0x0080:
                                {
                                    //LSL(2)
                                    bool cf = CPSR.cf;
                                    result = lsl(op1, op2 & 0x00ff, ref cf);
                                    CPSR.cf = cf;
                                    CPSR.set_NZ(result);
                                    GPR[Rd] = result;
                                }
                                break;

                            case 0x00c0:
                                //LSR(2)
                                {
                                    bool cf = CPSR.cf;
                                    result = lsr(op1, op2 & 0x00ff, ref cf);
                                    CPSR.cf = cf;
                                    CPSR.set_NZ(result);
                                    GPR[Rd] = result;
                                }
                                break;

                            case 0x0100:
                                //ASR (2)
                                {
                                    bool cf = CPSR.cf;
                                    result = asr(op1, op2 & 0x00ff, ref cf);
                                    CPSR.cf = cf;
                                    CPSR.set_NZ(result);
                                    GPR[Rd] = result;
                                }
                                break;

                            case 0x0140:
                                //ADC
                                result = op1 + op2;
                                if (CPSR.cf)
                                    result++;//Add CF

                                CPSR.set_flags_add(op1, op2, result, CPSR.cf);
                                GPR[Rd] = result;
                                break;

                            case 0x0180:
                                //SBC
                                result = op1 - op2 - 1;
                                if (CPSR.cf)
                                    result++;//Add CF

                                CPSR.set_flags_sub(op1, op2, result, CPSR.cf);
                                GPR[Rd] = result;
                                break;

                            case 0x01c0:
                                {//ROR
                                    bool cf = CPSR.cf;
                                    result = ror(op1, op2 & 0x00ff, ref cf);
                                    CPSR.cf = cf;
                                    CPSR.set_NZ(result);
                                    GPR[Rd] = result;
                                }
                                break;

                            case 0x0200:
                                //TST
                                CPSR.set_NZ(op1 & op2);
                                break;

                            case 0x0240:
                                //NEG
                                result = (uint)(0 - (int)op2);
                                CPSR.set_flags_sub(0, op2, result, true);
                                GPR[Rd] = result;
                                break;

                            case 0x0280:
                                //CMP(2)
                                CPSR.set_flags_sub(op1, op2, op1 - op2, true);
                                break;

                            case 0x02c0:
                                //CMN
                                CPSR.set_flags_add(op1, op2, op1 + op2, false);
                                break;

                            case 0x0300:
                                //ORR
                                result = op1 | op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;

                            case 0x0340:
                                //MUL
                                result = op1 * op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;

                            case 0x0380:
                                //BIC
                                result = op1 & ~op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;

                            case 0x03c0:
                                //MVN
                                result = ~op2;
                                CPSR.set_NZ(result);
                                GPR[Rd] = result;
                                break;
                        }//switch
                    }
                    else
                    {//special data processing -- NO FLAG UPDATE
                        //ADD(4),CMP(3),MOV(2),BX,BLX
                        uint Rd = ((opcode & 0x0080) >> 4) | (opcode & 0x07);
                        uint Rm = ((opcode >> 3) & 15);

                        switch (opcode & 0x0300)
                        {
                            case 0x0000:
                                //ADD (4) high registers
                                GPR[Rd] = get_reg(Rd) + get_reg(Rm);
                                break;

                            case 0x0100:
                                {//CMP (3) high registers
                                    uint op1 = get_reg(Rd);
                                    uint op2 = get_reg(Rm);
                                    CPSR.set_flags_sub(op1, op2, op1 - op2, true);
                                }
                                break;

                            case 0x0200:
                                {//MOV (2) high registers
                                    uint op2 = get_reg(Rm);
                                    if (Rd == GeneralPurposeRegisters.PCRegisterIndex)
                                        op2 = op2 & 0xfffffffe;//Tweak mov to PC
                                    GPR[Rd] = op2;
                                }
                                break;

                            case 0x0300:
                                {//BX/BLX Rm
                                    bool link = ((opcode & 0x0080) != 0);
                                    bx((opcode >> 3) & 0x0f, link);
                                }
                                break;
                        }//switch
                    }
                }
                else
                {//load from literal pool -- LDR PC
                    uint Rd = ((opcode >> 8) & 0x07);
                    uint address = (((opcode & 0x00ff) << 2)) + (get_reg(GeneralPurposeRegisters.PCRegisterIndex) & 0xfffffffc);
                    GPR[Rd] = SystemMemory.GetMemory(address, WordSize.FourByte);

                }//else
            }
            else
            {//load/store word, halfword, byte, signed byte
                uint Rd = (opcode & 0x07);
                uint Rn = ((opcode >> 3) & 0x07);
                uint Rm = ((opcode >> 6) & 0x07);

                uint address = get_reg(Rn) + get_reg(Rm);

                switch (opcode & 0X0E00)
                {
                    case 0x0000:
                        //STR (2) register
//                        SetMemory(address, ARMPluginInterfaces.MemorySize.Word, get_reg(Rd));
                        SystemMemory.SetMemory(address, WordSize.FourByte, get_reg(Rd));
                        //write_mem (rn + rm, get_reg (rd), 4, FALSE, mem_system);
                        break;

                    case 0x0200:
                        //STRH (2) register
                        //SetMemory(address, ARMPluginInterfaces.MemorySize.HalfWord, get_reg(Rd));
                        SystemMemory.SetMemory(address, WordSize.TwoByte, get_reg(Rd));
                        //write_mem(rn + rm, get_reg(rd), 2, FALSE, mem_system);
                        break;

                    case 0x0400:
                        //STRB (2) register
                        //SetMemory(address, ARMPluginInterfaces.MemorySize.Byte, get_reg(Rd));
                        SystemMemory.SetMemory(address, WordSize.OneByte, get_reg(Rd));

                        //write_mem(rn + rm, get_reg(rd), 1, FALSE, mem_system);
                        break;

                    case 0x0600:
                        //LDRSB register
                        GPR[Rd] = SystemMemory.GetMemory(address, WordSize.OneByte);
                        break;

                    case 0x0800:
                        //LDR (2) register
                        GPR[Rd] = SystemMemory.GetMemory(address, WordSize.FourByte);
                        break;

                    case 0x0a00:
                        //LDRH (2) register
                        GPR[Rd] = SystemMemory.GetMemory(address, WordSize.TwoByte);
                        break;

                    case 0x0c00:
                        //LDRB (2)
                        GPR[Rd] = SystemMemory.GetMemory(address, WordSize.OneByte);
                        break;

                    case 0x0e00:
                        //LDRSH (2)
                        GPR[Rd] = SystemMemory.GetMemory(address, WordSize.TwoByte);
                        break;
                }//switch
            }
            return 1;
        }//data_transfer(Thumb)

        private uint data1(uint opcode)
        {
            uint Rd = (opcode >> 8) & 0x07;
            uint imm = opcode & 0x00ff;
            uint result;
            switch (opcode & 0x1800)
            {
                case 0x0000:
                    //MOV (1)
                    result = imm;
                    CPSR.set_NZ(result);
                    GPR[Rd] = result;
                    break;

                case 0x0800:
                    {//CMP (1)
                        uint op1 = get_reg(Rd);
                        result = (op1 - imm);
                        CPSR.set_flags_sub(op1, imm, result, true);
                    }
                    break;

                case 0x1000:
                    {//ADD (2)
                        uint op1 = get_reg(Rd);
                        result = (op1 + imm);
                        CPSR.set_flags_add(op1, imm, result, false);
                        GPR[Rd] = result;
                    }
                    break;

                case 0x1800:
                    {//SUB (2)
                        uint op1 = get_reg(Rd);
                        result = (op1 - imm);
                        CPSR.set_flags_sub(op1, imm, result, true);
                        GPR[Rd] = result;
                    }
                    break;
            }//switch
            return 1;
        }//data1
    }
}