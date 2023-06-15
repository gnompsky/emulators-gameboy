using GameBoyEmulator.Core.DataTypes;

namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class LcdHandler : RamWindowHandler
    {
        private readonly InterruptHandler _interruptHandler;

        public LcdHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter,
            InterruptHandler interruptHandler) 
            : base(valueGetter, valueSetter)
        {
            _interruptHandler = interruptHandler;
        }

        public bool LCDCLcdEnabled => ValueGetter(LCDCAddr).BitIsSet(7);
        public ushort LCDCWindowTileMapArea => (ushort)(ValueGetter(LCDCAddr).BitIsSet(6) && LCDCWindowEnabled ? 0x9C00 : 0x9800);
        public bool LCDCWindowEnabled => ValueGetter(LCDCAddr).BitIsSet(5);

        public bool LCDCBgAndWindowTileDataAreaUsesSignedAddress => !ValueGetter(LCDCAddr).BitIsSet(4);
        /// <summary>
        /// This will either be 0x8000 in which case we're using normal addressing, or 0x9000 in which case our
        /// window starts at 0x8800 and we used signed addressing starting at a base of 0x9000 
        /// </summary>
        public ushort LCDCBgAndWindowTileDataArea(bool? usesSignedAddress = null) 
            => (ushort)(usesSignedAddress ?? LCDCBgAndWindowTileDataAreaUsesSignedAddress ? 0x9000 : 0x8000);

        public ushort LCDCBgTileMapArea => (ushort)(ValueGetter(LCDCAddr).BitIsSet(3) && !LCDCWindowEnabled ? 0x9C00 : 0x9800);
        public int LCDCObjSize => (ushort)(ValueGetter(LCDCAddr).BitIsSet(2) ? 16 : 8);
        public bool LCDCObjEnabled => ValueGetter(LCDCAddr).BitIsSet(1);
        public bool LCDCBgAndWindowPriority => ValueGetter(LCDCAddr).BitIsSet(0);

        public Modes STATMode
        {
            get
            {
                var registerValue = STAT;
                var bit1 = registerValue.BitIsSet(1);
                var bit0 = registerValue.BitIsSet(0);
                switch (bit0)
                {
                    case true: return bit1 ? Modes.Vram : Modes.VBlank;
                    case false: return bit1 ? Modes.Oam : Modes.HBlank;
                }
            }
            set
            {
                var newValue = STAT;

                // We need to check if we were previously interrupting. See STAT Blocking
                // https://gbdev.io/pandocs/Interrupt_Sources.html#int-48--stat-interrupt
                var alreadyInterrupting = ShouldFireInterruptForMode(STATMode, newValue);
                var fireInterrupt = !alreadyInterrupting && ShouldFireInterruptForMode(value, newValue);

                STAT = newValue.AdjustByte(
                    overrideBit1: value is Modes.Oam or Modes.Vram,
                    overrideBit0: value is Modes.Vram or Modes.VBlank
                );
                
                if (fireInterrupt) _interruptHandler.RequestInterrupt(InterruptHandler.Interrupt.STAT);
            }
        }

        private byte STAT
        {
            get => ValueGetter(STATAddr);
            set
            {
                ValueSetter(STATAddr, value);
                UpdateLYCEqualsLYFlag();
            }
        }

        public byte SCY => ValueGetter(SCYAddr);
        public byte SCX => ValueGetter(SCXAddr);
        
        public byte LY
        {
            get => ValueGetter(LYAddr);
            set
            {
                ValueSetter(LYAddr, value);
                UpdateLYCEqualsLYFlag();
            }
        }
        
        public byte LYC
        {
            get => ValueGetter(LYCAddr);
            set
            {
                ValueSetter(LYCAddr, value);
                UpdateLYCEqualsLYFlag();
            }
        }
        
        public Colors[] BGP {
            get
            {
                var colors = new Colors[4];
                var paletteByte = ValueGetter(BGPAddr);

                for (var bit = 0; bit < 8; bit += 2)
                {
                    colors[bit / 2] = (Colors)(
                        (paletteByte.BitIsSet(bit) ? 0b10 : 0b00) |
                        (paletteByte.BitIsSet(bit + 1) ? 0b01 : 0b00)
                    );
                }

                return colors;
            }
        }
        
        public byte WY => ValueGetter(WYAddr);
        public byte WX => ValueGetter(WXAddr);
        
        private const ushort LCDCAddr = 0xFF40;
        private const ushort STATAddr = 0xFF41;
        private const ushort SCYAddr = 0xFF42;
        private const ushort SCXAddr = 0xFF43;
        private const ushort LYAddr = 0xFF44;
        private const ushort LYCAddr = 0xFF45;
        private const ushort BGPAddr = 0xFF47;
        private const ushort WYAddr = 0xFF4A;
        private const ushort WXAddr = 0xFF4B;

        private static bool ShouldFireInterruptForMode(Modes mode, byte currentStat) =>
            (mode == Modes.HBlank && currentStat.BitIsSet(3))
            || (mode == Modes.VBlank && currentStat.BitIsSet(4))
            || (mode == Modes.Oam && currentStat.BitIsSet(5));
        
        private void UpdateLYCEqualsLYFlag()
        {
            // Go direct to memory instead of via STAT property to avoid looping back into here
            ValueSetter(
                STATAddr,
                STAT.AdjustByte(overrideBit2: LY == LYC)
            );
        }
    }
}