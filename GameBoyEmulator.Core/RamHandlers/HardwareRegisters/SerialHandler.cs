namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class SerialHandler : RamWindowHandler
    {
        public SerialHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }
        
        // TODO: Serial transfer
    }
}