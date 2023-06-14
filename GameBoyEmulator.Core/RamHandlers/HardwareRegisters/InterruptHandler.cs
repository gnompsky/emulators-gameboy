using GameBoyEmulator.Core.Components;

namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class InterruptHandler : RamWindowHandler
    {
        private const ushort IF_ADDR = 0xFF0F;
        private const ushort IE_ADDR = 0xFFFF;

        private readonly Registers _registers;

        public InterruptHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter, Registers registers)
            : base(valueGetter, valueSetter)
        {
            _registers = registers;
        }

        public void SetInterruptEnabled(Interrupt interrupt, bool enabled)
        {
            var curValue = ValueGetter(IE_ADDR);
            var newValue = enabled
                ? curValue.SetBit((int)interrupt)
                : curValue.UnsetBit((int)interrupt);

            ValueSetter(IE_ADDR, newValue);
        }

        public void RequestInterrupt(Interrupt interrupt)
        {
            var curValue = ValueGetter(IF_ADDR);
            if (curValue.BitIsSet((int)interrupt)) return;
            
            ValueSetter(IF_ADDR, curValue.SetBit((int)interrupt));
        }

        // TODO: When do we actually handle the interrupts
        public bool ConsumeInterruptIfRequested(Interrupt interrupt)
        {
            // An interrupt must have been requested and must also be enabled
            var shouldFire = _registers.IME
                             && ValueGetter(IF_ADDR).BitIsSet((int)interrupt)
                             && ValueGetter(IE_ADDR).BitIsSet((int)interrupt);

            if (shouldFire)
            {
                ValueSetter(IF_ADDR, ValueGetter(IF_ADDR).UnsetBit((int)interrupt));
            }

            return shouldFire;
        }

        public enum Interrupt
        {
            VBlank = 0,
            STAT = 1,
            Timer = 2,
            // TODO: IMPLEMENT ME
            Serial = 3,
            Joypad = 4
        }
    }
}