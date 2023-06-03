namespace GameBoyEmulator.Core.RamHandlers
{
    public abstract class RomWindowHandler : RamWindowHandler
    {
        protected RomWindowHandler(Func<ushort, byte> valueGetter) : base(valueGetter, NoOp)
        {
        }
        
        private static void NoOp(ushort _, byte __)
        {
        }
        
        public sealed override void WriteValue(ushort address, byte requestedValue)
        {
            // Do nothing, writes are not allowed
        }
    }
}