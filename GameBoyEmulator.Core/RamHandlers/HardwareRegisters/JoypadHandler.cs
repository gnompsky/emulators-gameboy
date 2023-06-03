namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class JoypadHandler : RamWindowHandler
    {
        private enum Mode { Action, Direction, None }
        private Mode _mode = Mode.None;
        
        public bool UpPressed { private get; set; }
        public bool DownPressed { private get; set; }
        public bool LeftPressed { private get; set; }
        public bool RightPressed { private get; set; }

        public bool StartPressed { private get; set; }
        public bool SelectPressed { private get; set; }
        public bool APressed { private get; set; }
        public bool BPressed { private get; set; }
        
        public JoypadHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }

        public override byte ReadValue(ushort _)
        {
            switch (_mode)
            {
                case Mode.Action:
                    // Action buttons selected
                    return ((byte)0b00011111).AdjustByte(
                        overrideBit3: !StartPressed,
                        overrideBit2: !SelectPressed,
                        overrideBit1: !BPressed,
                        overrideBit0: !APressed
                    );
                case Mode.Direction:
                    // Direction buttons selected
                    return ((byte)0b00101111).AdjustByte(
                        overrideBit3: !DownPressed,
                        overrideBit2: !UpPressed,
                        overrideBit1: !LeftPressed,
                        overrideBit0: !RightPressed
                    );
                case Mode.None:
                default:
                    return 0b00111111;
            }
        }

        public override void WriteValue(ushort _, byte requestedValue)
        {
            if (!requestedValue.BitIsSet(5))
            {
                _mode = Mode.Action;
            }
            else if (!requestedValue.BitIsSet(4))
            {
                _mode = Mode.Direction;
            }
            else
            {
                _mode = Mode.None;
            }
        }
    }
}