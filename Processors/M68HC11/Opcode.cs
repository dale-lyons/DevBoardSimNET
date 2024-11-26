using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M68HC11
{
    public enum OpcodesEnum
    {
        OP_ABA = 0x1b,
        OP_ABX = 0x3a,

        OP_ADCA_DIR = 0x89,
        OP_ADCA_IMM = 0x99,
        OP_ADCA_EXT = 0xb9,
        OP_ADCA_IND_X = 0xa9,

        OP_PRE_BYTE_18 = 0x18,
        OP_PRE_BYTE_1A = 0x1a,
        OP_PRE_BYTE_CD = 0xcd,

        OP_ADCB_DIR = 0xc9,
        OP_ADCB_IMM = 0xd9,
        OP_ADCB_EXT = 0xf9,
        OP_ADCB_IND_X = 0xe9,

        OP_ADDA_IMM = 0x8b,
        OP_ADDA_DIR = 0x9b,
        OP_ADDA_EXT = 0xbb,
        OP_ADDA_IND_X = 0xab,

        OP_ADDB_IMM = 0xcb,
        OP_ADDB_DIR = 0xdb,
        OP_ADDB_EXT = 0xfb,
        OP_ADDB_IND_X = 0xeb,

        OP_ADDD_IMM = 0xc3,
        OP_ADDD_DIR = 0xd3,
        OP_ADDD_EXT = 0xf3,
        OP_ADDD_IND_X = 0xe3,

        OP_ANDA_IMM = 0x84,
        OP_ANDA_DIR = 0x94,
        OP_ANDA_EXT = 0xb4,
        OP_ANDA_IND_X = 0xa4,

        OP_ANDB_IMM = 0xc4,
        OP_ANDB_DIR = 0xd4,
        OP_ANDB_EXT = 0xf4,
        OP_ANDB_IND_X = 0xe4,

        OP_ASL_EXT = 0x78,
        OP_ASL_IND_X = 0x68,
        OP_ASLA = 0x48,
        OP_ASLB = 0x58,
        OP_ASLD = 0x05,
        OP_ASR_EXT = 0x77,
        OP_ASR_IND_X = 0x67,
        OP_ASRA = 0x47,
        OP_ASRB = 0x57,

        OP_BCC = 0x24,
        OP_BCS = 0x25,
        OP_BEQ = 0x27,
        OP_BGE = 0x2c,
        OP_BGT = 0x2e,
        OP_BHI = 0x22,
        OP_BHS = 0x24,

        OP_BLE = 0x2f,
        OP_BLO = 0x25, ///< Branch if Lower, same as BCS
        OP_BLS = 0x23,
        OP_BLT = 0x2d,
        OP_BMI = 0x2b,
        OP_BNE = 0x26,
        OP_BPL = 0x2a,
        OP_BRA = 0x20,
        OP_BRN = 0x21, ///< Branch Never
        OP_BRSET_DIR = 0x12, ///< Branch if Bit(s) Set, direct
        OP_BRSET_IND_X = 0x1e, ///< Branch if Bit(s) Set, indirect
        OP_BSR = 0x8d, ///< Branch to Subroutine
        OP_BVC = 0x28,
        OP_BVS = 0x29,

        OP_CLI = 0x0e,
        OP_CLRA = 0x4f,
        OP_CLRB = 0x5f,
        OP_CLV = 0x0a,

        OP_CLR_EXT = 0x7f,
        OP_CLR_IND = 0x6f,

        OP_CMPA_IMM = 0x81,
        OP_CMPA_DIR = 0x91,
        OP_CMPA_EXT = 0xb1,
        OP_CMPA_IND_X = 0xa1,

        OP_CMPB_IMM = 0xc1,
        OP_CMPB_DIR = 0xd1,
        OP_CMPB_EXT = 0xf1,
        OP_CMPB_IND_X = 0xe1,

        OP_CPX_IMM = 0x8c,
        OP_CPX_DIR = 0x9c,
        OP_CPX_EXT = 0xbc,
        OP_CPX_IND = 0xac,

        OP_BITA_IMM = 0x85,
        OP_BITA_DIR = 0x95,
        OP_BITA_EXT = 0xb5,
        OP_BITA_IND_X = 0xa5,
        OP_BITB_IMM = 0xc5,
        OP_BITB_DIR = 0xd5,
        OP_BITB_EXT = 0xf5,
        OP_BITB_IND_X = 0xe5,

        OP_DAA = 0x19,
        OP_DECA = 0x4a,
        OP_DECB = 0x5a,
        OP_DES = 0x34,
        OP_DEX = 0x09,

        OP_EORA_IMM = 0x88,
        OP_EORA_DIR = 0x98,
        OP_EORA_EXT = 0xb8,
        OP_EORA_IND_X = 0xa8,
        OP_EORB_IMM = 0xc8,
        OP_EORB_DIR = 0xd8,
        OP_EORB_EXT = 0xf8,
        OP_EORB_IND_X = 0xe8,

        OP_FDIV = 0x03,
        OP_IDIV = 0x02,
        OP_INC_EXT = 0x7c,
        OP_INC_IND_X = 0x6c,
        OP_INCA = 0x4c,
        OP_INCB = 0x5c,
        OP_INS = 0x31,
        OP_INX = 0x08,

        OP_JMP_EXT = 0x7e,
        OP_JMP_IND_X = 0x6e,

        OP_JSR_DIR = 0x9d,
        OP_JSR_EXT = 0xbd,
        OP_JSR_IND = 0xad,

        OP_LDAA_IMM = 0x86,
        OP_LDAA_DIR = 0x96,
        OP_LDAA_EXT = 0xb6,
        OP_LDAA_IND_X = 0xa6,

        OP_LDAB_IMM = 0xc6,
        OP_LDAB_DIR = 0xd6,
        OP_LDAB_EXT = 0xf6,
        OP_LDAB_IND_X = 0xe6,

        OP_LDD_IMM = 0xcc,
        OP_LDD_DIR = 0xdc,
        OP_LDD_EXT = 0xfc,
        OP_LDD_IND_X = 0xec,

        OP_LDS_IMM = 0x8e,
        OP_LDS_DIR = 0x9e,
        OP_LDS_EXT = 0xbe,
        OP_LDS_IND = 0xae,

        OP_LDX_IMM = 0xce,
        OP_LDX_DIR = 0xde,
        OP_LDX_EXT = 0xfe,
        OP_LDX_IND = 0xee,

        OP_LSL_EXT = 0x78,
        OP_LSL_IND_X = 0x68,
        OP_LSR_EXT = 0x74,
        OP_LSR_IND_X = 0x64,
        OP_LSRA = 0x44,
        OP_LSRB = 0x54,
        OP_LSRD = 0x04,
        OP_MUL = 0x3d,

        OP_ORAA_IMM = 0x8a,
        OP_ORAA_DIR = 0x9a,
        OP_ORAA_EXT = 0xba,
        OP_ORAA_IND_X = 0xaa,
        OP_ORAB_IMM = 0xca,
        OP_ORAB_DIR = 0xda,
        OP_ORAB_EXT = 0xfa,
        OP_ORAB_IND_X = 0xea,

        OP_PSHA = 0x36,
        OP_PSHB = 0x37,
        OP_PSHX = 0x3c,
        OP_PULA = 0x32,
        OP_PULB = 0x33,
        OP_PULX = 0x38,

        OP_ROL_EXT = 0x79,
        OP_ROL_IND = 0x69,
        OP_ROLA = 0x49,
        OP_ROLB = 0x59,
        OP_ROR_EXT = 0x76,
        OP_ROR_IND = 0x66,
        OP_RORA = 0x46,
        OP_RORB = 0x56,
        OP_RTI = 0x3b,
        OP_RTS = 0x39,

        OP_SBA = 0x10,
        OP_SBCA_IMM = 0x82,
        OP_SBCA_DIR = 0x92,
        OP_SBCA_EXT = 0xb2,
        OP_SBCA_IND_X = 0xa2,
        OP_SBCB_IMM = 0xc2,
        OP_SBCB_DIR = 0xd2,
        OP_SBCB_EXT = 0xf2,
        OP_SBCB_IND_X = 0xe2,

        OP_SEC = 0x0d,
        OP_SEI = 0x0f,
        OP_SEV = 0x0b,

        OP_STAA_DIR = 0x97,
        OP_STAA_EXT = 0xb7,
        OP_STAA_IND_X = 0xa7,

        OP_STAB_DIR = 0xd7,
        OP_STAB_EXT = 0xf7,
        OP_STAB_IND_X = 0xe7,

        OP_STD_DIR = 0xdd,
        OP_STD_EXT = 0xfd,
        OP_STD_IND_X = 0xed,

        OP_STOP = 0xcf,
        OP_STS_DIR = 0x9f,
        OP_STS_EXT = 0xbf,
        OP_STS_IND_X = 0xaf,

        OP_STX_DIR = 0xdf,
        OP_STX_EXT = 0xff,
        OP_STX_IND = 0xef,

        OP_SUBA_IMM = 0x80,
        OP_SUBA_DIR = 0x90,
        OP_SUBA_EXT = 0xb0,
        OP_SUBA_IND_X = 0xa0,
        OP_SUBB_IMM = 0xc0,
        OP_SUBB_DIR = 0xd0,
        OP_SUBB_EXT = 0xf0,
        OP_SUBB_IND_X = 0xe0,
        OP_SUBD_IMM = 0x83,
        OP_SUBD_DIR = 0x93,
        OP_SUBD_EXT = 0xb3,
        OP_SUBD_IND_X = 0xa3,

        OP_SWI = 0x3f,
        OP_TAB = 0x16,
        OP_TAP = 0x06,
        OP_TBA = 0x17,
        OP_TEST = 0,
        OP_TPA = 0x07,
        OP_TST_EXT = 0x7d,
        OP_TST_IND_X = 0x6d,
        OP_TSTA = 0x4d,
        OP_TSTB = 0x5d,
        OP_TSX = 0x30,
        OP_TXS = 0x35,
        OP_WAI = 0x3e,
        OP_XGDX = 0x8f,

        // Page 1:
        OP_ABY = 0x3a,
        OP_CPY_IMM = 0x8c,
        OP_DEY = 0x09,

        OP_INY = 0x08,

        OP_LDAA_IND_Y = 0xa6,

        OP_LDY_IMM = 0xce,
        OP_LDY_DIR = 0xde,
        OP_LDY_EXT = 0xfe,
        OP_LDY_IND_Y = 0xee,

        OP_STAA_IND_Y = 0xa7,

        OP_STY_DIR = 0xdf,
        OP_STY_EXT = 0xff,
        OP_STY_IND_Y = 0xef,
        OP_TSY = 0x30,
        OP_BRCLR_IND_X = 0x1f,
    }//enum

    public partial class DisassemblerM68HC11
    {
        public static string opcodeToString(byte opcode)
        {
            switch (opcode)
            {
                case (byte)OpcodesEnum.OP_BITA_IMM:
                case (byte)OpcodesEnum.OP_BITA_DIR:
                case (byte)OpcodesEnum.OP_BITA_EXT:
                case (byte)OpcodesEnum.OP_BITA_IND_X:
                        return "BITA";
                case (byte)OpcodesEnum.OP_BITB_IMM:
                case (byte)OpcodesEnum.OP_BITB_DIR:
                case (byte)OpcodesEnum.OP_BITB_EXT:
                case (byte)OpcodesEnum.OP_BITB_IND_X:
                    return "BITB";
                case (byte)OpcodesEnum.OP_ABA: return "ABA";
                case (byte)OpcodesEnum.OP_ABX: return "ABX";
                case (byte)OpcodesEnum.OP_ADCA_DIR:
                case (byte)OpcodesEnum.OP_ADCA_IMM:
                case (byte)OpcodesEnum.OP_ADCA_EXT:
                case (byte)OpcodesEnum.OP_ADCA_IND_X:
                    return "ADCA";

                case (byte)OpcodesEnum.OP_ADCB_DIR:
                case (byte)OpcodesEnum.OP_ADCB_IMM:
                case (byte)OpcodesEnum.OP_ADCB_EXT:
                case (byte)OpcodesEnum.OP_ADCB_IND_X:
                    return "ADCB";

                case (byte)OpcodesEnum.OP_ADDA_IMM:
                case (byte)OpcodesEnum.OP_ADDA_DIR:
                case (byte)OpcodesEnum.OP_ADDA_EXT:
                case (byte)OpcodesEnum.OP_ADDA_IND_X:
                    return "ADDA";

                case (byte)OpcodesEnum.OP_ADDB_IMM:
                case (byte)OpcodesEnum.OP_ADDB_DIR:
                case (byte)OpcodesEnum.OP_ADDB_EXT:
                case (byte)OpcodesEnum.OP_ADDB_IND_X:
                    return "ADDB";

                case (byte)OpcodesEnum.OP_ADDD_IMM:
                case (byte)OpcodesEnum.OP_ADDD_DIR:
                case (byte)OpcodesEnum.OP_ADDD_EXT:
                case (byte)OpcodesEnum.OP_ADDD_IND_X:
                    return "ADDD";

                case (byte)OpcodesEnum.OP_ANDA_IMM:
                case (byte)OpcodesEnum.OP_ANDA_DIR:
                case (byte)OpcodesEnum.OP_ANDA_EXT:
                case (byte)OpcodesEnum.OP_ANDA_IND_X:
                    return "ANDA";

                case (byte)OpcodesEnum.OP_ANDB_IMM:
                case (byte)OpcodesEnum.OP_ANDB_DIR:
                case (byte)OpcodesEnum.OP_ANDB_EXT:
                case (byte)OpcodesEnum.OP_ANDB_IND_X:
                    return "ANDB";

                case (byte)OpcodesEnum.OP_ASL_EXT:
                case (byte)OpcodesEnum.OP_ASL_IND_X:
                    return "ASL";

                case (byte)OpcodesEnum.OP_ASLA: return "ASLA";
                case (byte)OpcodesEnum.OP_ASLB: return "ASLB";
                case (byte)OpcodesEnum.OP_ASLD: return "ASLD";
                case (byte)OpcodesEnum.OP_ASR_EXT:
                case (byte)OpcodesEnum.OP_ASR_IND_X:
                    return "ASR";
                case (byte)OpcodesEnum.OP_ASRA: return "ASRA";
                case (byte)OpcodesEnum.OP_ASRB: return "ASRB";
                case (byte)OpcodesEnum.OP_BCS: return "BCS";
                case (byte)OpcodesEnum.OP_BEQ: return "BEQ";
                case (byte)OpcodesEnum.OP_BGE: return "BGE";
                case (byte)OpcodesEnum.OP_BGT: return "BGT";
                case (byte)OpcodesEnum.OP_BHI: return "BHI";
                case (byte)OpcodesEnum.OP_BHS: return "BHS";
                case (byte)OpcodesEnum.OP_BLE: return "BLE";
                //	case OP_BLO: return "BLO";
                case (byte)OpcodesEnum.OP_BSR: return "BSR";
                case (byte)OpcodesEnum.OP_BLS: return "BLS";
                case (byte)OpcodesEnum.OP_BLT: return "BLT";
                case (byte)OpcodesEnum.OP_BMI: return "BMI";
                case (byte)OpcodesEnum.OP_BNE: return "BNE";
                case (byte)OpcodesEnum.OP_BPL: return "BPL";
                case (byte)OpcodesEnum.OP_BRA: return "BRA";
                case (byte)OpcodesEnum.OP_BVC: return "BVC";
                case (byte)OpcodesEnum.OP_BVS: return "BVS";
                case (byte)OpcodesEnum.OP_CMPA_IMM:
                case (byte)OpcodesEnum.OP_CMPA_DIR:
                case (byte)OpcodesEnum.OP_CMPA_EXT:
                case (byte)OpcodesEnum.OP_CMPA_IND_X:
                    return "CMPA";
                case (byte)OpcodesEnum.OP_CLR_EXT:
                case (byte)OpcodesEnum.OP_CLR_IND:
                    return "CLR";
                case (byte)OpcodesEnum.OP_CLI: return "CLI";
                case (byte)OpcodesEnum.OP_CLRA: return "CLRA";
                case (byte)OpcodesEnum.OP_CLRB: return "CLRB";
                case (byte)OpcodesEnum.OP_CLV: return "CLV";
                case (byte)OpcodesEnum.OP_CMPB_IMM:
                case (byte)OpcodesEnum.OP_CMPB_DIR:
                case (byte)OpcodesEnum.OP_CMPB_EXT:
                case (byte)OpcodesEnum.OP_CMPB_IND_X:
                    return "CMPB";
                case (byte)OpcodesEnum.OP_CPX_IMM:
                case (byte)OpcodesEnum.OP_CPX_DIR:
                case (byte)OpcodesEnum.OP_CPX_IND:
                    return "CPX";
                case (byte)OpcodesEnum.OP_DAA: return "DAA";
                case (byte)OpcodesEnum.OP_DECA: return "DECA";
                case (byte)OpcodesEnum.OP_DECB: return "DECB";
                case (byte)OpcodesEnum.OP_DES: return "DES";
                case (byte)OpcodesEnum.OP_DEX: return "DEX";
                case (byte)OpcodesEnum.OP_FDIV: return "FDIV";
                case (byte)OpcodesEnum.OP_IDIV: return "IDIV";
                case (byte)OpcodesEnum.OP_INCA: return "INCA";
                case (byte)OpcodesEnum.OP_INCB: return "INCB";
                case (byte)OpcodesEnum.OP_INS: return "INS";
                case (byte)OpcodesEnum.OP_INX: return "INX";
                case (byte)OpcodesEnum.OP_INC_EXT:
                case (byte)OpcodesEnum.OP_INC_IND_X:
                    return "INC";
                case (byte)OpcodesEnum.OP_JMP_EXT:
                case (byte)OpcodesEnum.OP_JMP_IND_X:
                    return "JMP";
                case (byte)OpcodesEnum.OP_JSR_DIR:
                case (byte)OpcodesEnum.OP_JSR_EXT:
                case (byte)OpcodesEnum.OP_JSR_IND:
                    return "JSR";
                case (byte)OpcodesEnum.OP_EORA_IMM:
                case (byte)OpcodesEnum.OP_EORA_DIR:
                case (byte)OpcodesEnum.OP_EORA_EXT:
                case (byte)OpcodesEnum.OP_EORA_IND_X:
                    return "EORA";
                case (byte)OpcodesEnum.OP_EORB_IMM:
                case (byte)OpcodesEnum.OP_EORB_DIR:
                case (byte)OpcodesEnum.OP_EORB_EXT:
                case (byte)OpcodesEnum.OP_EORB_IND_X:
                    return "EORB";
                case (byte)OpcodesEnum.OP_LDAA_IMM:
                case (byte)OpcodesEnum.OP_LDAA_DIR:
                case (byte)OpcodesEnum.OP_LDAA_EXT:
                case (byte)OpcodesEnum.OP_LDAA_IND_X:
                    return "LDAA";

                case (byte)OpcodesEnum.OP_LDAB_IMM:
                case (byte)OpcodesEnum.OP_LDAB_DIR:
                case (byte)OpcodesEnum.OP_LDAB_EXT:
                case (byte)OpcodesEnum.OP_LDAB_IND_X:
                    return "LDAB";

                case (byte)OpcodesEnum.OP_LDD_IMM:
                case (byte)OpcodesEnum.OP_LDD_DIR:
                case (byte)OpcodesEnum.OP_LDD_EXT:
                case (byte)OpcodesEnum.OP_LDD_IND_X:
                    return "LDD";

                case (byte)OpcodesEnum.OP_LDS_IMM:
                case (byte)OpcodesEnum.OP_LDS_DIR:
                case (byte)OpcodesEnum.OP_LDS_EXT:
                case (byte)OpcodesEnum.OP_LDS_IND:
                    return "LDS";

                case (byte)OpcodesEnum.OP_LDX_IMM:
                case (byte)OpcodesEnum.OP_LDX_DIR:
                case (byte)OpcodesEnum.OP_LDX_EXT:
                case (byte)OpcodesEnum.OP_LDX_IND:
                    return "LDX";

                // Duplicate opcode with ASL:
                //	case OP_LSL_EXT:
                //	case OP_LSL_IND_X:
                //		return "LSL";

                case (byte)OpcodesEnum.OP_LSR_EXT:
                case (byte)OpcodesEnum.OP_LSR_IND_X:
                    return "LSR";

                case (byte)OpcodesEnum.OP_LSRA: return "LSRA";
                case (byte)OpcodesEnum.OP_LSRB: return "LSRB";
                case (byte)OpcodesEnum.OP_LSRD: return "LSRD";
                case (byte)OpcodesEnum.OP_MUL: return "MUL";
                case (byte)OpcodesEnum.OP_ORAA_IMM:
                case (byte)OpcodesEnum.OP_ORAA_DIR:
                case (byte)OpcodesEnum.OP_ORAA_EXT:
                case (byte)OpcodesEnum.OP_ORAA_IND_X:
                    return "ORAA";
                case (byte)OpcodesEnum.OP_ORAB_IMM:
                case (byte)OpcodesEnum.OP_ORAB_DIR:
                case (byte)OpcodesEnum.OP_ORAB_EXT:
                case (byte)OpcodesEnum.OP_ORAB_IND_X:
                    return "ORAB";

                case (byte)OpcodesEnum.OP_PSHA: return "PSHA";
                case (byte)OpcodesEnum.OP_PSHB: return "PSHB";
                case (byte)OpcodesEnum.OP_PSHX: return "PSHX";
                case (byte)OpcodesEnum.OP_PULA: return "PULA";
                case (byte)OpcodesEnum.OP_PULB: return "PULB";
                case (byte)OpcodesEnum.OP_PULX: return "PULX";
                case (byte)OpcodesEnum.OP_ROLA: return "ROLA";
                case (byte)OpcodesEnum.OP_ROL_IND: return "ROL";
                case (byte)OpcodesEnum.OP_ROLB: return "ROLB";
                case (byte)OpcodesEnum.OP_RORA: return "RORA";
                case (byte)OpcodesEnum.OP_RORB: return "RORB";
                case (byte)OpcodesEnum.OP_RTS: return "RTS";

                case (byte)OpcodesEnum.OP_SBA: return "SBA";
                case (byte)OpcodesEnum.OP_SBCA_IMM:
                case (byte)OpcodesEnum.OP_SBCA_DIR:
                case (byte)OpcodesEnum.OP_SBCA_EXT:
                case (byte)OpcodesEnum.OP_SBCA_IND_X:
                    return "SBCA";
                case (byte)OpcodesEnum.OP_SBCB_IMM:
                case (byte)OpcodesEnum.OP_SBCB_DIR:
                case (byte)OpcodesEnum.OP_SBCB_EXT:
                case (byte)OpcodesEnum.OP_SBCB_IND_X:
                    return "SBCB";

                case (byte)OpcodesEnum.OP_SEC: return "SEC";
                case (byte)OpcodesEnum.OP_SEI: return "SEI";
                case (byte)OpcodesEnum.OP_SEV: return "SEV";

                case (byte)OpcodesEnum.OP_STAA_DIR:
                case (byte)OpcodesEnum.OP_STAA_EXT:
                case (byte)OpcodesEnum.OP_STAA_IND_X:
                    return "STAA";

                case (byte)OpcodesEnum.OP_STAB_DIR:
                case (byte)OpcodesEnum.OP_STAB_EXT:
                case (byte)OpcodesEnum.OP_STAB_IND_X:
                    return "STAB";

                case (byte)OpcodesEnum.OP_STD_DIR:
                case (byte)OpcodesEnum.OP_STD_EXT:
                case (byte)OpcodesEnum.OP_STD_IND_X:
                    return "STD";

                case (byte)OpcodesEnum.OP_STOP: return "STOP";
                case (byte)OpcodesEnum.OP_STS_DIR:
                //	case OP_STS_EXT: // TODO
                case (byte)OpcodesEnum.OP_STS_IND_X:
                    return "STS";

                case (byte)OpcodesEnum.OP_STX_DIR:
                case (byte)OpcodesEnum.OP_STX_EXT:
                case (byte)OpcodesEnum.OP_STX_IND:
                    return "STX";

                case (byte)OpcodesEnum.OP_SUBA_IMM:
                case (byte)OpcodesEnum.OP_SUBA_DIR:
                case (byte)OpcodesEnum.OP_SUBA_EXT:
                case (byte)OpcodesEnum.OP_SUBA_IND_X:
                    return "SUBA";
                case (byte)OpcodesEnum.OP_SUBB_IMM:
                case (byte)OpcodesEnum.OP_SUBB_DIR:
                case (byte)OpcodesEnum.OP_SUBB_EXT:
                case (byte)OpcodesEnum.OP_SUBB_IND_X:
                    return "SUBB";

                case (byte)OpcodesEnum.OP_SWI: return "SWI";
                case (byte)OpcodesEnum.OP_TAB: return "TAB";
                case (byte)OpcodesEnum.OP_TAP: return "TAP";
                case (byte)OpcodesEnum.OP_TBA: return "TBA";
                case (byte)OpcodesEnum.OP_TEST: return "TEST";
                case (byte)OpcodesEnum.OP_TPA: return "TPA";
                case (byte)OpcodesEnum.OP_TST_EXT:
                case (byte)OpcodesEnum.OP_TST_IND_X:
                    return "TST";
                case (byte)OpcodesEnum.OP_TSTA: return "TSTA";
                case (byte)OpcodesEnum.OP_TSTB: return "TSTB";
                case (byte)OpcodesEnum.OP_TSX: return "TSX";
                case (byte)OpcodesEnum.OP_TXS: return "TXS";
                case (byte)OpcodesEnum.OP_WAI: return "WAI";
                case (byte)OpcodesEnum.OP_XGDX: return "XGDX";
                case (byte)OpcodesEnum.OP_BRCLR_IND_X: return "BRCLR";
                default:
                    return "illegal opcode to disassemble";
            }//switch
        }

        private static string AddressString(ushort addr)
        {
            switch (addr)
            {
                case 0x1000: return "PORTA";
                case 0x1002: return "PIOC";
                case 0x1003: return "PORTC";
                case 0x1004: return "PORTB";
                case 0x1005: return "PORTCL";
                case 0x1006: return "DDRC";
                case 0x1008: return "PORTD";
                case 0x1009: return "DDRD";
                case 0x100a: return "PORTE";
                case 0x100b: return "CFORC";
                case 0x100c: return "OC1M";
                case 0x100d: return "OC1D";
                case 0x1020: return "TCTL1";
                case 0x1026: return "PACTL";
                case 0x1028: return "SPCR";
                case 0x1029: return "SPSR";
                case 0x102b: return "BAUD";
                case 0x102c: return "SCCR1";
                case 0x102d: return "SCCR2";
                case 0x1030: return "ADCTL";
                case 0x1031: return "ADR1";
                case 0x1032: return "ADR2";
                case 0x1033: return "ADR3";
                case 0x1034: return "ADR4";
                case 0x1039: return "OPTION";
                case 0x103a: return "COPRST";
                case 0x103b: return "PPROG";
                case 0x103c: return "HPRIO";
                case 0x103d: return "INIT";
                case 0x103e: return "TEST1";
                case 0x103f: return "CONFIG";
            }
            return "$" + addr.ToString("X4");
        }
    }//class OpcodeStringHandler
}