namespace GameBoyEmulator.Core.RamHandlers
{
    public abstract class RamWindowHandler
    {
        protected const byte GarbageValue = 0xFF;
        protected readonly Func<ushort, byte> ValueGetter;
        protected readonly Action<ushort, byte> ValueSetter;

        protected RamWindowHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter)
        {
            ValueGetter = valueGetter;
            ValueSetter = valueSetter;
        }

        public virtual byte ReadValue(ushort address) => ValueGetter(address);
        public virtual void WriteValue(ushort address, byte requestedValue) => ValueSetter(address, requestedValue);
    }
}