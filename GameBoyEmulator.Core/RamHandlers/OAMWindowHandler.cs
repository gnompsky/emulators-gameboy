namespace GameBoyEmulator.Core.RamHandlers
{
    public class OAMWindowHandler : RamWindowHandler
    {
        public OAMWindowHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
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
        private static bool CanAccess(ushort address)
        {
            var statMode = Ram.STAT_Mode;
            if (statMode is not (Modes.Oam or Modes.Vram)) return true;

            return statMode == Modes.Vram && address is < 0xFE69 or > 0xFE6B;
        }
    }
}