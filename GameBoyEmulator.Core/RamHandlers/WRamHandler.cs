namespace GameBoyEmulator.Core.RamHandlers
{
    public class WRamHandler : RamWindowHandler
    {
        private readonly ushort _mirrorStartAddress;
        
        public WRamHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter, ushort mirrorStartAddress) 
            : base(valueGetter, valueSetter)
        {
            _mirrorStartAddress = mirrorStartAddress;
        }

        public override byte ReadValue(ushort address)
        {
            if (address >= _mirrorStartAddress) address -= 2000;
            return base.ReadValue(address);
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            if (address >= _mirrorStartAddress) address -= 2000;
            base.WriteValue(address, requestedValue);
        }
    }
}