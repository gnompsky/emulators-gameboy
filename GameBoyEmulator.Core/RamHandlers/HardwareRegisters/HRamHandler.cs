namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class HRamHandler : RamWindowHandler
    {
        public HRamHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }
    }
}