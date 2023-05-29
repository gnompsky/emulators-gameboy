namespace GameBoyEmulator.Core
{
    public static class Ram
    {
        public static event Events.SerialCharacterWrittenHandler? SerialCharacterWritten;
        private static readonly byte[] Memory = new byte[int.MaxValue - 1000]; // TODO: Set actual max size

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
                SerialCharacterWritten?.Invoke(value);
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
            
            if ((address >= 0xFE00 && address <= 0xFE9F) && STAT_Mode is Modes.Oam or Modes.Vram)
            {
                // Protected during this GPU mode, return garbage
                return 0xFF;
            }
            if ((address >= 0xFE69 && address <= 0xFE6B) && STAT_Mode is Modes.Vram)
            {
                // Protected during this GPU mode, return garbage
                return 0xFF;
            }

            if (Memory[0xFF50] != 0x01 && address <= 0x0100)
            {
                // Boot ROM
                return BootstrapRom[address];
            }
            
            return Memory[address];
        }

        public static ushort GetNN(ushort address) => Maths.CombineBytes(GetN(address), GetN((ushort)(address + 1)));

        public static void SetN(ushort address, byte value)
        {
            // TODO: These two checks should be standardised better same with reads
            if ((address >= 0xFE00 && address <= 0xFE9F) && STAT_Mode is Modes.Oam or Modes.Vram)
            {
                // Protected during this GPU mode, do nothing
            }
            else if ((address >= 0xFE69 && address <= 0xFE6B) && STAT_Mode is Modes.Vram)
            {
                // Protected during this GPU mode, do nothing
            }
            if (Memory[0xFF50] != 0x01 && address <= 0x0100)
            {
                // Cart ROM is protected, do nothing
                // TODO: I assume?
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
                    Memory[address] = value;
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

        private static readonly byte[] BootstrapRom =
        {
            0x31, 0xFE, 0xFF, 0xAF, 0x21, 0xFF, 0x9F, 0x32, 0xCB, 0x7C, 0x20, 0xFB, 0x21, 0x26, 0xFF, 0x0E,
            0x11, 0x3E, 0x80, 0x32, 0xE2, 0x0C, 0x3E, 0xF3, 0xE2, 0x32, 0x3E, 0x77, 0x77, 0x3E, 0xFC, 0xE0,
            0x47, 0x11, 0x04, 0x01, 0x21, 0x10, 0x80, 0x1A, 0xCD, 0x95, 0x00, 0xCD, 0x96, 0x00, 0x13, 0x7B,
            0xFE, 0x34, 0x20, 0xF3, 0x11, 0xD8, 0x00, 0x06, 0x08, 0x1A, 0x13, 0x22, 0x23, 0x05, 0x20, 0xF9,
            0x3E, 0x19, 0xEA, 0x10, 0x99, 0x21, 0x2F, 0x99, 0x0E, 0x0C, 0x3D, 0x28, 0x08, 0x32, 0x0D, 0x20,
            0xF9, 0x2E, 0x0F, 0x18, 0xF3, 0x67, 0x3E, 0x64, 0x57, 0xE0, 0x42, 0x3E, 0x91, 0xE0, 0x40, 0x04,
            0x1E, 0x02, 0x0E, 0x0C, 0xF0, 0x44, 0xFE, 0x90, 0x20, 0xFA, 0x0D, 0x20, 0xF7, 0x1D, 0x20, 0xF2,
            0x0E, 0x13, 0x24, 0x7C, 0x1E, 0x83, 0xFE, 0x62, 0x28, 0x06, 0x1E, 0xC1, 0xFE, 0x64, 0x20, 0x06,
            0x7B, 0xE2, 0x0C, 0x3E, 0x87, 0xE2, 0xF0, 0x42, 0x90, 0xE0, 0x42, 0x15, 0x20, 0xD2, 0x05, 0x20,
            0x4F, 0x16, 0x20, 0x18, 0xCB, 0x4F, 0x06, 0x04, 0xC5, 0xCB, 0x11, 0x17, 0xC1, 0xCB, 0x11, 0x17,
            0x05, 0x20, 0xF5, 0x22, 0x23, 0x22, 0x23, 0xC9, 0xCE, 0xED, 0x66, 0x66, 0xCC, 0x0D, 0x00, 0x0B,
            0x03, 0x73, 0x00, 0x83, 0x00, 0x0C, 0x00, 0x0D, 0x00, 0x08, 0x11, 0x1F, 0x88, 0x89, 0x00, 0x0E,
            0xDC, 0xCC, 0x6E, 0xE6, 0xDD, 0xDD, 0xD9, 0x99, 0xBB, 0xBB, 0x67, 0x63, 0x6E, 0x0E, 0xEC, 0xCC,
            0xDD, 0xDC, 0x99, 0x9F, 0xBB, 0xB9, 0x33, 0x3E, 0x3C, 0x42, 0xB9, 0xA5, 0xB9, 0xA5, 0x42, 0x3C,
            0x21, 0x04, 0x01, 0x11, 0xA8, 0x00, 0x1A, 0x13, 0xBE, 0x20, 0xFE, 0x23, 0x7D, 0xFE, 0x34, 0x20,
            0xF5, 0x06, 0x19, 0x78, 0x86, 0x23, 0x05, 0x20, 0xFB, 0x86, 0x20, 0xFE, 0x3E, 0x01, 0xE0, 0x50
        };
    }
}