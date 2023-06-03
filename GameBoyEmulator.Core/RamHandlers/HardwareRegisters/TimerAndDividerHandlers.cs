using System.Diagnostics;

namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class TimerAndDividerHandlers : RamWindowHandler
    {
        private ulong _cycleDivLastTicked;
        private ulong _cycleTimaLastTicked;
        
        public TimerAndDividerHandlers(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            switch (address)
            {
                case HWRegisterWindowHandler.Addresses.DIV:
                    ValueSetter(address, 0);
                    break;
                case HWRegisterWindowHandler.Addresses.TAC:
                    ValueSetter(address, requestedValue);
                    break;
            }
        }

        public void TickAll()
        {
            var currentCycle = Clock.Cycle;

            // 256 Cycles (4194304 Hz) occur per DIV tick (16384 Hz)
            var cyclesSinceDivTick = currentCycle - _cycleDivLastTicked;
            var ticksDivOverdueBy = (int)cyclesSinceDivTick - 256;
            if (ticksDivOverdueBy >= 0)
            {
                var newDiv = ValueGetter(HWRegisterWindowHandler.Addresses.DIV).WrappingAdd(1);
                ValueSetter(HWRegisterWindowHandler.Addresses.DIV, newDiv);
                _cycleDivLastTicked = currentCycle - (ulong)ticksDivOverdueBy;
            }

            // Cycles per TIMA tick depends on TAC value
            var (timaEnabled, cyclesPerTick) = ParseTAC();
            if (!timaEnabled) return;
            
            var cyclesSinceTimaTick = currentCycle - _cycleTimaLastTicked;
            var ticksTimaOverDueBy = (int)cyclesSinceTimaTick - cyclesPerTick;
            if (ticksTimaOverDueBy >= 0)
            {
                var result = ValueGetter(HWRegisterWindowHandler.Addresses.TIMA) + 1;
                
                // On overflow we reset to TMA and fire interrupt
                if (result > 0xFF)
                {
                    ValueSetter(HWRegisterWindowHandler.Addresses.TIMA, ValueGetter(HWRegisterWindowHandler.Addresses.TMA));
                    Interrupts.Fire(Interrupts.Addresses.Timer);
                }
                else
                {
                    ValueSetter(HWRegisterWindowHandler.Addresses.TIMA, (byte)result);
                }

                _cycleTimaLastTicked = currentCycle - (ulong)ticksTimaOverDueBy;
            }
        }

        private (bool timaEnabled, int cyclesPerTick) ParseTAC()
        {
            var tac = ReadValue(HWRegisterWindowHandler.Addresses.TAC);

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