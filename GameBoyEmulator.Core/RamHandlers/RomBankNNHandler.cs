namespace GameBoyEmulator.Core.RamHandlers
{
    public class RomBankNNHandler : RomWindowHandler
    {
        public RomBankNNHandler(Func<ushort, byte> valueGetter) 
            : base(valueGetter)
        {
        }
        
        // TODO: Handle MBCs? Currently works like NoMBC (https://gbdev.io/pandocs/nombc.html)
        
    }
}