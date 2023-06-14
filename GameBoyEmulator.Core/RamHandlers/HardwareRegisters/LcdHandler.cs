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

        public bool LCDCLcdEnabled => ValueGetter(LCDC_ADDR).BitIsSet(7);
        public ushort LCDCWindowTileMapArea => (ushort)(ValueGetter(LCDC_ADDR).BitIsSet(6) && LCDCWindowEnabled ? 0x9C00 : 0x9800);
        public bool LCDCWindowEnabled => ValueGetter(LCDC_ADDR).BitIsSet(5);

        public bool LCDCBgAndWindowTileDataAreaUsesSignedAddress => !ValueGetter(LCDC_ADDR).BitIsSet(4);
        /// <summary>
        /// This will either be 0x8000 in which case we're using normal addressing, or 0x9000 in which case our
        /// window starts at 0x8800 and we used signed addressing starting at a base of 0x9000 
        /// </summary>
        public ushort LCDCBgAndWindowTileDataArea(bool? usesSignedAddress = null) 
            => (ushort)(usesSignedAddress ?? LCDCBgAndWindowTileDataAreaUsesSignedAddress ? 0x9000 : 0x8000);

        public ushort LCDCBgTileMapArea => (ushort)(ValueGetter(LCDC_ADDR).BitIsSet(3) && !LCDCWindowEnabled ? 0x9C00 : 0x9800);
        public int LCDCObjSize => (ushort)(ValueGetter(LCDC_ADDR).BitIsSet(2) ? 16 : 8);
        public bool LCDCObjEnabled => ValueGetter(LCDC_ADDR).BitIsSet(1);
        public bool LCDCBgAndWindowPriority => ValueGetter(LCDC_ADDR).BitIsSet(0);
        
        public byte STAT
        {
            get => ValueGetter(STAT_ADDR);
            set
            {
                ValueSetter(STAT_ADDR, value);
                UpdateLYCEqualsLYFlag();
            }
        }
        public Modes STAT_Mode
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
                var alreadyInterrupting = ShouldFireInterruptForMode(STAT_Mode, newValue);
                var fireInterrupt = !alreadyInterrupting && ShouldFireInterruptForMode(value, newValue);

                STAT = newValue.AdjustByte(
                    overrideBit1: value is Modes.Oam or Modes.Vram,
                    overrideBit0: value is Modes.Vram or Modes.VBlank
                );
                
                if (fireInterrupt) _interruptHandler.RequestInterrupt(InterruptHandler.Interrupt.STAT);
            }
        }

        public byte SCY => ValueGetter(SCY_ADDR);
        public byte SCX => ValueGetter(SCX_ADDR);
        
        public byte LY
        {
            get => ValueGetter(LY_ADDR);
            set
            {
                ValueSetter(LY_ADDR, value);
                UpdateLYCEqualsLYFlag();
            }
        }
        
        public byte LYC
        {
            get => ValueGetter(LYC_ADDR);
            set
            {
                ValueSetter(LYC_ADDR, value);
                UpdateLYCEqualsLYFlag();
            }
        }
        
        public byte WY => ValueGetter(WY_ADDR);
        public byte WX => ValueGetter(WX_ADDR);
        
        private const ushort LCDC_ADDR = 0xFF40;
        private const ushort STAT_ADDR = 0xFF41;
        private const ushort SCY_ADDR = 0xFF42;
        private const ushort SCX_ADDR = 0xFF43;
        private const ushort LY_ADDR = 0xFF44;
        private const ushort LYC_ADDR = 0xFF45;
        private const ushort WY_ADDR = 0xFF4A;
        private const ushort WX_ADDR = 0xFF4B;

        private static bool ShouldFireInterruptForMode(Modes mode, byte currentStat) =>
            (mode == Modes.HBlank && currentStat.BitIsSet(3))
            || (mode == Modes.VBlank && currentStat.BitIsSet(4))
            || (mode == Modes.Oam && currentStat.BitIsSet(5));
        
        private void UpdateLYCEqualsLYFlag()
        {
            // Go direct to memory instead of via STAT property to avoid looping back into here
            ValueSetter(
                STAT_ADDR,
                STAT.AdjustByte(overrideBit2: LY == LYC)
            );
        }
    }
}