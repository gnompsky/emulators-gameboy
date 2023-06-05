namespace GameBoyEmulator.Core.RamHandlers
{
    public class ProhibitedAddressHandler : RamWindowHandler
    {
        public ProhibitedAddressHandler() : base(_ => 0x0, (_, _) => { })
        {
        }

        public override byte ReadValue(ushort address)
        {
            throw new AccessViolationException();
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            throw new AccessViolationException();
        }
    }
}