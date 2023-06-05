using GameBoyEmulator.Core.Components;

namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class TimerAndDividerHandler : RamWindowHandler
    {
        private int _cyclesSinceLastDiv;
        private int _cyclesSinceLastTima;

        public TimerAndDividerHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }

        public void ClearDiv()
        {
            ValueSetter(Memory.Addresses.DIV, 0);
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            switch (address)
            {
                case Memory.Addresses.DIV:
                    ValueSetter(address, 0);
                    break;
                case Memory.Addresses.TAC:
                    ValueSetter(address, requestedValue);
                    break;
            }
        }

        public void Reset()
        {
            _cyclesSinceLastDiv = 0;
            _cyclesSinceLastTima = 0;
        }

        public void Step(int deltaCycles)
        {
            // 256 Cycles (4194304 Hz) occur per DIV tick (16384 Hz)
            _cyclesSinceLastDiv += deltaCycles;
            var ticksDivOverdueBy = _cyclesSinceLastDiv - 256;
            if (ticksDivOverdueBy >= 0)
            {
                var newDiv = ValueGetter(Memory.Addresses.DIV).WrappingAdd(1);
                ValueSetter(Memory.Addresses.DIV, newDiv);
                _cyclesSinceLastDiv = ticksDivOverdueBy;
            }

            // Cycles per TIMA tick depends on TAC value
            var (timaEnabled, cyclesPerTick) = ParseTAC();
            if (!timaEnabled) return;
            
            _cyclesSinceLastTima += deltaCycles;
            var ticksTimaOverDueBy = _cyclesSinceLastTima - cyclesPerTick;
            if (ticksTimaOverDueBy >= 0)
            {
                var result = ValueGetter(Memory.Addresses.TIMA) + 1;
                
                // On overflow we reset to TMA and fire interrupt
                if (result > 0xFF)
                {
                    ValueSetter(Memory.Addresses.TIMA, ValueGetter(Memory.Addresses.TMA));
                    // TODO: Fire interrupt
                    //Interrupts.Fire(Interrupts.Addresses.Timer);
                }
                else
                {
                    ValueSetter(Memory.Addresses.TIMA, (byte)result);
                }

                _cyclesSinceLastTima = ticksTimaOverDueBy;
            }
        }

        private (bool timaEnabled, int cyclesPerTick) ParseTAC()
        {
            var tac = ReadValue(Memory.Addresses.TAC);

            var bit0 = tac.BitIsSet(0);
            var bit1 = tac.BitIsSet(1);
            var bit2 = tac.BitIsSet(2);

            int cyclesPerTick;
            if (bit0)
            {
                cyclesPerTick = bit1 ? 256 : 16;
            }
            else
            {
                cyclesPerTick = bit1 ? 64 : 1024;
            }

            return (timaEnabled: bit2, cyclesPerTick);
        }
    }
}