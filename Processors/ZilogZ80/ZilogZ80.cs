using System;
using System.Collections.Generic;
using Intel8085;
using Microsoft.Win32;
using Preferences;
using Processors;

namespace ZilogZ80
{
    public partial class ZilogZ80 : Intel8085.Intel8085
    {
        private performArithmeticDelegate8[] mFuncs8;
        private RegistersView mRegistersView;

        public override IList<IInstructionTest> GetInstructionTests()
        {
            return InstructionTests.InstructionTestsZ80.TestsZ80;
        }
        public override string ParserName { get { return mZilogZ80ProcessorConfig.ActiveParser; } }

        public override IRegistersView RegistersView { get { return mRegistersView; } }
        private ZilogZ80ProcessorConfig mZilogZ80ProcessorConfig;
        public override PreferencesBase Settings
        { get { return mZilogZ80ProcessorConfig; } }
        public override void SaveSettings(PreferencesBase settings)
        {
            PreferencesBase.Save<ZilogZ80ProcessorConfig>(mZilogZ80ProcessorConfig, ZilogZ80ProcessorConfig.Key);
        }

        public override void OnRaiseInterrupt(string interrupt)
        {
            if (!InterruptsEnabled)
                return;
            if (mInteruptMode != 2)
                return;

            if (string.IsNullOrEmpty(interrupt))
                return;

            int pos = interrupt.IndexOf(':');
            if (pos < 0)
                return;

            string vectorS = interrupt.Substring(pos+1);
            if (!byte.TryParse(vectorS, System.Globalization.NumberStyles.HexNumber, null, out byte res))
                return;
            
            byte[] bytes = new byte[] { res, (Registers as RegistersZ80).I };
            ushort addr = BitConverter.ToUInt16(bytes, 0);
            var vec = (ushort)SystemMemory.GetMemory(addr, WordSize.TwoByte, false);

            Push((ushort)Registers.PC);
            Registers.PC = vec;
            InterruptsEnabled = false;
        }

        public override string[] DoubleRegisters()
        {
            return new string[]
                {
                    "ix",
                    "iy",
                    "hl",
                    "de",
                    "bc",
                    "sp",
                    "pc"
                };
        }

        public ZilogZ80(ISystemMemory systemMemory) : base(systemMemory)
        {
            Registers = new RegistersZ80();
            mRegistersView = new RegistersView(this, Registers as RegistersZ80);
            mZilogZ80ProcessorConfig = PreferencesBase.Load<ZilogZ80ProcessorConfig>(ZilogZ80ProcessorConfig.Key);

            mOpcodeFunctions[0x08] = handle08;
            mOpcodeFunctions[0x10] = handle10;
            mOpcodeFunctions[0x18] = handle18;
            mOpcodeFunctions[0x20] = handle20;
            mOpcodeFunctions[0x28] = handle28;
            mOpcodeFunctions[0x30] = handle30;
            mOpcodeFunctions[0x38] = handle38;
            mOpcodeFunctions[0xcb] = handlecb;
            mOpcodeFunctions[0xd9] = handled9;
            mOpcodeFunctions[0xdd] = handledd;
            mOpcodeFunctions[0xed] = handleed;
            mOpcodeFunctions[0xfd] = handlefd;

            handleedInit();
            mFuncs8 = new performArithmeticDelegate8[]
            {
                add8, adc8, sub8, sbc8, and8, xor8, or8, cpi8
            };
        }

        //public override IParser CreateParser()
        //{
        //    return Parsers.Parsers.GetParser(ParserName, this);
        //}
        public override IDisassembler CreateDisassembler(int topAddress)
        {
            return new DisassemblerZ80(this);
        }

        //public override bool StepInto(StatusUpdateDelegate callback)
        //{
        //    return base.StepInto(callback);
        //}
        //public override bool StepOver(StatusUpdateDelegate callback)
        //{
        //    return base.StepOver(callback);
        //}

