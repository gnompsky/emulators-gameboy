using GameBoyEmulator.Core.Components;

namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class AudioHandler : RamWindowHandler
    {
        public readonly MemoryStream AudioStream = new MemoryStream();
        private ulong _cycleOfLastSample;
        
        private byte NR52 {
            get
            {
                var ch1On = ValueGetter(Memory.Addresses.NR14).BitIsSet(7);
                var ch2On = ValueGetter(Memory.Addresses.NR24).BitIsSet(7);
                var ch3On = ValueGetter(Memory.Addresses.NR34).BitIsSet(7);
                var ch4On = ValueGetter(Memory.Addresses.NR44).BitIsSet(7);

                var nr52 = ((byte)0x0).AdjustByte(
                    overrideBit7: _apuEnabled,
                    overrideBit3: ch4On,
                    overrideBit2: ch3On,
                    overrideBit1: ch2On,
                    overrideBit0: ch1On
                );
                ValueSetter(Memory.Addresses.NR52, nr52);
                return nr52;
            }
            set
            {
                _apuEnabled = value.BitIsSet(7); 
                ValueSetter(Memory.Addresses.NR52, NR52);
            }
        }
        
        private bool _apuEnabled = true;
        
        
        public AudioHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter) 
            : base(valueGetter, valueSetter)
        {
        }

        public override byte ReadValue(ushort address)
        {
            // If APU is disabled, all values except NR52 are cleared so we can just return 0
            if (!_apuEnabled && address != Memory.Addresses.NR52)
            {
                return 0x0;
            }
            
            switch (address)
            {
                case Memory.Addresses.NR52:
                    return NR52;
                default:
                    return ValueGetter(address);
            }
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            // If APU is disabled, all values except NR52 are read-only, so bail out here
            if (!_apuEnabled && address != Memory.Addresses.NR52) return;

            switch (address)
            {
                case Memory.Addresses.NR52:
                    NR52 = requestedValue;
                    break;
                case Memory.Addresses.NR14:
                case Memory.Addresses.NR24:
                case Memory.Addresses.NR34:
                case Memory.Addresses.NR44:
                    ValueSetter(address, requestedValue);
                    if (requestedValue.BitIsSet(7))
                    {
                        //TriggerAudioChannel(address);
                    }

                    break;
                default:
                    // TODO: I assume theres more magic to implement for other registers
                    ValueSetter(address, requestedValue);
                    break;
            }
        }

        public void TickAudio()
        {
            var cyclesSinceLastSample = Clock.Cycle - _cycleOfLastSample;
            var cyclesOverdueBy = (int)cyclesSinceLastSample - 87;
            
            // TODO: Take sample
            // AudioStream.Position = 0;
            // AudioStream.Write(new[]
            // {
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            //     (byte)Clock.Cycle,
            // });
            

            _cycleOfLastSample = Clock.Cycle - (ulong)cyclesOverdueBy;
        }

        // private void TriggerAudioChannel(ushort address)
        // {
        //     int channel = address switch
        //     {
        //         HWRegisterWindowHandler.Addresses.NR14 => 1,
        //         HWRegisterWindowHandler.Addresses.NR24 => 2,
        //         HWRegisterWindowHandler.Addresses.NR34 => 3,
        //         HWRegisterWindowHandler.Addresses.NR44 => 4,
        //         _ => throw new ArgumentOutOfRangeException(nameof(channel))
        //     };
        //     var panByte = ValueGetter(HWRegisterWindowHandler.Addresses.NR51);
        //     int pan = address switch
        //     {
        //         HWRegisterWindowHandler.Addresses.NR14 => (panByte.BitIsSet(0) ? 1 : 0) - (panByte.BitIsSet(4) ? 1 : 0),
        //         HWRegisterWindowHandler.Addresses.NR24 => (panByte.BitIsSet(1) ? 1 : 0) - (panByte.BitIsSet(5) ? 1 : 0),
        //         HWRegisterWindowHandler.Addresses.NR34 => (panByte.BitIsSet(2) ? 1 : 0) - (panByte.BitIsSet(6) ? 1 : 0),
        //         HWRegisterWindowHandler.Addresses.NR44 => (panByte.BitIsSet(3) ? 1 : 0) - (panByte.BitIsSet(7) ? 1 : 0),
        //         _ => throw new ArgumentOutOfRangeException(nameof(channel))
        //     };
        //
        //     AudioStream.Position = 0;
        //     AudioStream.Write(new[]
        //     {
        //         ValueGetter((ushort)(address - 4)),
        //         ValueGetter((ushort)(address - 3)),
        //         ValueGetter((ushort)(address - 2)),
        //         ValueGetter((ushort)(address - 1)),
        //         ValueGetter(address),
        //         ValueGetter((ushort)(address - 4)),
        //         ValueGetter((ushort)(address - 3)),
        //         ValueGetter((ushort)(address - 2)),
        //         ValueGetter((ushort)(address - 1)),
        //         ValueGetter(address)
        //     });
        // }
    }
}