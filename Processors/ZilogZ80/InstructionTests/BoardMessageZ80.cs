using Intel8085;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZilogZ80.InstructionTests
{
    public class BoardMessageZ80222
    {
        private static bool gotone;
        public enum ImmDataSizeEnum
        {
            SWord = 0x01,
            SByte = 0x02,
        }
        public enum PrebyteSizeEnum
        {
            SZeroB = 0x00,
            SOneB = 0x01,
            STwoB = 0x02,
            SThreeB = 0x03
        }

        public bool Valid;
        private byte[] buff = new byte[2];
        public BoardMessageZ80222() { }

        public static BoardMessageZ80222 FromStream(byte[] bytes)
        {
            if (bytes.Length != 18)
                throw new Exception();

            if (gotone)
            {

            }
            else
                gotone = true;

            var ret = new BoardMessageZ80222();
            ret.flags = bytes[1];
            ret.a = bytes[2];
            ret.b = bytes[4];
            ret.c = bytes[3];
            ret.d = bytes[6];
            ret.e = bytes[5];
            ret.h = bytes[8];
            ret.l = bytes[7];
            ret.SP = BitConverter.ToUInt16(bytes, 9);
            ret.IX = BitConverter.ToUInt16(bytes, 11);
            ret.Data = BitConverter.ToUInt16(bytes, 13);
            ret.IY = BitConverter.ToUInt16(bytes, 15);
            ret.Cs = bytes[17];
            return ret;
        }

        public byte tst { get; set; }
        public byte flags { get; set; }
        public byte PrebyteSize { get; set; } = (byte)PrebyteSizeEnum.SZeroB;
        public byte Prebyte1 { get; set; }
        public byte Prebyte2 { get; set; }
        public byte Opcode { get; set; }
        public byte a { get; set; }
        public byte b { get; set; }
        public byte c { get; set; }
        public byte d { get; set; }
        public byte e { get; set; }
        public byte h { get; set; }
        public byte l { get; set; }
        public ushort SP { get; set; }
        public ushort Addr { get; set; } = 0;
        public ushort Data { get; set; } = 0;
        public byte useIMMData { get; set; } = 0;
        public ushort ImmData { get; set; } = 0;
        public ushort IX { get; set; }
        public ushort IY { get; set; }
        //public byte dd { get; set; }
        public byte Cs { get; set; } = 0;

        public byte[] ToStream()
        {
            byte[] ret = null;
            using (var ms = new MemoryStream())
            {
                ms.WriteByte((byte)':');
                ms.WriteByte(0);                                //length
                ms.WriteByte(tst);
                ms.WriteByte(PrebyteSize);                      //+3 board
                ms.WriteByte(Prebyte1);
                ms.WriteByte(Opcode);
                ms.WriteByte(useIMMData);
                ms.WriteByte((byte)(ImmData & 0xff));
                ms.WriteByte((byte)(ImmData >> 8));
                ms.WriteByte(flags);
                ms.WriteByte(a);                                //+10
                ms.WriteByte(c);
                ms.WriteByte(b);
                ms.WriteByte(e);
                ms.WriteByte(d);
                ms.WriteByte(l);
                ms.WriteByte(h);                                //+16
                ms.WriteByte((byte)(SP & 0xff));
                ms.WriteByte((byte)(SP >> 8));
                ms.WriteByte((byte)(Addr & 0xff));              //+19
                ms.WriteByte((byte)(Addr >> 8));                //+20
                ms.WriteByte((byte)(Data & 0xff));
                ms.WriteByte((byte)(Data >> 8));
                ms.WriteByte((byte)(IX & 0xff));                //+23
                ms.WriteByte((byte)(IX >> 8));
                ms.WriteByte((byte)(IY & 0xff));                //+25
                ms.WriteByte((byte)(IY >> 8));
                ms.WriteByte(Prebyte2);                        //+27
                ms.WriteByte(0);                               //+28   (checksum)
                ret = ms.ToArray();
            }
            ret[1] = (byte)(ret.Length - 3);
            Cs = 0;
            for (int ii = 1; ii < ret.Length - 1; ii++)
                Cs += ret[ii];
            ret[ret.Length - 1] = Cs;
            return ret;
        }

        public ushort getDblreg(byte reg)
        {
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b:
                    return BC;
                case (byte)DoubleRegisterEnums.d:
                    return DE;
                case (byte)DoubleRegisterEnums.h:
                    return HL;
                case (byte)DoubleRegisterEnums.sp:
                    return SP;
                case (byte)DoubleRegisterEnums.psw:
                    return PSW;
                case (byte)DoubleRegisterEnums.ix:
                    return IX;
                case (byte)DoubleRegisterEnums.iy:
                    return IY;
                default:
                    throw new Exception();
            }
        }

        public void setDblreg(byte reg, ushort val)
        {
            switch (reg)
            {
                case (byte)DoubleRegisterEnums.b:
                    BC = val; break;
                case (byte)DoubleRegisterEnums.d:
                    DE = val; break;
                case (byte)DoubleRegisterEnums.h:
                    HL = val; break;
                case (byte)DoubleRegisterEnums.sp:
                    SP = val; break;
                case (byte)DoubleRegisterEnums.psw:
                    PSW = val; break;
                case (byte)DoubleRegisterEnums.ix:
                    IX = val; break;
                case (byte)DoubleRegisterEnums.iy:
                    IY = val; break;
                default:
                    throw new Exception();
            }
        }

        public void setSinglereg(byte reg, byte val)
        {
            switch (reg)
            {
                case (byte)SingleRegisterEnums.b:
                    b = val; break;
                case (byte)SingleRegisterEnums.c:
                    c = val; break;
                case (byte)SingleRegisterEnums.d:
                    d = val; break;
                case (byte)SingleRegisterEnums.e:
                    e = val; break;
                case (byte)SingleRegisterEnums.h:
                    h = val; break;
                case (byte)SingleRegisterEnums.l:
                    l = val; break;
                case (byte)SingleRegisterEnums.a:
                    a = val; break;
                default:
                    throw new Exception();
            }
        }

        public byte getSinglereg(byte reg)
        {
            switch (reg)
            {
                case (byte)SingleRegisterEnums.b:
                    return b;
                case (byte)SingleRegisterEnums.c:
                    return c;
                case (byte)SingleRegisterEnums.d:
                    return d;
                case (byte)SingleRegisterEnums.e:
                    return e;
                case (byte)SingleRegisterEnums.h:
                    return h;
                case (byte)SingleRegisterEnums.l:
                    return l;
                case (byte)SingleRegisterEnums.a:
                    return a;
                default:
                    throw new Exception();
            }
        }
        public ushort PSW
        {
            get
            {
                buff[0] = flags;
                buff[1] = a;
                return BitConverter.ToUInt16(buff, 0);
            }
            set
            {
                buff = BitConverter.GetBytes(value);
                flags = buff[0];
                a = buff[1];
            }
        }

        public ushort BC
        {
            get
            {
                buff[0] = c;
                buff[1] = b;
                return BitConverter.ToUInt16(buff, 0);
            }
            set
            {
                buff = BitConverter.GetBytes(value);
                c = buff[0];
                b = buff[1];
            }
        }
        public ushort DE
        {
            get
            {
                buff[0] = e;
                buff[1] = d;
                return BitConverter.ToUInt16(buff, 0);
            }
            set
            {
                buff = BitConverter.GetBytes(value);
                e = buff[0];
                d = buff[1];
            }
        }
        public ushort HL
        {
            get
            {
                buff[0] = l;
                buff[1] = h;
                return BitConverter.ToUInt16(buff, 0);
            }
            set
            {
                buff = BitConverter.GetBytes(value);
                l = buff[0];
                h = buff[1];
            }
        }
    }
}