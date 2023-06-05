namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class LcdHandler : RamWindowHandler
    {
        public LcdHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }
        
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
        
        private const ushort STAT_ADDR = 0xFF41;
        private const ushort LY_ADDR = 0xFF44;
        private const ushort LYC_ADDR = 0xFF45;
        
        
        private void UpdateLYCEqualsLYFlag()
        {
            // Go direct to memory instead of via STAT property to avoid looping back into here
            ValueSetter(
                STAT_ADDR,
                LY == LYC
                    ? Maths.SetBit(2, STAT)
                    : Maths.UnsetBit(2, STAT)
            );
        }
    }
}