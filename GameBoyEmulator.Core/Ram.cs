using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core
{
    public static partial class Ram
    {
        public static readonly HWRegisterWindowHandler HardwareRegisters =
            new HWRegisterWindowHandler(ReadByte, WriteByte);
        
        private static readonly byte[] Memory = new byte[0xFFFF + 1];
        private static byte ReadByte(ushort address) => Memory[address];
        private static void WriteByte(ushort address, byte value) => Memory[address] = value;

        private static readonly RangeList<ushort, RamWindowHandler> SpecialCaseHandlers =
            new RangeList<ushort, RamWindowHandler>
            {
                { (0x0000, 0x0100), new BootstrapWindowHandler(ReadByte) },
                { (0xFE00, 0xFE9F), new OAMWindowHandler(ReadByte, WriteByte) },
                { (0xFF00, 0xFF4B), HardwareRegisters }
            };

#region LCD Status Registers

        public static byte STAT
        {
            get => Memory[STAT_ADDR];
            set
            {
                Memory[STAT_ADDR] = value;
                UpdateLYCEqualsLYFlag();
            }
        }
        public static Modes STAT_Mode
        {
            get
            {
                var registerValue = STAT;
                var bit1 = Maths.BitIsSet(1, registerValue);
                var bit0 = Maths.BitIsSet(0, registerValue);
                switch (bit0)
                {
                    case true: return bit1 ? Modes.Vram : Modes.VBlank;
                    case false: return bit1 ? Modes.Oam : Modes.HBlank;
                }
            }
            set
            {
                var newValue = STAT;
                
                newValue = value is Modes.Vram or Modes.VBlank
                    ? Maths.SetBit(0, newValue)
                    : Maths.UnsetBit(0, newValue);
                newValue = value is Modes.Oam or Modes.Vram
                    ? Maths.SetBit(1, newValue)
                    : Maths.UnsetBit(1, newValue);
                
                STAT = newValue;
            }
        }
        
        public static byte LY
        {
            get => Memory[LY_ADDR];
            set
            {
                Memory[LY_ADDR] = value;
                UpdateLYCEqualsLYFlag();
            }
        }

        public static byte LYC
        {
            get => Memory[LYC_ADDR];
            set
            {
                Memory[LYC_ADDR] = value;
                UpdateLYCEqualsLYFlag();
            }
        }

        private const ushort STAT_ADDR = 0xFF41;
        private const ushort LY_ADDR = 0xFF44;
        private const ushort LYC_ADDR = 0xFF45;

#endregion

        public static byte SB
        {
            set
            {
                Memory[SB_ADDR] = value;
            }
        }

        private const ushort SB_ADDR = 0xFF01;

        public static void Reset()
        {
            Array.Fill(Memory, (byte)0x00);
        }

        public static void LoadROM(byte[] romBytes)
        {
            // TODO: This needs work
            Array.Copy(romBytes, Memory, Math.Min(romBytes.Length, Memory.Length));
        }

        public static byte GetN(ushort address)
        {
            Clock.Cycle += 4;

            return SpecialCaseHandlers.TryGetValue(address, out var handler)
                ? handler.ReadValue(address)
                : ReadByte(address);
        }

        public static ushort GetNN(ushort address) => Maths.CombineBytes(GetN(address), GetN((ushort)(address + 1)));

        public static void SetN(ushort address, byte value)
        {
            if (SpecialCaseHandlers.TryGetValue(address, out var handler))
            {
                handler.WriteValue(address, value);
                return;
            }
            
            // TODO: Any protected memory addresses?
            Clock.Cycle += 4;

            switch (address)
            {
                case SB_ADDR:
                    SB = value;
                    break;
                case STAT_ADDR:
                    STAT = value;
                    break;
                case LY_ADDR:
                    // This is readonly so ignore the write
                    break;
                case LYC_ADDR:
                    LYC = value;
                    break;
                default:
                    WriteByte(address, value);
                    break;
            }
        }

        public static byte GetNextN() => GetN(Registers.PC++);
        public static sbyte GetNextSignedN() => (sbyte)GetN(Registers.PC++); // TODO: This almost certainly won't work. How do we handle this?
        public static ushort GetNextNN() => Maths.CombineBytes(GetNextN(), GetNextN());

        private static void UpdateLYCEqualsLYFlag()
        {
            // Go direct to memory instead of via STAT property to avoid looping back into here
            Memory[STAT_ADDR] = LY == LYC
                ? Maths.SetBit(2, STAT)
                : Maths.UnsetBit(2, STAT);
        }
    }
}