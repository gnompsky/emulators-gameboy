using System.Collections.ObjectModel;
using System.Diagnostics;
using GameBoyEmulator.Core.Extensions;

namespace GameBoyEmulator.Core.Components
{
    public partial class Instructions
    {
        private readonly Registers _registers;
        private readonly Memory _memory;

        private readonly IReadOnlyDictionary<byte, (string name, ExecuteDelegate execute)> _map;
        private readonly IReadOnlyDictionary<byte, (string name, ExecuteDelegate execute)> _cbMap;

        private delegate void ExecuteDelegate(ref int cycles);
        private delegate T Getter<out T>(ref int cycles);
        private delegate void Setter<in T>(T value, ref int cycles);

        public Instructions(GameBoy gameBoy, Registers registers, Memory memory)
        {
            _registers = registers;
            _memory = memory;

            _map = new ReadOnlyDictionary<byte, (string name, ExecuteDelegate execute)>(
                new Dictionary<byte, (string name, ExecuteDelegate execute)>
                {
#region 0x0_
                    { 0x00, ("NOOP", (ref int _) => { }) },
                    { 0x01, ("LD BC,d16", LdNN(SetBC, GetNextNN)) },
                    { 0x02, ("LD (BC),A", LdIndirectN(GetBC, GetA)) },
                    { 0x03, ("INC BC", IncBC) },
                    { 0x04, ("INC B", IncB) },
                    { 0x05, ("DEC B", DecB) },
                    { 0x06, ("LD B,d8", LdN(SetB, GetNextN)) },
                    { 0x07, ("RLCA", RotateLeft(GetA, SetA)) },
                    { 0x08, ("LD (a16),SP", SetIndirect(GetNextNN, (ref int cycles) => GetSP(ref cycles))) },
                    { 0x09, ("ADD HL,BC", AddNN(GetBC)) },
                    { 0x0A, ("LD A,(BC)", LdN(SetA, GetIndirect(GetBC))) },
                    { 0x0B, ("DEC BC", DecBC) },
                    { 0x0C, ("INC C", IncC) },
                    { 0x0D, ("DEC C", DecC) },
                    { 0x0E, ("LD C,d8", LdN(SetC, GetNextN)) },
                    { 0x0F, ("RRCA", RotateRight(GetA, SetA)) },

#endregion

#region 0x1_

                    {
                        0x10, ("STOP 0", (ref int _) =>
                        {
                            gameBoy.StopMode = true;
                            _memory.TimerAndDividerHandler.ClearDiv();

                            // Stop is for some reason 2 bytes wide despite the operand not being used, so we skip forward an extra byte
                            _registers.PC++;
                        })
                    },
                    { 0x11, ("LD DE,d16", LdNN(SetDE, GetNextNN)) },
                    { 0x12, ("LD (DE),A", LdIndirectN(GetDE, GetA)) },
                    { 0x13, ("INC DE", IncDE) },
                    { 0x14, ("INC D", IncD) },
                    { 0x15, ("DEC D", DecD) },
                    { 0x16, ("LD D,d8", LdN(SetD, GetNextN)) },
                    { 0x17, ("RLA", RotateLeftThroughCarry(GetA, SetA)) },
                    { 0x18, ("JR r8", JumpRelative(GetNextSignedN)) },
                    { 0x19, ("ADD HL,DE", AddNN(GetDE)) },
                    { 0x1A, ("LD A,(DE)", LdN(SetA, GetIndirect(GetDE))) },
                    { 0x1B, ("DEC DE", DecDE) },
                    { 0x1C, ("INC E", IncE) },
                    { 0x1D, ("DEC E", DecE) },
                    { 0x1E, ("LD E,d8", LdN(SetE, GetNextN)) },
                    { 0x1F, ("RRA", RotateRightThroughCarry(GetA, SetA)) },

#endregion

#region 0x2_

                    { 0x20, ("JR NZ,r8", JumpRelativeIf(Not(GetZero), GetNextSignedN)) },
                    { 0x21, ("LD HL,d16", LdNN(SetHL, GetNextNN)) },
                    {
                        0x22, ("LD (HL+),A", (ref int cycles) =>
                        {
                            LdIndirectN(GetHL, GetA)(ref cycles);
                            IncHL(ref cycles);
                        })
                    },
                    { 0x23, ("INC HL", IncHL) },
                    { 0x24, ("INC H", IncH) },
                    { 0x25, ("DEC H", DecH) },
                    { 0x26, ("LD H,d8", LdN(SetH, GetNextN)) },
                    { 0x27, ("DAA", Daa) },
                    { 0x28, ("JR Z,r8", JumpRelativeIf(GetZero, GetNextSignedN)) },
                    { 0x29, ("ADD HL,HL", AddNN(GetHL)) },
                    {
                        0x2A, ("LD A,(HL+)", (ref int cycles) =>
                        {
                            LdN(SetA, GetHLI)(ref cycles);
                            IncHL(ref cycles);
                        })
                    },
                    { 0x2B, ("DEC HL", DecHL) },
                    { 0x2C, ("INC L", IncL) },
                    { 0x2D, ("DEC L", DecL) },
                    { 0x2E, ("LD L,d8", LdN(SetL, GetNextN)) },
                    { 0x2F, ("CPL", Cpl) },

#endregion

#region 0x3_

                    { 0x30, ("JR NC,r8", JumpRelativeIf(Not(GetCarry), GetNextSignedN)) },
                    { 0x31, ("LD SP,d16", LdNN(SetSP, GetNextNN)) },
                    {
                        0x32, ("LD (HL-),A", (ref int cycles) =>
                        {
                            LdIndirectN(GetHL, GetA)(ref cycles);
                            DecHL(ref cycles);
                        })
                    },
                    { 0x33, ("INC SP", IncSP) },
                    { 0x34, ("INC (HL)", (ref int cycles) => IncN(SetHLI, GetHLI, ref cycles)) },
                    { 0x35, ("DEC (HL)", (ref int cycles) => DecN(SetHLI, GetHLI, ref cycles)) },
                    { 0x36, ("LD (HL),d8", LdN(SetHLI, GetNextN)) },
                    { 0x37, ("SCF", Scf) },
                    { 0x38, ("JR C,r8", JumpRelativeIf(GetCarry, GetNextSignedN)) },
                    { 0x39, ("ADD HL,SP", AddNN(GetSP)) },
                    {
                        0x3A, ("LD A,(HL-)", (ref int cycles) =>
                        {
                            LdN(SetA, GetHLI)(ref cycles);
                            DecHL(ref cycles);
                        })
                    },
                    { 0x3B, ("DEC SP", DecSP) },
                    { 0x3C, ("INC A", IncA) },
                    { 0x3D, ("DEC A", DecA) },
                    { 0x3E, ("LD A,d8", LdN(SetA, GetNextN)) },
                    { 0x3F, ("CCF", Ccf) },

#endregion

#region 0x4_

                    { 0x40, ("LD B,B", LdN(SetB, GetB)) },
                    { 0x41, ("LD B,C", LdN(SetB, GetC)) },
                    { 0x42, ("LD B,D", LdN(SetB, GetD)) },
                    { 0x43, ("LD B,E", LdN(SetB, GetE)) },
                    { 0x44, ("LD B,H", LdN(SetB, GetH)) },
                    { 0x45, ("LD B,L", LdN(SetB, GetL)) },
                    { 0x46, ("LD B,(HL)", LdN(SetB, GetHLI)) },
                    { 0x47, ("LD B,A", LdN(SetB, GetA)) },
                    { 0x48, ("LD C,B", LdN(SetC, GetB)) },
                    { 0x49, ("LD C,C", LdN(SetC, GetC)) },
                    { 0x4A, ("LD C,D", LdN(SetC, GetD)) },
                    { 0x4B, ("LD C,E", LdN(SetC, GetE)) },
                    { 0x4C, ("LD C,H", LdN(SetC, GetH)) },
                    { 0x4D, ("LD C,L", LdN(SetC, GetL)) },
                    { 0x4E, ("LD C,(HL)", LdN(SetC, GetHLI)) },
                    { 0x4F, ("LD C,A", LdN(SetC, GetA)) },

#endregion

#region 0x5_

                    { 0x50, ("LD D,B", LdN(SetD, GetB)) },
                    { 0x51, ("LD D,C", LdN(SetD, GetC)) },
                    { 0x52, ("LD D,D", LdN(SetD, GetD)) },
                    { 0x53, ("LD D,E", LdN(SetD, GetE)) },
                    { 0x54, ("LD D,H", LdN(SetD, GetH)) },
                    { 0x55, ("LD D,L", LdN(SetD, GetL)) },
                    { 0x56, ("LD D,(HL)", LdN(SetD, GetHLI)) },
                    { 0x57, ("LD D,A", LdN(SetD, GetA)) },
                    { 0x58, ("LD E,B", LdN(SetE, GetB)) },
                    { 0x59, ("LD E,C", LdN(SetE, GetC)) },
                    { 0x5A, ("LD E,D", LdN(SetE, GetD)) },
                    { 0x5B, ("LD E,E", LdN(SetE, GetE)) },
                    { 0x5C, ("LD E,H", LdN(SetE, GetH)) },
                    { 0x5D, ("LD E,L", LdN(SetE, GetL)) },
                    { 0x5E, ("LD E,(HL)", LdN(SetE, GetHLI)) },
                    { 0x5F, ("LD E,A", LdN(SetE, GetA)) },

#endregion

#region 0x6_

                    { 0x60, ("LD H,B", LdN(SetH, GetB)) },
                    { 0x61, ("LD H,C", LdN(SetH, GetC)) },
                    { 0x62, ("LD H,D", LdN(SetH, GetD)) },
                    { 0x63, ("LD H,E", LdN(SetH, GetE)) },
                    { 0x64, ("LD H,H", LdN(SetH, GetH)) },
                    { 0x65, ("LD H,L", LdN(SetH, GetL)) },
                    { 0x66, ("LD H,(HL)", LdN(SetH, GetHLI)) },
                    { 0x67, ("LD H,A", LdN(SetH, GetA)) },
                    { 0x68, ("LD L,B", LdN(SetL, GetB)) },
                    { 0x69, ("LD L,C", LdN(SetL, GetC)) },
                    { 0x6A, ("LD L,D", LdN(SetL, GetD)) },
                    { 0x6B, ("LD L,E", LdN(SetL, GetE)) },
                    { 0x6C, ("LD L,H", LdN(SetL, GetH)) },
                    { 0x6D, ("LD L,L", LdN(SetL, GetL)) },
                    { 0x6E, ("LD L,(HL)", LdN(SetL, GetHLI)) },
                    { 0x6F, ("LD L,A", LdN(SetL, GetA)) },

#endregion

#region 0x7_

                    { 0x70, ("LD (HL),B", LdN(SetHLI, GetB)) },
                    { 0x71, ("LD (HL),C", LdN(SetHLI, GetC)) },
                    { 0x72, ("LD (HL),D", LdN(SetHLI, GetD)) },
                    { 0x73, ("LD (HL),E", LdN(SetHLI, GetE)) },
                    { 0x74, ("LD (HL),H", LdN(SetHLI, GetH)) },
                    { 0x75, ("LD (HL),L", LdN(SetHLI, GetL)) },
                    { 0x76, ("HALT", (ref int _) =>
                    {
                        gameBoy.HaltMode = true;
                    }) },
                    { 0x77, ("LD (HL),A", LdN(SetHLI, GetA)) },
                    { 0x78, ("LD A,B", LdN(SetA, GetB)) },
                    { 0x79, ("LD A,C", LdN(SetA, GetC)) },
                    { 0x7A, ("LD A,D", LdN(SetA, GetD)) },
                    { 0x7B, ("LD A,E", LdN(SetA, GetE)) },
                    { 0x7C, ("LD A,H", LdN(SetA, GetH)) },
                    { 0x7D, ("LD A,L", LdN(SetA, GetL)) },
                    { 0x7E, ("LD A,(HL)", LdN(SetA, GetHLI)) },
                    { 0x7F, ("LD A,A", LdN(SetA, GetA)) },

#endregion

#region 0x8_

                    { 0x80, ("ADD A,B", AddN(GetB)) },
                    { 0x81, ("ADD A,C", AddN(GetC)) },
                    { 0x82, ("ADD A,D", AddN(GetD)) },
                    { 0x83, ("ADD A,E", AddN(GetE)) },
                    { 0x84, ("ADD A,H", AddN(GetH)) },
                    { 0x85, ("ADD A,L", AddN(GetL)) },
                    { 0x86, ("ADD A,(HL)", AddN(GetHLI)) },
                    { 0x87, ("ADD A,A", AddN(GetA)) },
                    { 0x88, ("ADC A,B", AdcN(GetB)) },
                    { 0x89, ("ADC A,C", AdcN(GetC)) },
                    { 0x8A, ("ADC A,D", AdcN(GetD)) },
                    { 0x8B, ("ADC A,E", AdcN(GetE)) },
                    { 0x8C, ("ADC A,H", AdcN(GetH)) },
                    { 0x8D, ("ADC A,L", AdcN(GetL)) },
                    { 0x8E, ("ADC A,(HL)", AdcN(GetHLI)) },
                    { 0x8F, ("ADC A,A", AdcN(GetA)) },

#endregion

#region 0x9_

                    { 0x90, ("SUB B", SubN(GetB)) },
                    { 0x91, ("SUB C", SubN(GetC)) },
                    { 0x92, ("SUB D", SubN(GetD)) },
                    { 0x93, ("SUB E", SubN(GetE)) },
                    { 0x94, ("SUB H", SubN(GetH)) },
                    { 0x95, ("SUB L", SubN(GetL)) },
                    { 0x96, ("SUB (HL)", SubN(GetHLI)) },
                    { 0x97, ("SUB A", SubN(GetA)) },
                    { 0x98, ("SBC A,B", SbcN(GetB)) },
                    { 0x99, ("SBC A,C", SbcN(GetC)) },
                    { 0x9A, ("SBC A,D", SbcN(GetD)) },
                    { 0x9B, ("SBC A,E", SbcN(GetE)) },
                    { 0x9C, ("SBC A,H", SbcN(GetH)) },
                    { 0x9D, ("SBC A,L", SbcN(GetL)) },
                    { 0x9E, ("SBC A,(HL)", SbcN(GetHLI)) },
                    { 0x9F, ("SBC A,A", SbcN(GetA)) },

#endregion

#region 0xA_

                    { 0xA0, ("AND B", And(GetB)) },
                    { 0xA1, ("AND C", And(GetC)) },
                    { 0xA2, ("AND D", And(GetD)) },
                    { 0xA3, ("AND E", And(GetE)) },
                    { 0xA4, ("AND H", And(GetH)) },
                    { 0xA5, ("AND L", And(GetL)) },
                    { 0xA6, ("AND (HL)", And(GetHLI)) },
                    { 0xA7, ("AND A", And(GetA)) },
                    { 0xA8, ("XOR B", Xor(GetB)) },
                    { 0xA9, ("XOR C", Xor(GetC)) },
                    { 0xAA, ("XOR D", Xor(GetD)) },
                    { 0xAB, ("XOR E", Xor(GetE)) },
                    { 0xAC, ("XOR H", Xor(GetH)) },
                    { 0xAD, ("XOR L", Xor(GetL)) },
                    { 0xAE, ("XOR (HL)", Xor(GetHLI)) },
                    { 0xAF, ("XOR A", Xor(GetA)) },

#endregion

#region 0xB_

                    { 0xB0, ("OR B", Or(GetB)) },
                    { 0xB1, ("OR C", Or(GetC)) },
                    { 0xB2, ("OR D", Or(GetD)) },
                    { 0xB3, ("OR E", Or(GetE)) },
                    { 0xB4, ("OR H", Or(GetH)) },
                    { 0xB5, ("OR L", Or(GetL)) },
                    { 0xB6, ("OR (HL)", Or(GetHLI)) },
                    { 0xB7, ("OR A", Or(GetA)) },
                    { 0xB8, ("CP B", Compare(GetB)) },
                    { 0xB9, ("CP C", Compare(GetC)) },
                    { 0xBA, ("CP D", Compare(GetD)) },
                    { 0xBB, ("CP E", Compare(GetE)) },
                    { 0xBC, ("CP H", Compare(GetH)) },
                    { 0xBD, ("CP L", Compare(GetL)) },
                    { 0xBE, ("CP (HL)", Compare(GetHLI)) },
                    { 0xBF, ("CP A", Compare(GetA)) },

#endregion

#region 0xC_

                    { 0xC0, ("RET NZ", ReturnIf(Not(GetZero))) },
                    { 0xC1, ("POP BC", Pop(SetBC)) },
                    { 0xC2, ("JP NZ,a16", JumpIf(Not(GetZero), GetNextNN)) },
                    { 0xC3, ("JP a16", Jump(GetNextNN)) },
                    { 0xC4, ("CALL NZ,a16", CallIf(Not(GetZero), GetNextNN)) },
                    { 0xC5, ("PUSH BC", Push(GetBC)) },
                    { 0xC6, ("ADD A,d8", AddN(GetNextN)) },
                    { 0xC7, ("RST 00H", Call((ref int _) => 0x0000)) },
                    { 0xC8, ("RET Z", ReturnIf(GetZero)) },
                    { 0xC9, ("RET", ReturnIf((ref int _) => true)) },
                    { 0xCA, ("JP Z,a16", JumpIf(GetZero, GetNextNN)) },
                    { 0xCB, ("PREFIX CB", (ref int cycles) => ExecuteFrom(_cbMap!, GetNextN(ref cycles), ref cycles)) },
                    { 0xCC, ("CALL Z,a16", CallIf(GetZero, GetNextNN)) },
                    { 0xCD, ("CALL a16", Call(GetNextNN)) },
                    { 0xCE, ("ADC A,d8", AdcN(GetNextN)) },
                    { 0xCF, ("RST 08H", Call((ref int _) => 0x0008)) },

#endregion

#region 0xD_

                    { 0xD0, ("RET NC", ReturnIf(Not(GetCarry))) },
                    { 0xD1, ("POP DE", Pop(SetDE)) },
                    { 0xD2, ("JP NC,a16", JumpIf(Not(GetCarry), GetNextNN)) },
                    // 0xD3 - Non-existent
                    { 0xD4, ("CALL NC,a16", CallIf(Not(GetCarry), GetNextNN)) },
                    { 0xD5, ("PUSH DE", Push(GetDE)) },
                    { 0xD6, ("SUB d8", SubN(GetNextN)) },
                    { 0xD7, ("RST 10H", Call((ref int _) => 0x0010)) },
                    { 0xD8, ("RET C", ReturnIf(GetCarry)) },
                    {
                        0xD9, ("RETI", (ref int cycles) =>
                        {
                            ReturnIf((ref int _) => true)(ref cycles);
                            _registers.IME = true;
                        })
                    },
                    { 0xDA, ("JP C,a16", JumpIf(GetCarry, GetNextNN)) },
                    // 0xDB - Non-existent
                    { 0xDC, ("CALL C,a16", CallIf(GetCarry, GetNextNN)) },
                    // 0xDD - Non-existent
                    { 0xDE, ("SBC A,d8", SbcN(GetNextN)) },
                    { 0xDF, ("RST 18H", Call((ref int _) => 0x0018)) },

#endregion

#region 0xE_

                    { 0xE0, ("LDH (a8),A", LdIndirectN(GetNextN, GetA)) },
                    { 0xE1, ("POP HL", Pop(SetHL)) },
                    { 0xE2, ("LD (C),A", LdIndirectN(GetC, GetA)) },
                    // 0xE3 - Non-existent
                    // 0xE4 - Non-existent
                    { 0xE5, ("PUSH HL", Push(GetHL)) },
                    { 0xE6, ("AND d8", And(GetNextN)) },
                    { 0xE7, ("RST 20H", Call((ref int _) => 0x0020)) },
                    { 0xE8, ("ADD SP,r8", (ref int cycles) =>
                    {
                        AddNN(SetSP, GetSP, GetNextN)(ref cycles);
                        _registers.IsZero = true;
                    }) },
                    { 0xE9, ("JP (HL)", Jump(GetHL)) },
                    { 0xEA, ("LD (a16),A", LdIndirectN(GetNextNN, GetA)) },
                    // 0xEB - Non-existent
                    // 0xEC - Non-existent
                    // 0xED - Non-existent
                    { 0xEE, ("XOR d8", Xor(GetNextN)) },
                    { 0xEF, ("RST 28H", Call((ref int _) => 0x0028)) },

#endregion

#region 0xF_

                    {
                        0xF0,
                        ("LDH A,(a8)", LdN(SetA, GetIndirect((ref int cycles) => (ushort)(0xFF00 + GetNextN(ref cycles)))))
                    },
                    { 0xF1, ("POP AF", Pop(SetAF)) },
                    {
                        0xF2,
                        ("LD A,(C)", LdN(SetA, GetIndirect((ref int cycles) => (ushort)(0xFF00 + GetC(ref cycles)))))
                    },
                    { 0xF3, ("DI", (ref int _) => _registers.IME = false) },
                    // 0xF4 - Non-existent
                    { 0xF5, ("PUSH AF", Push(GetAF)) },
                    { 0xF6, ("OR d8", Or(GetNextN)) },
                    { 0xF7, ("RST 30H", Call((ref int _) => 0x0030)) },
                    { 0xF8, ("LD HL,SP+r8", LdNN(SetHL, (ref int cycles) => (ushort)(GetSP(ref cycles) + GetNextNN(ref cycles)))) },
                    { 0xF9, ("LD SP,HL", LdNN(SetSP, GetHL)) },
                    { 0xFA, ("LD A,(a16)", LdN(SetA, GetIndirect(GetNextNN))) },
                    { 0xFB, ("EI", (ref int _) => _registers.IME = true) },
                    // 0xFC - Non-existent
                    // 0xFD - Non-existent
                    { 0xFE, ("CP d8", Compare(GetNextN)) },
                    { 0xFF, ("RST 38H", Call((ref int _) => 0x0038)) },

#endregion
                }
            );

            _cbMap = new ReadOnlyDictionary<byte, (string name, ExecuteDelegate execute)>(
                new Dictionary<byte, (string name, ExecuteDelegate execute)>
                {
#region 0x0_

                    { 0x00, ("RLC B", RotateLeft(GetB, SetB)) },
                    { 0x01, ("RLC C", RotateLeft(GetC, SetC)) },
                    { 0x02, ("RLC D", RotateLeft(GetD, SetD)) },
                    { 0x03, ("RLC E", RotateLeft(GetE, SetE)) },
                    { 0x04, ("RLC H", RotateLeft(GetH, SetH)) },
                    { 0x05, ("RLC L", RotateLeft(GetL, SetL)) },
                    { 0x06, ("RLC (HL)", RotateLeft(GetHLI, SetHLI)) },
                    { 0x07, ("RLC A", RotateLeft(GetA, SetA)) },
                    { 0x08, ("RRC B", RotateRight(GetB, SetB)) },
                    { 0x09, ("RRC C", RotateRight(GetC, SetC)) },
                    { 0x0A, ("RRC D", RotateRight(GetD, SetD)) },
                    { 0x0B, ("RRC E", RotateRight(GetE, SetE)) },
                    { 0x0C, ("RRC H", RotateRight(GetH, SetH)) },
                    { 0x0D, ("RRC L", RotateRight(GetL, SetL)) },
                    { 0x0E, ("RRC (HL)", RotateRight(GetHLI, SetHLI)) },
                    { 0x0F, ("RRC A", RotateRight(GetA, SetA)) },

#endregion

#region 0x1_

                    { 0x10, ("RL B", RotateLeftThroughCarry(GetB, SetB)) },
                    { 0x11, ("RL C", RotateLeftThroughCarry(GetC, SetC)) },
                    { 0x12, ("RL D", RotateLeftThroughCarry(GetD, SetD)) },
                    { 0x13, ("RL E", RotateLeftThroughCarry(GetE, SetE)) },
                    { 0x14, ("RL H", RotateLeftThroughCarry(GetH, SetH)) },
                    { 0x15, ("RL L", RotateLeftThroughCarry(GetL, SetL)) },
                    { 0x16, ("RL (HL)", RotateLeftThroughCarry(GetHLI, SetHLI)) },
                    { 0x17, ("RL A", RotateLeftThroughCarry(GetA, SetA)) },
                    { 0x18, ("RR B", RotateRightThroughCarry(GetB, SetB)) },
                    { 0x19, ("RR C", RotateRightThroughCarry(GetC, SetC)) },
                    { 0x1A, ("RR D", RotateRightThroughCarry(GetD, SetD)) },
                    { 0x1B, ("RR E", RotateRightThroughCarry(GetE, SetE)) },
                    { 0x1C, ("RR H", RotateRightThroughCarry(GetH, SetH)) },
                    { 0x1D, ("RR L", RotateRightThroughCarry(GetL, SetL)) },
                    { 0x1E, ("RR (HL)", RotateRightThroughCarry(GetHLI, SetHLI)) },
                    { 0x1F, ("RR A", RotateRightThroughCarry(GetA, SetA)) },

#endregion

#region 0x2_

                    { 0x20, ("SLA B", Shift(SetB, GetB, true)) },
                    { 0x21, ("SLA C", Shift(SetC, GetC, true)) },
                    { 0x22, ("SLA D", Shift(SetD, GetD, true)) },
                    { 0x23, ("SLA E", Shift(SetE, GetE, true)) },
                    { 0x24, ("SLA H", Shift(SetH, GetH, true)) },
                    { 0x25, ("SLA L", Shift(SetL, GetL, true)) },
                    { 0x26, ("SLA (HL)", Shift(SetHLI, GetHLI, true)) },
                    { 0x27, ("SLA A", Shift(SetA, GetA, true)) },
                    { 0x28, ("SRA B", Shift(SetB, GetB, false)) },
                    { 0x29, ("SRA C", Shift(SetC, GetC, false)) },
                    { 0x2A, ("SRA D", Shift(SetD, GetD, false)) },
                    { 0x2B, ("SRA E", Shift(SetE, GetE, false)) },
                    { 0x2C, ("SRA H", Shift(SetH, GetH, false)) },
                    { 0x2D, ("SRA L", Shift(SetL, GetL, false)) },
                    { 0x2E, ("SRA (HL)", Shift(SetHLI, GetHLI, false)) },
                    { 0x2F, ("SRA A", Shift(SetA, GetA, false)) },

#endregion

#region 0x3_

                    { 0x30, ("SWAP B", Swap(SetB, GetB)) },
                    { 0x31, ("SWAP C", Swap(SetC, GetC)) },
                    { 0x32, ("SWAP D", Swap(SetD, GetD)) },
                    { 0x33, ("SWAP E", Swap(SetE, GetE)) },
                    { 0x34, ("SWAP H", Swap(SetH, GetH)) },
                    { 0x35, ("SWAP L", Swap(SetL, GetL)) },
                    { 0x36, ("SWAP (HL)", Swap(SetHLI, GetHLI)) },
                    { 0x37, ("SWAP A", Swap(SetA, GetA)) },
                    { 0x38, ("SRL B", Shift(SetB, GetB, false, true)) },
                    { 0x39, ("SRL C", Shift(SetC, GetC, false, true)) },
                    { 0x3A, ("SRL D", Shift(SetD, GetD, false, true)) },
                    { 0x3B, ("SRL E", Shift(SetE, GetE, false, true)) },
                    { 0x3C, ("SRL H", Shift(SetH, GetH, false, true)) },
                    { 0x3D, ("SRL L", Shift(SetL, GetL, false, true)) },
                    { 0x3E, ("SRL (HL)", Shift(SetHLI, GetHLI, false, true)) },
                    { 0x3F, ("SRL A", Shift(SetA, GetA, false, true)) },

#endregion

#region 0x4_

                    { 0x40, ("BIT 0,B", Bit(0, GetB)) },
                    { 0x41, ("BIT 0,C", Bit(0, GetC)) },
                    { 0x42, ("BIT 0,D", Bit(0, GetD)) },
                    { 0x43, ("BIT 0,E", Bit(0, GetE)) },
                    { 0x44, ("BIT 0,H", Bit(0, GetH)) },
                    { 0x45, ("BIT 0,L", Bit(0, GetL)) },
                    { 0x46, ("BIT 0,(HL)", Bit(0, GetHLI)) },
                    { 0x47, ("BIT 0,A", Bit(0, GetA)) },
                    { 0x48, ("BIT 1,B", Bit(1, GetB)) },
                    { 0x49, ("BIT 1,C", Bit(1, GetC)) },
                    { 0x4A, ("BIT 1,D", Bit(1, GetD)) },
                    { 0x4B, ("BIT 1,E", Bit(1, GetE)) },
                    { 0x4C, ("BIT 1,H", Bit(1, GetH)) },
                    { 0x4D, ("BIT 1,L", Bit(1, GetL)) },
                    { 0x4E, ("BIT 1,(HL)", Bit(1, GetHLI)) },
                    { 0x4F, ("BIT 1,A", Bit(1, GetA)) },

#endregion

#region 0x5_

                    { 0x50, ("BIT 2,B", Bit(2, GetB)) },
                    { 0x51, ("BIT 2,C", Bit(2, GetC)) },
                    { 0x52, ("BIT 2,D", Bit(2, GetD)) },
                    { 0x53, ("BIT 2,E", Bit(2, GetE)) },
                    { 0x54, ("BIT 2,H", Bit(2, GetH)) },
                    { 0x55, ("BIT 2,L", Bit(2, GetL)) },
                    { 0x56, ("BIT 2,(HL)", Bit(2, GetHLI)) },
                    { 0x57, ("BIT 2,A", Bit(2, GetA)) },
                    { 0x58, ("BIT 3,B", Bit(3, GetB)) },
                    { 0x59, ("BIT 3,C", Bit(3, GetC)) },
                    { 0x5A, ("BIT 3,D", Bit(3, GetD)) },
                    { 0x5B, ("BIT 3,E", Bit(3, GetE)) },
                    { 0x5C, ("BIT 3,H", Bit(3, GetH)) },
                    { 0x5D, ("BIT 3,L", Bit(3, GetL)) },
                    { 0x5E, ("BIT 3,(HL)", Bit(3, GetHLI)) },
                    { 0x5F, ("BIT 3,A", Bit(3, GetA)) },

#endregion

#region 0x6_

                    { 0x60, ("BIT 4,B", Bit(4, GetB)) },
                    { 0x61, ("BIT 4,C", Bit(4, GetC)) },
                    { 0x62, ("BIT 4,D", Bit(4, GetD)) },
                    { 0x63, ("BIT 4,E", Bit(4, GetE)) },
                    { 0x64, ("BIT 4,H", Bit(4, GetH)) },
                    { 0x65, ("BIT 4,L", Bit(4, GetL)) },
                    { 0x66, ("BIT 4,(HL)", Bit(4, GetHLI)) },
                    { 0x67, ("BIT 4,A", Bit(4, GetA)) },
                    { 0x68, ("BIT 5,B", Bit(5, GetB)) },
                    { 0x69, ("BIT 5,C", Bit(5, GetC)) },
                    { 0x6A, ("BIT 5,D", Bit(5, GetD)) },
                    { 0x6B, ("BIT 5,E", Bit(5, GetE)) },
                    { 0x6C, ("BIT 5,H", Bit(5, GetH)) },
                    { 0x6D, ("BIT 5,L", Bit(5, GetL)) },
                    { 0x6E, ("BIT 5,(HL)", Bit(5, GetHLI)) },
                    { 0x6F, ("BIT 5,A", Bit(5, GetA)) },

#endregion

#region 0x7_

                    { 0x70, ("BIT 6,B", Bit(6, GetB)) },
                    { 0x71, ("BIT 6,C", Bit(6, GetC)) },
                    { 0x72, ("BIT 6,D", Bit(6, GetD)) },
                    { 0x73, ("BIT 6,E", Bit(6, GetE)) },
                    { 0x74, ("BIT 6,H", Bit(6, GetH)) },
                    { 0x75, ("BIT 6,L", Bit(6, GetL)) },
                    { 0x76, ("BIT 6,(HL)", Bit(6, GetHLI)) },
                    { 0x77, ("BIT 6,A", Bit(6, GetA)) },
                    { 0x78, ("BIT 7,B", Bit(7, GetB)) },
                    { 0x79, ("BIT 7,C", Bit(7, GetC)) },
                    { 0x7A, ("BIT 7,D", Bit(7, GetD)) },
                    { 0x7B, ("BIT 7,E", Bit(7, GetE)) },
                    { 0x7C, ("BIT 7,H", Bit(7, GetH)) },
                    { 0x7D, ("BIT 7,L", Bit(7, GetL)) },
                    { 0x7E, ("BIT 7,(HL)", Bit(7, GetHLI)) },
                    { 0x7F, ("BIT 7,A", Bit(7, GetA)) },

#endregion

#region 0x8_

                    { 0x80, ("RES 0,B", Unset(0, GetB, SetB)) },
                    { 0x81, ("RES 0,C", Unset(0, GetC, SetC)) },
                    { 0x82, ("RES 0,D", Unset(0, GetD, SetD)) },
                    { 0x83, ("RES 0,E", Unset(0, GetE, SetE)) },
                    { 0x84, ("RES 0,H", Unset(0, GetH, SetH)) },
                    { 0x85, ("RES 0,L", Unset(0, GetL, SetL)) },
                    { 0x86, ("RES 0,(HL)", Unset(0, GetHLI, SetHLI)) },
                    { 0x87, ("RES 0,A", Unset(0, GetA, SetA)) },
                    { 0x88, ("RES 1,B", Unset(1, GetB, SetB)) },
                    { 0x89, ("RES 1,C", Unset(1, GetC, SetC)) },
                    { 0x8A, ("RES 1,D", Unset(1, GetD, SetD)) },
                    { 0x8B, ("RES 1,E", Unset(1, GetE, SetE)) },
                    { 0x8C, ("RES 1,H", Unset(1, GetH, SetH)) },
                    { 0x8D, ("RES 1,L", Unset(1, GetL, SetL)) },
                    { 0x8E, ("RES 1,(HL)", Unset(1, GetHLI, SetHLI)) },
                    { 0x8F, ("RES 1,A", Unset(1, GetA, SetA)) },

#endregion

#region 0x9_

                    { 0x90, ("RES 2,B", Unset(2, GetB, SetB)) },
                    { 0x91, ("RES 2,C", Unset(2, GetC, SetC)) },
                    { 0x92, ("RES 2,D", Unset(2, GetD, SetD)) },
                    { 0x93, ("RES 2,E", Unset(2, GetE, SetE)) },
                    { 0x94, ("RES 2,H", Unset(2, GetH, SetH)) },
                    { 0x95, ("RES 2,L", Unset(2, GetL, SetL)) },
                    { 0x96, ("RES 2,(HL)", Unset(2, GetHLI, SetHLI)) },
                    { 0x97, ("RES 2,A", Unset(2, GetA, SetA)) },
                    { 0x98, ("RES 3,B", Unset(3, GetB, SetB)) },
                    { 0x99, ("RES 3,C", Unset(3, GetC, SetC)) },
                    { 0x9A, ("RES 3,D", Unset(3, GetD, SetD)) },
                    { 0x9B, ("RES 3,E", Unset(3, GetE, SetE)) },
                    { 0x9C, ("RES 3,H", Unset(3, GetH, SetH)) },
                    { 0x9D, ("RES 3,L", Unset(3, GetL, SetL)) },
                    { 0x9E, ("RES 3,(HL)", Unset(3, GetHLI, SetHLI)) },
                    { 0x9F, ("RES 3,A", Unset(3, GetA, SetA)) },

#endregion

#region 0xA_

                    { 0xA0, ("RES 4,B", Unset(4, GetB, SetB)) },
                    { 0xA1, ("RES 4,C", Unset(4, GetC, SetC)) },
                    { 0xA2, ("RES 4,D", Unset(4, GetD, SetD)) },
                    { 0xA3, ("RES 4,E", Unset(4, GetE, SetE)) },
                    { 0xA4, ("RES 4,H", Unset(4, GetH, SetH)) },
                    { 0xA5, ("RES 4,L", Unset(4, GetL, SetL)) },
                    { 0xA6, ("RES 4,(HL)", Unset(4, GetHLI, SetHLI)) },
                    { 0xA7, ("RES 4,A", Unset(4, GetA, SetA)) },
                    { 0xA8, ("RES 5,B", Unset(5, GetB, SetB)) },
                    { 0xA9, ("RES 5,C", Unset(5, GetC, SetC)) },
                    { 0xAA, ("RES 5,D", Unset(5, GetD, SetD)) },
                    { 0xAB, ("RES 5,E", Unset(5, GetE, SetE)) },
                    { 0xAC, ("RES 5,H", Unset(5, GetH, SetH)) },
                    { 0xAD, ("RES 5,L", Unset(5, GetL, SetL)) },
                    { 0xAE, ("RES 5,(HL)", Unset(5, GetHLI, SetHLI)) },
                    { 0xAF, ("RES 5,A", Unset(5, GetA, SetA)) },

#endregion

#region 0xB_

                    { 0xB0, ("RES 6,B", Unset(6, GetB, SetB)) },
                    { 0xB1, ("RES 6,C", Unset(6, GetC, SetC)) },
                    { 0xB2, ("RES 6,D", Unset(6, GetD, SetD)) },
                    { 0xB3, ("RES 6,E", Unset(6, GetE, SetE)) },
                    { 0xB4, ("RES 6,H", Unset(6, GetH, SetH)) },
                    { 0xB5, ("RES 6,L", Unset(6, GetL, SetL)) },
                    { 0xB6, ("RES 6,(HL)", Unset(6, GetHLI, SetHLI)) },
                    { 0xB7, ("RES 6,A", Unset(6, GetA, SetA)) },
                    { 0xB8, ("RES 7,B", Unset(7, GetB, SetB)) },
                    { 0xB9, ("RES 7,C", Unset(7, GetC, SetC)) },
                    { 0xBA, ("RES 7,D", Unset(7, GetD, SetD)) },
                    { 0xBB, ("RES 7,E", Unset(7, GetE, SetE)) },
                    { 0xBC, ("RES 7,H", Unset(7, GetH, SetH)) },
                    { 0xBD, ("RES 7,L", Unset(7, GetL, SetL)) },
                    { 0xBE, ("RES 7,(HL)", Unset(7, GetHLI, SetHLI)) },
                    { 0xBF, ("RES 7,A", Unset(7, GetA, SetA)) },

#endregion

#region 0xC_

                    { 0xC0, ("SET 0,B", Set(0, GetB, SetB)) },
                    { 0xC1, ("SET 0,C", Set(0, GetC, SetC)) },
                    { 0xC2, ("SET 0,D", Set(0, GetD, SetD)) },
                    { 0xC3, ("SET 0,E", Set(0, GetE, SetE)) },
                    { 0xC4, ("SET 0,H", Set(0, GetH, SetH)) },
                    { 0xC5, ("SET 0,L", Set(0, GetL, SetL)) },
                    { 0xC6, ("SET 0,(HL)", Set(0, GetHLI, SetHLI)) },
                    { 0xC7, ("SET 0,A", Set(0, GetA, SetA)) },
                    { 0xC8, ("SET 1,B", Set(1, GetB, SetB)) },
                    { 0xC9, ("SET 1,C", Set(1, GetC, SetC)) },
                    { 0xCA, ("SET 1,D", Set(1, GetD, SetD)) },
                    { 0xCB, ("SET 1,E", Set(1, GetE, SetE)) },
                    { 0xCC, ("SET 1,H", Set(1, GetH, SetH)) },
                    { 0xCD, ("SET 1,L", Set(1, GetL, SetL)) },
                    { 0xCE, ("SET 1,(HL)", Set(1, GetHLI, SetHLI)) },
                    { 0xCF, ("SET 1,A", Set(1, GetA, SetA)) },

#endregion

#region 0xD_

                    { 0xD0, ("SET 2,B", Set(2, GetB, SetB)) },
                    { 0xD1, ("SET 2,C", Set(2, GetC, SetC)) },
                    { 0xD2, ("SET 2,D", Set(2, GetD, SetD)) },
                    { 0xD3, ("SET 2,E", Set(2, GetE, SetE)) },
                    { 0xD4, ("SET 2,H", Set(2, GetH, SetH)) },
                    { 0xD5, ("SET 2,L", Set(2, GetL, SetL)) },
                    { 0xD6, ("SET 2,(HL)", Set(2, GetHLI, SetHLI)) },
                    { 0xD7, ("SET 2,A", Set(2, GetA, SetA)) },
                    { 0xD8, ("SET 3,B", Set(3, GetB, SetB)) },
                    { 0xD9, ("SET 3,C", Set(3, GetC, SetC)) },
                    { 0xDA, ("SET 3,D", Set(3, GetD, SetD)) },
                    { 0xDB, ("SET 3,E", Set(3, GetE, SetE)) },
                    { 0xDC, ("SET 3,H", Set(3, GetH, SetH)) },
                    { 0xDD, ("SET 3,L", Set(3, GetL, SetL)) },
                    { 0xDE, ("SET 3,(HL)", Set(3, GetHLI, SetHLI)) },
                    { 0xDF, ("SET 3,A", Set(3, GetA, SetA)) },

#endregion

#region 0xE_

                    { 0xE0, ("SET 4,B", Set(4, GetB, SetB)) },
                    { 0xE1, ("SET 4,C", Set(4, GetC, SetC)) },
                    { 0xE2, ("SET 4,D", Set(4, GetD, SetD)) },
                    { 0xE3, ("SET 4,E", Set(4, GetE, SetE)) },
                    { 0xE4, ("SET 4,H", Set(4, GetH, SetH)) },
                    { 0xE5, ("SET 4,L", Set(4, GetL, SetL)) },
                    { 0xE6, ("SET 4,(HL)", Set(4, GetHLI, SetHLI)) },
                    { 0xE7, ("SET 4,A", Set(4, GetA, SetA)) },
                    { 0xE8, ("SET 5,B", Set(5, GetB, SetB)) },
                    { 0xE9, ("SET 5,C", Set(5, GetC, SetC)) },
                    { 0xEA, ("SET 5,D", Set(5, GetD, SetD)) },
                    { 0xEB, ("SET 5,E", Set(5, GetE, SetE)) },
                    { 0xEC, ("SET 5,H", Set(5, GetH, SetH)) },
                    { 0xED, ("SET 5,L", Set(5, GetL, SetL)) },
                    { 0xEE, ("SET 5,(HL)", Set(5, GetHLI, SetHLI)) },
                    { 0xEF, ("SET 5,A", Set(5, GetA, SetA)) },

#endregion

#region 0xF_

                    { 0xF0, ("SET 6,B", Set(6, GetB, SetB)) },
                    { 0xF1, ("SET 6,C", Set(6, GetC, SetC)) },
                    { 0xF2, ("SET 6,D", Set(6, GetD, SetD)) },
                    { 0xF3, ("SET 6,E", Set(6, GetE, SetE)) },
                    { 0xF4, ("SET 6,H", Set(6, GetH, SetH)) },
                    { 0xF5, ("SET 6,L", Set(6, GetL, SetL)) },
                    { 0xF6, ("SET 6,(HL)", Set(6, GetHLI, SetHLI)) },
                    { 0xF7, ("SET 6,A", Set(6, GetA, SetA)) },
                    { 0xF8, ("SET 7,B", Set(7, GetB, SetB)) },
                    { 0xF9, ("SET 7,C", Set(7, GetC, SetC)) },
                    { 0xFA, ("SET 7,D", Set(7, GetD, SetD)) },
                    { 0xFB, ("SET 7,E", Set(7, GetE, SetE)) },
                    { 0xFC, ("SET 7,H", Set(7, GetH, SetH)) },
                    { 0xFD, ("SET 7,L", Set(7, GetL, SetL)) },
                    { 0xFE, ("SET 7,(HL)", Set(7, GetHLI, SetHLI)) },
                    { 0xFF, ("SET 7,A", Set(7, GetA, SetA)) },

#endregion
                }
            );
        }

