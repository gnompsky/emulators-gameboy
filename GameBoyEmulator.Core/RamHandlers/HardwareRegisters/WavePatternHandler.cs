namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class WavePatternHandler : RamWindowHandler
    {
        public WavePatternHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }
    }
}