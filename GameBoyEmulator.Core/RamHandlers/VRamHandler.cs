using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.RamHandlers
{
    public class VRamHandler : RamWindowHandler
    {
        private readonly LcdHandler _lcdHandler;

        private bool AccessDenied => _lcdHandler.STATMode == Modes.Vram;

        public VRamHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter, LcdHandler lcdHandler) 
            : base(valueGetter, valueSetter)
        {
            _lcdHandler = lcdHandler;
        }
        
        public override byte ReadValue(ushort address)
        {
            return AccessDenied ? GarbageValue : ValueGetter(address);
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            if (!AccessDenied) ValueSetter(address, requestedValue);
        }
    }
}