        public void ExecuteNext(StreamWriter logStream, ref int cycles)
        {
            //if (_registers.PC == 0xC246) Debugger.Launch();

            var dummy = 0;
            var parts = new[]
            {
                $"A:{_registers.A.ToHex()}",
                $"F:{_registers.F.ToHex()}",
                $"B:{_registers.B.ToHex()}",
                $"C:{_registers.C.ToHex()}",
                $"D:{_registers.D.ToHex()}",
                $"E:{_registers.E.ToHex()}",
                $"H:{_registers.H.ToHex()}",
                $"L:{_registers.L.ToHex()}",
                $"SP:{_registers.SP.ToHex()}",
                $"PC:{_registers.PC.ToHex()}",
                $"PCMEM:{_memory.GetN(_registers.PC, ref dummy).ToHex()},{_memory.GetN((ushort)(_registers.PC+1), ref dummy).ToHex()},{_memory.GetN((ushort)(_registers.PC+2), ref dummy).ToHex()},{_memory.GetN((ushort)(_registers.PC+3), ref dummy).ToHex()}",
            };
            logStream.WriteLine(string.Join(' ', parts));

            var instruction = GetNextN(ref cycles);
            ExecuteFrom(_map, instruction, ref cycles);
        }

        private static void ExecuteFrom(IReadOnlyDictionary<byte, (string name, ExecuteDelegate execute)> map, byte instruction, ref int cycles)
        {
            var (instructionName, execute) = map[instruction];
            execute(ref cycles);
        }
        
        private byte GetNextN(ref int cycles) => _memory.GetN(_registers.PC++, ref cycles);
        private sbyte GetNextSignedN(ref int cycles) => (sbyte)_memory.GetN(_registers.PC++, ref cycles); // TODO: This almost certainly won't work. How do we handle this?
        private ushort GetNextNN(ref int cycles) => Maths.CombineBytes(GetNextN(ref cycles), GetNextN(ref cycles));
    }
}