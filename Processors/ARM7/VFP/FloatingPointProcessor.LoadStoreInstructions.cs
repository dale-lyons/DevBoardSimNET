namespace ARM7.VFP
{
    /// <summary>
    /// Part of the FloatingPointProcessor class
    /// Handles all the load/store instructions
    /// </summary>
    partial class FloatingPointProcessor
    {
        /// <summary>
        /// Execute a load/store instruction. Decode it and execute it.
        /// </summary>
        /// <param name="opcode">opcode of instruction</param>
        /// <returns>clock cycles used</returns>
        public uint load_store(uint opcode)
        {
            bool singleType = isSingle(opcode);
            uint Rn = UnpackRn(opcode);
            uint Rd = UnpackRd(opcode);
            uint Fm = UnpackFm(opcode);
            bool LBit = ((opcode & 0x00100000) != 0);

            //Check if ARMv5TE extendsion instructions
            if ((opcode & 0x0fe000d0) == 0x0c400010)
            {
                if (!singleType)
                {
                    if (LBit)
                    {
                        //fmrrd
                        mArm7.GPR[Rd] = this._FPR.ReadRaw(Fm, true);
                        mArm7.GPR[Rn] = this._FPR.ReadRaw(Fm, false);
                    }
                    else
                    {
                        //fmdrr
                        this._FPR.WriteRaw(Fm, mArm7.GPR[Rd], true);
                        this._FPR.WriteRaw(Fm, mArm7.GPR[Rn], false);
                    }
                }
                else
                {
                    //single precision
                    //if destination/source register is 15, result is unpredictable. Ignore it.
                    if (Fm < 15)
                    {
                        if (LBit)
                        {
                            //fmrrs
                            this._FPR.WriteRaw(Rd, this._FPR.ReadRaw(Fm));
                            this._FPR.WriteRaw(Rn, this._FPR.ReadRaw(Fm+1));
                        }
                        else
                        {
                            //fmsrr
                            this._FPR.WriteRaw(Fm, this._FPR.ReadRaw(Rd));
                            this._FPR.WriteRaw(Fm + 1, this._FPR.ReadRaw(Rn));
                        }
                    }
                }
                //todo - calculate coeect clock cycles
                return 1;
            }//if

            uint puw = UnpackPUW(opcode);
            bool WBit = ((opcode & 0x00200000) != 0);
            bool UBit = ((opcode & 0x00800000) != 0);
            uint Fd = UnpackFd(opcode);

            uint start_reg = singleType ? Fd : Fd * 2;

            uint count = singleType ? (opcode & 0x0f) : (opcode & 0x0e);
            uint address = Utils.valid_address(mArm7.GPR[Rn]);
            uint offset = ((opcode & 0xff)*4);
            if (Rn == 15) address += 4;

            uint new_base = 0;
            switch (puw)
            {
                case 0:
                case 1:
                case 7:
                    //invalid, undefined
                    return 0;

                case 2:
                case 3:
                case 5:
                    //multi
                    if (UBit)
                    {
                        new_base = address + 4 * count;
                    }
                    else
                    {
        				new_base = address - 4 * count; address = new_base;
                    }
                    break;

                case 4:
                case 6:
                    //single
                    if (UBit)
                    {
                        address += offset;
                    }
                    else
                    {
                        address -= offset;
                    }
                    count = singleType ? (uint)1 : (uint)2;
                    break;
            }//switch

            if (count + start_reg > 32) return 1;
            for (uint ii = start_reg; count > 0; ii++, count--)
            {
                var systemMemory = mArm7.SystemMemory;
                if (LBit)
                    mArm7.FPP.FPR.WriteRaw(ii, systemMemory.GetMemory(address, Processors.WordSize.FourByte, true));
                else
                    systemMemory.SetMemory(address, Processors.WordSize.FourByte, mArm7.FPP.FPR.ReadRaw(ii), true);
                address += 4;
            }
            if (WBit)
            {
                mArm7.GPR[Rn] = new_base;
            }
            //todo - calculate coeect clock cycles
            return 1;
        }//load_store
    }//class FloatingPointProcessor
}