        private uint handle08(byte opcode)
        {//ex af,af'
            var reg = Registers as RegistersZ80;
            reg.Switch(true);
            return 4;
        }

        private uint handle10(byte opcode)
        {//djnz, e
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);
            regZ80.B -= 1;
            if (regZ80.B == 0)
                return 8;

            if (opcode2.Bit7())
                regZ80.PC -= (uint)(byte)(~opcode2 + 1);
            else
                regZ80.PC += opcode2;

            return 15;
        }
        private uint handle18(byte opcode)
        {//jr d
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);
            if (opcode2.Bit7())
                regZ80.PC -= (uint)(byte)(~opcode2 + 1);
            else
                regZ80.PC += opcode2;
            return 12;
        }
        private uint handle20(byte opcode)
        {//jr nz,d
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);

            if (!regZ80.Zero)
            {
                if (opcode2.Bit7())
                    regZ80.PC -= (uint)(byte)(~opcode2 + 1);
                else
                    regZ80.PC += opcode2;
                return 12;
            }
            return 7;
        }
        private uint handle28(byte opcode)
        {//jr z,d
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);

            if (regZ80.Zero)
            {
                if (opcode2.Bit7())
                    regZ80.PC -= (uint)(byte)(~opcode2 + 1);
                else
                    regZ80.PC += opcode2;
                return 12;
            }
            return 7;
        }

        private uint handle30(byte opcode)
        {//jr nc,d
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);

            if (!regZ80.Carry)
            {
                if (opcode2.Bit7())
                    regZ80.PC -= (uint)(byte)(~opcode2 + 1);
                else
                    regZ80.PC += opcode2;
                return 12;
            }
            return 7;
        }

        private uint handle38(byte opcode)
        {//jr c,d
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            byte opcode2 = (byte)SystemMemory.GetMemory(Registers.PC++, WordSize.OneByte, true);

            if (regZ80.Carry)
            {
                if (opcode2.Bit7())
                    regZ80.PC -= (uint)(byte)(~opcode2 + 1);
                else
                    regZ80.PC += opcode2;
                return 12;
            }
            return 7;
        }

        private uint handled9(byte opcode)
        {//exx
            var reg = Registers as RegistersZ80;
            reg.Switch(false);
            return 4;
        }

        //hl+rr+Carry
        private ushort adc16(ushort a, ushort b)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            uint c = regZ80.Carry ? (uint)1 : 0;
            uint result = (uint)(a + b + c);
            regZ80.Carry = ((result & 0xffff0000) != 0);
            return (ushort)result;
        }

        private ushort sub16(ushort a, ushort b)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            uint result = (uint)(a - b);
            regZ80.Carry = !carryResult16(result);
            return (ushort)result;
        }

        /// <summary>
        /// a-b-Carry
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private ushort sbc16(ushort a, ushort b)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            uint c = regZ80.Carry ? (uint)1 : 0;
            uint result = (uint)(a - b - c);
            regZ80.Carry = !carryResult16(result);
            return (ushort)result;
        }

        private ushort and16(ushort a, ushort b)
        {
            (Registers as RegistersZ80).Carry = false;
            return (ushort)(a & b);
        }
        private ushort xor16(ushort a, ushort b)
        {
            (Registers as RegistersZ80).Carry = false;
            (Registers as RegistersZ80).AuxCarry = false;
            return (ushort)(a ^ b);
        }
        private ushort or16(ushort a, ushort b)
        {
            (Registers as RegistersZ80).Carry = false;
            (Registers as RegistersZ80).AuxCarry = false;
            return (ushort)(a | b);
        }
        private ushort cpi16(ushort a, ushort b)
        {
            RegistersZ80 regZ80 = Registers as RegistersZ80;
            uint temp = (ushort)(a + (ushort)~b + 1);
            regZ80.Carry = !carryResult16(temp);
            //todo aux carry?
            //regZ80.AuxCarry = auxCarryResult(a, (byte)~b, true);
            regZ80.setFlagsSZP((byte)temp);
            return 0;
        }
    }
}