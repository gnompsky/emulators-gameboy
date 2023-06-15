using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.RamHandlers
{
    public class OamHandler : RamWindowHandler
    {
        private readonly LcdHandler _lcdHandler;

        public OamHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter, LcdHandler lcdHandler) 
            : base(valueGetter, valueSetter)
        {
            _lcdHandler = lcdHandler;
        }
        
        public override byte ReadValue(ushort address)
        {
            return CanAccess(address) 
                ? ValueGetter(address) 
                : GarbageValue;
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            if (CanAccess(address)) ValueSetter(address, requestedValue);
        }

        /// <summary>
        /// OAM Mode means no access whatsoever.
        /// VRAM Mode means no access to 0xFE69-0xFE6B
        /// </summary>
        private bool CanAccess(ushort address)
        {
            var statMode = _lcdHandler.STATMode;
            if (statMode is not (Modes.Oam or Modes.Vram)) return true;

            return statMode == Modes.Vram && address is < 0xFE69 or > 0xFE6B;
        }
    }
}