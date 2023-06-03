using System.Collections.ObjectModel;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core
{
    public static partial class Instructions
    {
        public static event Events.InstructionExecutingHandler? InstructionExecuting;

        public static void Execute(byte instruction) => ExecuteFrom(Map, instruction);

        private static void ExecuteFrom(IReadOnlyDictionary<byte, (string name, Action execute)> map, byte instruction)
        {
            var (instructionName, execute) = map[instruction];

            InstructionExecuting?.Invoke($"0x{Convert.ToString(Registers.PC-1, 16).PadLeft(4, '0').ToUpperInvariant()} - {instructionName}");
            execute();
        }

        private static readonly Action NotImpl = () =>
        {
            string ToHex(byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";

            // We're dead in the water here so it's ok to fuck with the PC
            // We want to print out the current instruction and next 2 bytes for debugging
            Registers.PC--;
            var message = $"{ToHex(Ram.GetNextN())} ({ToHex(Ram.GetNextN())} {ToHex(Ram.GetNextN())})";
            throw new NotImplementedException(message);
        };

        private static readonly IReadOnlyDictionary<byte, (string name, Action execute)> Map = new ReadOnlyDictionary<byte, (string name, Action execute)>(
            new Dictionary<byte, (string name, Action execute)>
            {
#region 0x0_

                { 0x00, ("NOOP", () => { }) },
                { 0x01, ("LD BC,d16", LdNN(Setters.SetBC, Ram.GetNextNN)) },
                { 0x02, ("LD (BC),A", LdIndirectN(Getters.GetBC, Getters.GetA)) },
                { 0x03, ("INC BC", Setters.IncBC) },
                { 0x04, ("INC B", Setters.IncB) },
                { 0x05, ("DEC B", Setters.DecB) },
                { 0x06, ("LD B,d8", LdN(Setters.SetB, Ram.GetNextN)) },
                { 0x07, ("RLCA", RotateLeft(Getters.GetA, Setters.SetA)) },
                { 0x08, ("LD (a16),SP", NotImpl) },
                { 0x09, ("ADD HL,BC", NotImpl) },
                { 0x0A, ("LD A,(BC)", LdN(Setters.SetA, Getters.GetIndirect(Getters.GetBC))) },
                { 0x0B, ("DEC BC", Setters.DecBC) },
                { 0x0C, ("INC C", Setters.IncC) },
                { 0x0D, ("DEC C", Setters.DecC) },
                { 0x0E, ("LD C,d8", LdN(Setters.SetC, Ram.GetNextN)) },
                { 0x0F, ("RRCA", RotateRight(Getters.GetA, Setters.SetA)) },

#endregion

#region 0x1_

                {
                    0x10, ("STOP 0", () =>
                    {
                        Cpu.StopMode = true;
                        Ram.HardwareRegisters.Timers.WriteValue(HWRegisterWindowHandler.Addresses.DIV, 0);

                        // Stop is for some reason 2 bytes wide despite the operand not being used, so we skip forward an extra byte
                        Registers.PC++;
                    })
                },
                { 0x11, ("LD DE,d16", LdNN(Setters.SetDE, Ram.GetNextNN)) },
                { 0x12, ("LD (DE),A", LdIndirectN(Getters.GetDE, Getters.GetA)) },
                { 0x13, ("INC DE", Setters.IncDE) },
                { 0x14, ("INC D", Setters.IncD) },
                { 0x15, ("DEC D", Setters.DecD) },
                { 0x16, ("LD D,d8", LdN(Setters.SetD, Ram.GetNextN)) },
                { 0x17, ("RLA", RotateLeftThroughCarry(Getters.GetA, Setters.SetA)) },
                { 0x18, ("JR r8", JumpRelative(Ram.GetNextSignedN)) },
                { 0x19, ("ADD HL,DE", NotImpl) },
                { 0x1A, ("LD A,(DE)", LdN(Setters.SetA, Getters.GetIndirect(Getters.GetDE))) },
                { 0x1B, ("DEC DE", Setters.DecDE) },
                { 0x1C, ("INC E", Setters.IncE) },
                { 0x1D, ("DEC E", Setters.DecE) },
                { 0x1E, ("LD E,d8", LdN(Setters.SetE, Ram.GetNextN)) },
                { 0x1F, ("RRA", RotateRightThroughCarry(Getters.GetA, Setters.SetA)) },

#endregion

#region 0x2_

                { 0x20, ("JR NZ,r8", JumpRelativeIf(Getters.Not(Getters.GetZero), Ram.GetNextSignedN)) },
                { 0x21, ("LD HL,d16", LdNN(Setters.SetHL, Ram.GetNextNN)) },
                {
                    0x22, ("LD (HL+),A", () =>
                    {
                        LdIndirectN(Getters.GetHL, Getters.GetA);
                        Setters.IncHL();
                    })
                },
                { 0x23, ("INC HL", Setters.IncHL) },
                { 0x24, ("INC H", Setters.IncH) },
                { 0x25, ("DEC H", Setters.DecH) },
                { 0x26, ("LD H,d8", LdN(Setters.SetH, Ram.GetNextN)) },
                { 0x27, ("DAA", NotImpl) },
                { 0x28, ("JR Z,r8", JumpRelativeIf(Getters.GetZero, Ram.GetNextSignedN)) },
                { 0x29, ("ADD HL,HL", NotImpl) },
                {
                    0x2A, ("LD A,(HL+)", () =>
                    {
                        LdN(Setters.SetA, Getters.GetHLI);
                        Setters.IncHL();
                    })
                },
                { 0x2B, ("DEC HL", Setters.DecHL) },
                { 0x2C, ("INC L", Setters.IncL) },
                { 0x2D, ("DEC L", Setters.DecL) },
                { 0x2E, ("LD L,d8", LdN(Setters.SetL, Ram.GetNextN)) },
                { 0x2F, ("CPL", NotImpl) },

#endregion

#region 0x3_

                { 0x30, ("JR NC,r8", JumpRelativeIf(Getters.Not(Getters.GetCarry), Ram.GetNextSignedN)) },
                { 0x31, ("LD SP,d16", LdNN(Setters.SetSP, Ram.GetNextNN)) },
                {
                    0x32, ("LD (HL-),A", () =>
                    {
                        LdIndirectN(Getters.GetHL, Getters.GetA);
                        Setters.DecHL();
                    })
                },
                { 0x33, ("INC SP", Setters.IncSP) },
                { 0x34, ("INC (HL)", Setters.IncN(Setters.SetHLI, Getters.GetHLI)) },
                { 0x35, ("DEC (HL)", Setters.DecN(Setters.SetHLI, Getters.GetHLI)) },
                { 0x36, ("LD (HL),d8", LdN(Setters.SetHLI, Ram.GetNextN)) },
                { 0x37, ("SCF", NotImpl) },
                { 0x38, ("JR C,r8", JumpRelativeIf(Getters.GetCarry, Ram.GetNextSignedN)) },
                { 0x39, ("ADD HL,SP", NotImpl) },
                {
                    0x3A, ("LD A,(HL-)", () =>
                    {
                        LdN(Setters.SetA, Getters.GetHLI);
                        Setters.DecHL();
                    })
                },
                { 0x3B, ("DEC SP", Setters.DecSP) },
                { 0x3C, ("INC A", Setters.IncA) },
                { 0x3D, ("DEC A", Setters.DecA) },
                { 0x3E, ("LD A,d8", LdN(Setters.SetA, Ram.GetNextN)) },
                { 0x3F, ("CCF", NotImpl) },

#endregion

#region 0x4_

                { 0x40, ("LD B,B", LdN(Setters.SetB, Getters.GetB)) },
                { 0x41, ("LD B,C", LdN(Setters.SetB, Getters.GetC)) },
                { 0x42, ("LD B,D", LdN(Setters.SetB, Getters.GetD)) },
                { 0x43, ("LD B,E", LdN(Setters.SetB, Getters.GetE)) },
                { 0x44, ("LD B,H", LdN(Setters.SetB, Getters.GetH)) },
                { 0x45, ("LD B,L", LdN(Setters.SetB, Getters.GetL)) },
                { 0x46, ("LD B,(HL)", LdN(Setters.SetB, Getters.GetHLI)) },
                { 0x47, ("LD B,A", LdN(Setters.SetB, Getters.GetA)) },
                { 0x48, ("LD C,B", LdN(Setters.SetC, Getters.GetB)) },
                { 0x49, ("LD C,C", LdN(Setters.SetC, Getters.GetC)) },
                { 0x4A, ("LD C,D", LdN(Setters.SetC, Getters.GetD)) },
                { 0x4B, ("LD C,E", LdN(Setters.SetC, Getters.GetE)) },
                { 0x4C, ("LD C,H", LdN(Setters.SetC, Getters.GetH)) },
                { 0x4D, ("LD C,L", LdN(Setters.SetC, Getters.GetL)) },
                { 0x4E, ("LD C,(HL)", LdN(Setters.SetC, Getters.GetHLI)) },
                { 0x4F, ("LD C,A", LdN(Setters.SetC, Getters.GetA)) },

#endregion

#region 0x5_

                { 0x50, ("LD D,B", LdN(Setters.SetD, Getters.GetB)) },
                { 0x51, ("LD D,C", LdN(Setters.SetD, Getters.GetC)) },
                { 0x52, ("LD D,D", LdN(Setters.SetD, Getters.GetD)) },
                { 0x53, ("LD D,E", LdN(Setters.SetD, Getters.GetE)) },
                { 0x54, ("LD D,H", LdN(Setters.SetD, Getters.GetH)) },
                { 0x55, ("LD D,L", LdN(Setters.SetD, Getters.GetL)) },
                { 0x56, ("LD D,(HL)", LdN(Setters.SetD, Getters.GetHLI)) },
                { 0x57, ("LD D,A", LdN(Setters.SetD, Getters.GetA)) },
                { 0x58, ("LD E,B", LdN(Setters.SetE, Getters.GetB)) },
                { 0x59, ("LD E,C", LdN(Setters.SetE, Getters.GetC)) },
                { 0x5A, ("LD E,D", LdN(Setters.SetE, Getters.GetD)) },
                { 0x5B, ("LD E,E", LdN(Setters.SetE, Getters.GetE)) },
                { 0x5C, ("LD E,H", LdN(Setters.SetE, Getters.GetH)) },
                { 0x5D, ("LD E,L", LdN(Setters.SetE, Getters.GetL)) },
                { 0x5E, ("LD E,(HL)", LdN(Setters.SetE, Getters.GetHLI)) },
                { 0x5F, ("LD E,A", LdN(Setters.SetE, Getters.GetA)) },

#endregion

#region 0x6_

                { 0x60, ("LD H,B", LdN(Setters.SetH, Getters.GetB)) },
                { 0x61, ("LD H,C", LdN(Setters.SetH, Getters.GetC)) },
                { 0x62, ("LD H,D", LdN(Setters.SetH, Getters.GetD)) },
                { 0x63, ("LD H,E", LdN(Setters.SetH, Getters.GetE)) },
                { 0x64, ("LD H,H", LdN(Setters.SetH, Getters.GetH)) },
                { 0x65, ("LD H,L", LdN(Setters.SetH, Getters.GetL)) },
                { 0x66, ("LD H,(HL)", LdN(Setters.SetH, Getters.GetHLI)) },
                { 0x67, ("LD H,A", LdN(Setters.SetH, Getters.GetA)) },
                { 0x68, ("LD L,B", LdN(Setters.SetL, Getters.GetB)) },
                { 0x69, ("LD L,C", LdN(Setters.SetL, Getters.GetC)) },
                { 0x6A, ("LD L,D", LdN(Setters.SetL, Getters.GetD)) },
                { 0x6B, ("LD L,E", LdN(Setters.SetL, Getters.GetE)) },
                { 0x6C, ("LD L,H", LdN(Setters.SetL, Getters.GetH)) },
                { 0x6D, ("LD L,L", LdN(Setters.SetL, Getters.GetL)) },
                { 0x6E, ("LD L,(HL)", LdN(Setters.SetL, Getters.GetHLI)) },
                { 0x6F, ("LD L,A", LdN(Setters.SetL, Getters.GetA)) },

#endregion

#region 0x7_

                { 0x70, ("LD (HL),B", LdN(Setters.SetHLI, Getters.GetB)) },
                { 0x71, ("LD (HL),C", LdN(Setters.SetHLI, Getters.GetC)) },
                { 0x72, ("LD (HL),D", LdN(Setters.SetHLI, Getters.GetD)) },
                { 0x73, ("LD (HL),E", LdN(Setters.SetHLI, Getters.GetE)) },
                { 0x74, ("LD (HL),H", LdN(Setters.SetHLI, Getters.GetH)) },
                { 0x75, ("LD (HL),L", LdN(Setters.SetHLI, Getters.GetL)) },
                { 0x76, ("HALT", NotImpl) },
                { 0x77, ("LD (HL),A", LdN(Setters.SetHLI, Getters.GetA)) },
                { 0x78, ("LD A,B", LdN(Setters.SetA, Getters.GetB)) },
                { 0x79, ("LD A,C", LdN(Setters.SetA, Getters.GetC)) },
                { 0x7A, ("LD A,D", LdN(Setters.SetA, Getters.GetD)) },
                { 0x7B, ("LD A,E", LdN(Setters.SetA, Getters.GetE)) },
                { 0x7C, ("LD A,H", LdN(Setters.SetA, Getters.GetH)) },
                { 0x7D, ("LD A,L", LdN(Setters.SetA, Getters.GetL)) },
                { 0x7E, ("LD A,(HL)", LdN(Setters.SetA, Getters.GetHLI)) },
                { 0x7F, ("LD A,A", LdN(Setters.SetA, Getters.GetA)) },

#endregion

#region 0x8_

                { 0x80, ("ADD A,B", Setters.AddN(Getters.GetB)) },
                { 0x81, ("ADD A,C", Setters.AddN(Getters.GetC)) },
                { 0x82, ("ADD A,D", Setters.AddN(Getters.GetD)) },
                { 0x83, ("ADD A,E", Setters.AddN(Getters.GetE)) },
                { 0x84, ("ADD A,H", Setters.AddN(Getters.GetH)) },
                { 0x85, ("ADD A,L", Setters.AddN(Getters.GetL)) },
                { 0x86, ("ADD A,(HL)", Setters.AddN(Getters.GetHLI)) },
                { 0x87, ("ADD A,A", Setters.AddN(Getters.GetA)) },
                { 0x88, ("ADC A,B", Setters.AdcN(Getters.GetB)) },
                { 0x89, ("ADC A,C", Setters.AdcN(Getters.GetC)) },
                { 0x8A, ("ADC A,D", Setters.AdcN(Getters.GetD)) },
                { 0x8B, ("ADC A,E", Setters.AdcN(Getters.GetE)) },
                { 0x8C, ("ADC A,H", Setters.AdcN(Getters.GetH)) },
                { 0x8D, ("ADC A,L", Setters.AdcN(Getters.GetL)) },
                { 0x8E, ("ADC A,(HL)", Setters.AdcN(Getters.GetHLI)) },
                { 0x8F, ("ADC A,A", Setters.AdcN(Getters.GetA)) },

#endregion

#region 0x9_

                { 0x90, ("SUB B", Setters.SubN(Getters.GetB)) },
                { 0x91, ("SUB C", Setters.SubN(Getters.GetC)) },
                { 0x92, ("SUB D", Setters.SubN(Getters.GetD)) },
                { 0x93, ("SUB E", Setters.SubN(Getters.GetE)) },
                { 0x94, ("SUB H", Setters.SubN(Getters.GetH)) },
                { 0x95, ("SUB L", Setters.SubN(Getters.GetL)) },
                { 0x96, ("SUB (HL)", Setters.SubN(Getters.GetHLI)) },
                { 0x97, ("SUB A", Setters.SubN(Getters.GetA)) },
                { 0x98, ("SBC A,B", Setters.SbcN(Getters.GetB)) },
                { 0x99, ("SBC A,C", Setters.SbcN(Getters.GetC)) },
                { 0x9A, ("SBC A,D", Setters.SbcN(Getters.GetD)) },
                { 0x9B, ("SBC A,E", Setters.SbcN(Getters.GetE)) },
                { 0x9C, ("SBC A,H", Setters.SbcN(Getters.GetH)) },
                { 0x9D, ("SBC A,L", Setters.SbcN(Getters.GetL)) },
                { 0x9E, ("SBC A,(HL)", Setters.SbcN(Getters.GetHLI)) },
                { 0x9F, ("SBC A,A", Setters.SbcN(Getters.GetA)) },

#endregion

#region 0xA_

                { 0xA0, ("AND B", Setters.And(Getters.GetB)) },
                { 0xA1, ("AND C", Setters.And(Getters.GetC)) },
                { 0xA2, ("AND D", Setters.And(Getters.GetD)) },
                { 0xA3, ("AND E", Setters.And(Getters.GetE)) },
                { 0xA4, ("AND H", Setters.And(Getters.GetH)) },
                { 0xA5, ("AND L", Setters.And(Getters.GetL)) },
                { 0xA6, ("AND (HL)", Setters.And(Getters.GetHLI)) },
                { 0xA7, ("AND A", Setters.And(Getters.GetA)) },
                { 0xA8, ("XOR B", Setters.Xor(Getters.GetB)) },
                { 0xA9, ("XOR C", Setters.Xor(Getters.GetC)) },
                { 0xAA, ("XOR D", Setters.Xor(Getters.GetD)) },
                { 0xAB, ("XOR E", Setters.Xor(Getters.GetE)) },
                { 0xAC, ("XOR H", Setters.Xor(Getters.GetH)) },
                { 0xAD, ("XOR L", Setters.Xor(Getters.GetL)) },
                { 0xAE, ("XOR (HL)", Setters.Xor(Getters.GetHLI)) },
                { 0xAF, ("XOR A", Setters.Xor(Getters.GetA)) },

#endregion

#region 0xB_

                { 0xB0, ("OR B", Setters.Or(Getters.GetB)) },
                { 0xB1, ("OR C", Setters.Or(Getters.GetC)) },
                { 0xB2, ("OR D", Setters.Or(Getters.GetD)) },
                { 0xB3, ("OR E", Setters.Or(Getters.GetE)) },
                { 0xB4, ("OR H", Setters.Or(Getters.GetH)) },
                { 0xB5, ("OR L", Setters.Or(Getters.GetL)) },
                { 0xB6, ("OR (HL)", Setters.Or(Getters.GetHLI)) },
                { 0xB7, ("OR A", Setters.Or(Getters.GetA)) },
                { 0xB8, ("CP B", Compare(Getters.GetB)) },
                { 0xB9, ("CP C", Compare(Getters.GetC)) },
                { 0xBA, ("CP D", Compare(Getters.GetD)) },
                { 0xBB, ("CP E", Compare(Getters.GetE)) },
                { 0xBC, ("CP H", Compare(Getters.GetH)) },
                { 0xBD, ("CP L", Compare(Getters.GetL)) },
                { 0xBE, ("CP (HL)", Compare(Getters.GetHLI)) },
                { 0xBF, ("CP A", Compare(Getters.GetA)) },

#endregion

#region 0xC_

                { 0xC0, ("RET NZ", ReturnIf(Getters.Not(Getters.GetZero))) },
                { 0xC1, ("POP BC", Pop(Setters.SetBC)) },
                { 0xC2, ("JP NZ,a16", JumpIf(Getters.Not(Getters.GetZero), Ram.GetNextNN)) },
                { 0xC3, ("JP a16", Jump(Ram.GetNextNN)) },
                { 0xC4, ("CALL NZ,a16", CallIf(Getters.Not(Getters.GetZero), Ram.GetNextNN)) },
                { 0xC5, ("PUSH BC", Push(Getters.GetBC)) },
                { 0xC6, ("ADD A,d8", Setters.AddN(Ram.GetNextN)) },
                { 0xC7, ("RST 00H", NotImpl) },
                { 0xC8, ("RET Z", ReturnIf(Getters.GetZero)) },
                { 0xC9, ("RET", ReturnIf(() => true)) },
                { 0xCA, ("JP Z,a16", JumpIf(Getters.GetZero, Ram.GetNextNN)) },
                { 0xCB, ("PREFIX CB", () => ExecuteFrom(CbMap, Ram.GetNextN())) },
                { 0xCC, ("CALL Z,a16", CallIf(Getters.GetZero, Ram.GetNextNN)) },
                { 0xCD, ("CALL a16", Call(Ram.GetNextNN)) },
                { 0xCE, ("ADC A,d8", Setters.AdcN(Ram.GetNextN)) },
                { 0xCF, ("RST 08H", NotImpl) },

#endregion

#region 0xD_

                { 0xD0, ("RET NC", ReturnIf(Getters.Not(Getters.GetCarry))) },
                { 0xD1, ("POP DE", Pop(Setters.SetDE)) },
                { 0xD2, ("JP NC,a16", JumpIf(Getters.Not(Getters.GetCarry), Ram.GetNextNN)) },
                // 0xD3 - Non-existent
                { 0xD4, ("CALL NC,a16", CallIf(Getters.Not(Getters.GetCarry), Ram.GetNextNN)) },
                { 0xD5, ("PUSH DE", Push(Getters.GetDE)) },
                { 0xD6, ("SUB d8", Setters.SubN(Ram.GetNextN)) },
                { 0xD7, ("RST 10H", NotImpl) },
                { 0xD8, ("RET C", ReturnIf(Getters.GetCarry)) },
                { 0xD9, ("RETI", () =>
                {
                    ReturnIf(() => true);
                    Registers.IME = true;
                }) },
                { 0xDA, ("JP C,a16", JumpIf(Getters.GetCarry, Ram.GetNextNN)) },
                // 0xDB - Non-existent
                { 0xDC, ("CALL C,a16", CallIf(Getters.GetCarry, Ram.GetNextNN)) },
                // 0xDD - Non-existent
                { 0xDE, ("SBC A,d8", Setters.SbcN(Ram.GetNextN)) },
                { 0xDF, ("RST 18H", NotImpl) },

#endregion

#region 0xE_

                { 0xE0, ("LDH (a8),A", LdIndirectN(Ram.GetNextN, Getters.GetA)) },
                { 0xE1, ("POP HL", Pop(Setters.SetHL)) },
                { 0xE2, ("LD (C),A", LdIndirectN(Getters.GetC, Getters.GetA)) },
                // 0xE3 - Non-existent
                // 0xE4 - Non-existent
                { 0xE5, ("PUSH HL", Push(Getters.GetHL)) },
                { 0xE6, ("AND d8", Setters.And(Ram.GetNextN)) },
                { 0xE7, ("RST 20H", NotImpl) },
                { 0xE8, ("ADD SP,r8", NotImpl) },
                { 0xE9, ("JP (HL)", Jump(Getters.GetHL)) },
                { 0xEA, ("LD (a16),A", LdIndirectN(Ram.GetNextNN, Getters.GetA)) },
                // 0xEB - Non-existent
                // 0xEC - Non-existent
                // 0xED - Non-existent
                { 0xEE, ("XOR d8", Setters.Xor(Ram.GetNextN)) },
                { 0xEF, ("RST 28H", NotImpl) },

#endregion

#region 0xF_

                { 0xF0, ("LDH A,(a8)", LdN(Setters.SetA, Getters.GetIndirect(() => (ushort)(0xFF00 + Ram.GetNextN())))) },
                { 0xF1, ("POP AF", Pop(Setters.SetAF)) },
                { 0xF2, ("LD A,(C)", LdN(Setters.SetA, Getters.GetIndirect(() => (ushort)(0xFF00 + Getters.GetC())))) },
                { 0xF3, ("DI", () => Registers.IME = false) },
                // 0xF4 - Non-existent
                { 0xF5, ("PUSH AF", Push(Getters.GetAF)) },
                { 0xF6, ("OR d8", Setters.Or(Ram.GetNextN)) },
                { 0xF7, ("RST 30H", NotImpl) },
                { 0xF8, ("LD HL,SP+r8", NotImpl) },
                { 0xF9, ("LD SP,HL", LdNN(Setters.SetSP, Getters.GetHL)) },
                { 0xFA, ("LD A,(a16)", LdN(Setters.SetA, Getters.GetIndirect(Ram.GetNextNN))) },
                { 0xFB, ("EI", () => Registers.IME = true) },
                // 0xFC - Non-existent
                // 0xFD - Non-existent
                { 0xFE, ("CP d8", Compare(Ram.GetNextN)) },
                { 0xFF, ("RST 38H", NotImpl) },

#endregion
            }
        );

        private static readonly IReadOnlyDictionary<byte, (string name, Action execute)> CbMap = new ReadOnlyDictionary<byte, (string name, Action execute)>(
            new Dictionary<byte, (string name, Action execute)>
            {
#region 0x0_

                { 0x00, ("RLC B", RotateLeft(Getters.GetB, Setters.SetB)) },
                { 0x01, ("RLC C", RotateLeft(Getters.GetC, Setters.SetC)) },
                { 0x02, ("RLC D", RotateLeft(Getters.GetD, Setters.SetD)) },
                { 0x03, ("RLC E", RotateLeft(Getters.GetE, Setters.SetE)) },
                { 0x04, ("RLC H", RotateLeft(Getters.GetH, Setters.SetH)) },
                { 0x05, ("RLC L", RotateLeft(Getters.GetL, Setters.SetL)) },
                { 0x06, ("RLC (HL)", RotateLeft(Getters.GetHLI, Setters.SetHLI)) },
                { 0x07, ("RLC A", RotateLeft(Getters.GetA, Setters.SetA)) },
                { 0x08, ("RRC B", RotateRight(Getters.GetB, Setters.SetB)) },
                { 0x09, ("RRC C", RotateRight(Getters.GetC, Setters.SetC)) },
                { 0x0A, ("RRC D", RotateRight(Getters.GetD, Setters.SetD)) },
                { 0x0B, ("RRC E", RotateRight(Getters.GetE, Setters.SetE)) },
                { 0x0C, ("RRC H", RotateRight(Getters.GetH, Setters.SetH)) },
                { 0x0D, ("RRC L", RotateRight(Getters.GetL, Setters.SetL)) },
                { 0x0E, ("RRC (HL)", RotateRight(Getters.GetHLI, Setters.SetHLI)) },
                { 0x0F, ("RRC A", RotateRight(Getters.GetA, Setters.SetA)) },

#endregion

#region 0x1_

                { 0x10, ("RL B", RotateLeftThroughCarry(Getters.GetB, Setters.SetB)) },
                { 0x11, ("RL C", RotateLeftThroughCarry(Getters.GetC, Setters.SetC)) },
                { 0x12, ("RL D", RotateLeftThroughCarry(Getters.GetD, Setters.SetD)) },
                { 0x13, ("RL E", RotateLeftThroughCarry(Getters.GetE, Setters.SetE)) },
                { 0x14, ("RL H", RotateLeftThroughCarry(Getters.GetH, Setters.SetH)) },
                { 0x15, ("RL L", RotateLeftThroughCarry(Getters.GetL, Setters.SetL)) },
                { 0x16, ("RL (HL)", RotateLeftThroughCarry(Getters.GetHLI, Setters.SetHLI)) },
                { 0x17, ("RL A", RotateLeftThroughCarry(Getters.GetA, Setters.SetA)) },
                { 0x18, ("RR B", RotateRightThroughCarry(Getters.GetB, Setters.SetB)) },
                { 0x19, ("RR C", RotateRightThroughCarry(Getters.GetC, Setters.SetC)) },
                { 0x1A, ("RR D", RotateRightThroughCarry(Getters.GetD, Setters.SetD)) },
                { 0x1B, ("RR E", RotateRightThroughCarry(Getters.GetE, Setters.SetE)) },
                { 0x1C, ("RR H", RotateRightThroughCarry(Getters.GetH, Setters.SetH)) },
                { 0x1D, ("RR L", RotateRightThroughCarry(Getters.GetL, Setters.SetL)) },
                { 0x1E, ("RR (HL)", RotateRightThroughCarry(Getters.GetHLI, Setters.SetHLI)) },
                { 0x1F, ("RR A", RotateRightThroughCarry(Getters.GetA, Setters.SetA)) },

#endregion

#region 0x2_

                { 0x20, ("SLA B", NotImpl) },
                { 0x21, ("SLA C", NotImpl) },
                { 0x22, ("SLA D", NotImpl) },
                { 0x23, ("SLA E", NotImpl) },
                { 0x24, ("SLA H", NotImpl) },
                { 0x25, ("SLA L", NotImpl) },
                { 0x26, ("SLA (HL)", NotImpl) },
                { 0x27, ("SLA A", NotImpl) },
                { 0x28, ("SRA B", NotImpl) },
                { 0x29, ("SRA C", NotImpl) },
                { 0x2A, ("SRA D", NotImpl) },
                { 0x2B, ("SRA E", NotImpl) },
                { 0x2C, ("SRA H", NotImpl) },
                { 0x2D, ("SRA L", NotImpl) },
                { 0x2E, ("SRA (HL)", NotImpl) },
                { 0x2F, ("SRA A", NotImpl) },

#endregion

#region 0x3_

                { 0x30, ("SWAP B", NotImpl) },
                { 0x31, ("SWAP C", NotImpl) },
                { 0x32, ("SWAP D", NotImpl) },
                { 0x33, ("SWAP E", NotImpl) },
                { 0x34, ("SWAP H", NotImpl) },
                { 0x35, ("SWAP L", NotImpl) },
                { 0x36, ("SWAP (HL)", NotImpl) },
                { 0x37, ("SWAP A", NotImpl) },
                { 0x38, ("SRL B", NotImpl) },
                { 0x39, ("SRL C", NotImpl) },
                { 0x3A, ("SRL D", NotImpl) },
                { 0x3B, ("SRL E", NotImpl) },
                { 0x3C, ("SRL H", NotImpl) },
                { 0x3D, ("SRL L", NotImpl) },
                { 0x3E, ("SRL (HL)", NotImpl) },
                { 0x3F, ("SRL A", NotImpl) },

#endregion

#region 0x4_

                { 0x40, ("BIT 0,B", Bit(0, Getters.GetB)) },
                { 0x41, ("BIT 0,C", Bit(0, Getters.GetC)) },
                { 0x42, ("BIT 0,D", Bit(0, Getters.GetD)) },
                { 0x43, ("BIT 0,E", Bit(0, Getters.GetE)) },
                { 0x44, ("BIT 0,H", Bit(0, Getters.GetH)) },
                { 0x45, ("BIT 0,L", Bit(0, Getters.GetL)) },
                { 0x46, ("BIT 0,(HL)", Bit(0, Getters.GetHLI)) },
                { 0x47, ("BIT 0,A", Bit(0, Getters.GetA)) },
                { 0x48, ("BIT 1,B", Bit(1, Getters.GetB)) },
                { 0x49, ("BIT 1,C", Bit(1, Getters.GetC)) },
                { 0x4A, ("BIT 1,D", Bit(1, Getters.GetD)) },
                { 0x4B, ("BIT 1,E", Bit(1, Getters.GetE)) },
                { 0x4C, ("BIT 1,H", Bit(1, Getters.GetH)) },
                { 0x4D, ("BIT 1,L", Bit(1, Getters.GetL)) },
                { 0x4E, ("BIT 1,(HL)", Bit(1, Getters.GetHLI)) },
                { 0x4F, ("BIT 1,A", Bit(1, Getters.GetA)) },

#endregion

#region 0x5_

                { 0x50, ("BIT 2,B", Bit(2, Getters.GetB)) },
                { 0x51, ("BIT 2,C", Bit(2, Getters.GetC)) },
                { 0x52, ("BIT 2,D", Bit(2, Getters.GetD)) },
                { 0x53, ("BIT 2,E", Bit(2, Getters.GetE)) },
                { 0x54, ("BIT 2,H", Bit(2, Getters.GetH)) },
                { 0x55, ("BIT 2,L", Bit(2, Getters.GetL)) },
                { 0x56, ("BIT 2,(HL)", Bit(2, Getters.GetHLI)) },
                { 0x57, ("BIT 2,A", Bit(2, Getters.GetA)) },
                { 0x58, ("BIT 3,B", Bit(3, Getters.GetB)) },
                { 0x59, ("BIT 3,C", Bit(3, Getters.GetC)) },
                { 0x5A, ("BIT 3,D", Bit(3, Getters.GetD)) },
                { 0x5B, ("BIT 3,E", Bit(3, Getters.GetE)) },
                { 0x5C, ("BIT 3,H", Bit(3, Getters.GetH)) },
                { 0x5D, ("BIT 3,L", Bit(3, Getters.GetL)) },
                { 0x5E, ("BIT 3,(HL)", Bit(3, Getters.GetHLI)) },
                { 0x5F, ("BIT 3,A", Bit(3, Getters.GetA)) },

#endregion

#region 0x6_

                { 0x60, ("BIT 4,B", Bit(4, Getters.GetB)) },
                { 0x61, ("BIT 4,C", Bit(4, Getters.GetC)) },
                { 0x62, ("BIT 4,D", Bit(4, Getters.GetD)) },
                { 0x63, ("BIT 4,E", Bit(4, Getters.GetE)) },
                { 0x64, ("BIT 4,H", Bit(4, Getters.GetH)) },
                { 0x65, ("BIT 4,L", Bit(4, Getters.GetL)) },
                { 0x66, ("BIT 4,(HL)", Bit(4, Getters.GetHLI)) },
                { 0x67, ("BIT 4,A", Bit(4, Getters.GetA)) },
                { 0x68, ("BIT 5,B", Bit(5, Getters.GetB)) },
                { 0x69, ("BIT 5,C", Bit(5, Getters.GetC)) },
                { 0x6A, ("BIT 5,D", Bit(5, Getters.GetD)) },
                { 0x6B, ("BIT 5,E", Bit(5, Getters.GetE)) },
                { 0x6C, ("BIT 5,H", Bit(5, Getters.GetH)) },
                { 0x6D, ("BIT 5,L", Bit(5, Getters.GetL)) },
                { 0x6E, ("BIT 5,(HL)", Bit(5, Getters.GetHLI)) },
                { 0x6F, ("BIT 5,A", Bit(5, Getters.GetA)) },

#endregion

#region 0x7_

                { 0x70, ("BIT 6,B", Bit(6, Getters.GetB)) },
                { 0x71, ("BIT 6,C", Bit(6, Getters.GetC)) },
                { 0x72, ("BIT 6,D", Bit(6, Getters.GetD)) },
                { 0x73, ("BIT 6,E", Bit(6, Getters.GetE)) },
                { 0x74, ("BIT 6,H", Bit(6, Getters.GetH)) },
                { 0x75, ("BIT 6,L", Bit(6, Getters.GetL)) },
                { 0x76, ("BIT 6,(HL)", Bit(6, Getters.GetHLI)) },
                { 0x77, ("BIT 6,A", Bit(6, Getters.GetA)) },
                { 0x78, ("BIT 7,B", Bit(7, Getters.GetB)) },
                { 0x79, ("BIT 7,C", Bit(7, Getters.GetC)) },
                { 0x7A, ("BIT 7,D", Bit(7, Getters.GetD)) },
                { 0x7B, ("BIT 7,E", Bit(7, Getters.GetE)) },
                { 0x7C, ("BIT 7,H", Bit(7, Getters.GetH)) },
                { 0x7D, ("BIT 7,L", Bit(7, Getters.GetL)) },
                { 0x7E, ("BIT 7,(HL)", Bit(7, Getters.GetHLI)) },
                { 0x7F, ("BIT 7,A", Bit(7, Getters.GetA)) },

#endregion

#region 0x8_

                { 0x80, ("RES 0,B", Unset(0, Getters.GetB, Setters.SetB)) },
                { 0x81, ("RES 0,C", Unset(0, Getters.GetC, Setters.SetC)) },
                { 0x82, ("RES 0,D", Unset(0, Getters.GetD, Setters.SetD)) },
                { 0x83, ("RES 0,E", Unset(0, Getters.GetE, Setters.SetE)) },
                { 0x84, ("RES 0,H", Unset(0, Getters.GetH, Setters.SetH)) },
                { 0x85, ("RES 0,L", Unset(0, Getters.GetL, Setters.SetL)) },
                { 0x86, ("RES 0,(HL)", Unset(0, Getters.GetHLI, Setters.SetHLI)) },
                { 0x87, ("RES 0,A", Unset(0, Getters.GetA, Setters.SetA)) },
                { 0x88, ("RES 1,B", Unset(1, Getters.GetB, Setters.SetB)) },
                { 0x89, ("RES 1,C", Unset(1, Getters.GetC, Setters.SetC)) },
                { 0x8A, ("RES 1,D", Unset(1, Getters.GetD, Setters.SetD)) },
                { 0x8B, ("RES 1,E", Unset(1, Getters.GetE, Setters.SetE)) },
                { 0x8C, ("RES 1,H", Unset(1, Getters.GetH, Setters.SetH)) },
                { 0x8D, ("RES 1,L", Unset(1, Getters.GetL, Setters.SetL)) },
                { 0x8E, ("RES 1,(HL)", Unset(1, Getters.GetHLI, Setters.SetHLI)) },
                { 0x8F, ("RES 1,A", Unset(1, Getters.GetA, Setters.SetA)) },

#endregion

#region 0x9_

                { 0x90, ("RES 2,B", Unset(2, Getters.GetB, Setters.SetB)) },
                { 0x91, ("RES 2,C", Unset(2, Getters.GetC, Setters.SetC)) },
                { 0x92, ("RES 2,D", Unset(2, Getters.GetD, Setters.SetD)) },
                { 0x93, ("RES 2,E", Unset(2, Getters.GetE, Setters.SetE)) },
                { 0x94, ("RES 2,H", Unset(2, Getters.GetH, Setters.SetH)) },
                { 0x95, ("RES 2,L", Unset(2, Getters.GetL, Setters.SetL)) },
                { 0x96, ("RES 2,(HL)", Unset(2, Getters.GetHLI, Setters.SetHLI)) },
                { 0x97, ("RES 2,A", Unset(2, Getters.GetA, Setters.SetA)) },
                { 0x98, ("RES 3,B", Unset(3, Getters.GetB, Setters.SetB)) },
                { 0x99, ("RES 3,C", Unset(3, Getters.GetC, Setters.SetC)) },
                { 0x9A, ("RES 3,D", Unset(3, Getters.GetD, Setters.SetD)) },
                { 0x9B, ("RES 3,E", Unset(3, Getters.GetE, Setters.SetE)) },
                { 0x9C, ("RES 3,H", Unset(3, Getters.GetH, Setters.SetH)) },
                { 0x9D, ("RES 3,L", Unset(3, Getters.GetL, Setters.SetL)) },
                { 0x9E, ("RES 3,(HL)", Unset(3, Getters.GetHLI, Setters.SetHLI)) },
                { 0x9F, ("RES 3,A", Unset(3, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xA_

                { 0xA0, ("RES 4,B", Unset(4, Getters.GetB, Setters.SetB)) },
                { 0xA1, ("RES 4,C", Unset(4, Getters.GetC, Setters.SetC)) },
                { 0xA2, ("RES 4,D", Unset(4, Getters.GetD, Setters.SetD)) },
                { 0xA3, ("RES 4,E", Unset(4, Getters.GetE, Setters.SetE)) },
                { 0xA4, ("RES 4,H", Unset(4, Getters.GetH, Setters.SetH)) },
                { 0xA5, ("RES 4,L", Unset(4, Getters.GetL, Setters.SetL)) },
                { 0xA6, ("RES 4,(HL)", Unset(4, Getters.GetHLI, Setters.SetHLI)) },
                { 0xA7, ("RES 4,A", Unset(4, Getters.GetA, Setters.SetA)) },
                { 0xA8, ("RES 5,B", Unset(5, Getters.GetB, Setters.SetB)) },
                { 0xA9, ("RES 5,C", Unset(5, Getters.GetC, Setters.SetC)) },
                { 0xAA, ("RES 5,D", Unset(5, Getters.GetD, Setters.SetD)) },
                { 0xAB, ("RES 5,E", Unset(5, Getters.GetE, Setters.SetE)) },
                { 0xAC, ("RES 5,H", Unset(5, Getters.GetH, Setters.SetH)) },
                { 0xAD, ("RES 5,L", Unset(5, Getters.GetL, Setters.SetL)) },
                { 0xAE, ("RES 5,(HL)", Unset(5, Getters.GetHLI, Setters.SetHLI)) },
                { 0xAF, ("RES 5,A", Unset(5, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xB_

                { 0xB0, ("RES 6,B", Unset(6, Getters.GetB, Setters.SetB)) },
                { 0xB1, ("RES 6,C", Unset(6, Getters.GetC, Setters.SetC)) },
                { 0xB2, ("RES 6,D", Unset(6, Getters.GetD, Setters.SetD)) },
                { 0xB3, ("RES 6,E", Unset(6, Getters.GetE, Setters.SetE)) },
                { 0xB4, ("RES 6,H", Unset(6, Getters.GetH, Setters.SetH)) },
                { 0xB5, ("RES 6,L", Unset(6, Getters.GetL, Setters.SetL)) },
                { 0xB6, ("RES 6,(HL)", Unset(6, Getters.GetHLI, Setters.SetHLI)) },
                { 0xB7, ("RES 6,A", Unset(6, Getters.GetA, Setters.SetA)) },
                { 0xB8, ("RES 7,B", Unset(7, Getters.GetB, Setters.SetB)) },
                { 0xB9, ("RES 7,C", Unset(7, Getters.GetC, Setters.SetC)) },
                { 0xBA, ("RES 7,D", Unset(7, Getters.GetD, Setters.SetD)) },
                { 0xBB, ("RES 7,E", Unset(7, Getters.GetE, Setters.SetE)) },
                { 0xBC, ("RES 7,H", Unset(7, Getters.GetH, Setters.SetH)) },
                { 0xBD, ("RES 7,L", Unset(7, Getters.GetL, Setters.SetL)) },
                { 0xBE, ("RES 7,(HL)", Unset(7, Getters.GetHLI, Setters.SetHLI)) },
                { 0xBF, ("RES 7,A", Unset(7, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xC_

                { 0xC0, ("SET 0,B", Set(0, Getters.GetB, Setters.SetB)) },
                { 0xC1, ("SET 0,C", Set(0, Getters.GetC, Setters.SetC)) },
                { 0xC2, ("SET 0,D", Set(0, Getters.GetD, Setters.SetD)) },
                { 0xC3, ("SET 0,E", Set(0, Getters.GetE, Setters.SetE)) },
                { 0xC4, ("SET 0,H", Set(0, Getters.GetH, Setters.SetH)) },
                { 0xC5, ("SET 0,L", Set(0, Getters.GetL, Setters.SetL)) },
                { 0xC6, ("SET 0,(HL)", Set(0, Getters.GetHLI, Setters.SetHLI)) },
                { 0xC7, ("SET 0,A", Set(0, Getters.GetA, Setters.SetA)) },
                { 0xC8, ("SET 1,B", Set(1, Getters.GetB, Setters.SetB)) },
                { 0xC9, ("SET 1,C", Set(1, Getters.GetC, Setters.SetC)) },
                { 0xCA, ("SET 1,D", Set(1, Getters.GetD, Setters.SetD)) },
                { 0xCB, ("SET 1,E", Set(1, Getters.GetE, Setters.SetE)) },
                { 0xCC, ("SET 1,H", Set(1, Getters.GetH, Setters.SetH)) },
                { 0xCD, ("SET 1,L", Set(1, Getters.GetL, Setters.SetL)) },
                { 0xCE, ("SET 1,(HL)", Set(1, Getters.GetHLI, Setters.SetHLI)) },
                { 0xCF, ("SET 1,A", Set(1, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xD_

                { 0xD0, ("SET 2,B", Set(2, Getters.GetB, Setters.SetB)) },
                { 0xD1, ("SET 2,C", Set(2, Getters.GetC, Setters.SetC)) },
                { 0xD2, ("SET 2,D", Set(2, Getters.GetD, Setters.SetD)) },
                { 0xD3, ("SET 2,E", Set(2, Getters.GetE, Setters.SetE)) },
                { 0xD4, ("SET 2,H", Set(2, Getters.GetH, Setters.SetH)) },
                { 0xD5, ("SET 2,L", Set(2, Getters.GetL, Setters.SetL)) },
                { 0xD6, ("SET 2,(HL)", Set(2, Getters.GetHLI, Setters.SetHLI)) },
                { 0xD7, ("SET 2,A", Set(2, Getters.GetA, Setters.SetA)) },
                { 0xD8, ("SET 3,B", Set(3, Getters.GetB, Setters.SetB)) },
                { 0xD9, ("SET 3,C", Set(3, Getters.GetC, Setters.SetC)) },
                { 0xDA, ("SET 3,D", Set(3, Getters.GetD, Setters.SetD)) },
                { 0xDB, ("SET 3,E", Set(3, Getters.GetE, Setters.SetE)) },
                { 0xDC, ("SET 3,H", Set(3, Getters.GetH, Setters.SetH)) },
                { 0xDD, ("SET 3,L", Set(3, Getters.GetL, Setters.SetL)) },
                { 0xDE, ("SET 3,(HL)", Set(3, Getters.GetHLI, Setters.SetHLI)) },
                { 0xDF, ("SET 3,A", Set(3, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xE_

                { 0xE0, ("SET 4,B", Set(4, Getters.GetB, Setters.SetB)) },
                { 0xE1, ("SET 4,C", Set(4, Getters.GetC, Setters.SetC)) },
                { 0xE2, ("SET 4,D", Set(4, Getters.GetD, Setters.SetD)) },
                { 0xE3, ("SET 4,E", Set(4, Getters.GetE, Setters.SetE)) },
                { 0xE4, ("SET 4,H", Set(4, Getters.GetH, Setters.SetH)) },
                { 0xE5, ("SET 4,L", Set(4, Getters.GetL, Setters.SetL)) },
                { 0xE6, ("SET 4,(HL)", Set(4, Getters.GetHLI, Setters.SetHLI)) },
                { 0xE7, ("SET 4,A", Set(4, Getters.GetA, Setters.SetA)) },
                { 0xE8, ("SET 5,B", Set(5, Getters.GetB, Setters.SetB)) },
                { 0xE9, ("SET 5,C", Set(5, Getters.GetC, Setters.SetC)) },
                { 0xEA, ("SET 5,D", Set(5, Getters.GetD, Setters.SetD)) },
                { 0xEB, ("SET 5,E", Set(5, Getters.GetE, Setters.SetE)) },
                { 0xEC, ("SET 5,H", Set(5, Getters.GetH, Setters.SetH)) },
                { 0xED, ("SET 5,L", Set(5, Getters.GetL, Setters.SetL)) },
                { 0xEE, ("SET 5,(HL)", Set(5, Getters.GetHLI, Setters.SetHLI)) },
                { 0xEF, ("SET 5,A", Set(5, Getters.GetA, Setters.SetA)) },

#endregion

#region 0xF_

                { 0xF0, ("SET 6,B", Set(6, Getters.GetB, Setters.SetB)) },
                { 0xF1, ("SET 6,C", Set(6, Getters.GetC, Setters.SetC)) },
                { 0xF2, ("SET 6,D", Set(6, Getters.GetD, Setters.SetD)) },
                { 0xF3, ("SET 6,E", Set(6, Getters.GetE, Setters.SetE)) },
                { 0xF4, ("SET 6,H", Set(6, Getters.GetH, Setters.SetH)) },
                { 0xF5, ("SET 6,L", Set(6, Getters.GetL, Setters.SetL)) },
                { 0xF6, ("SET 6,(HL)", Set(6, Getters.GetHLI, Setters.SetHLI)) },
                { 0xF7, ("SET 6,A", Set(6, Getters.GetA, Setters.SetA)) },
                { 0xF8, ("SET 7,B", Set(7, Getters.GetB, Setters.SetB)) },
                { 0xF9, ("SET 7,C", Set(7, Getters.GetC, Setters.SetC)) },
                { 0xFA, ("SET 7,D", Set(7, Getters.GetD, Setters.SetD)) },
                { 0xFB, ("SET 7,E", Set(7, Getters.GetE, Setters.SetE)) },
                { 0xFC, ("SET 7,H", Set(7, Getters.GetH, Setters.SetH)) },
                { 0xFD, ("SET 7,L", Set(7, Getters.GetL, Setters.SetL)) },
                { 0xFE, ("SET 7,(HL)", Set(7, Getters.GetHLI, Setters.SetHLI)) },
                { 0xFF, ("SET 7,A", Set(7, Getters.GetA, Setters.SetA)) },

#endregion
            }
        );
    }
}