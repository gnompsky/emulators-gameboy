namespace GameBoyEmulator.Core.RamHandlers.HardwareRegisters
{
    public class HWRegisterWindowHandler : RamWindowHandler
    {
        public readonly JoypadHandler Joypad;
        public readonly TimerAndDividerHandlers Timers;
        public readonly AudioHandler Audio;
        
        public HWRegisterWindowHandler(Func<ushort, byte> valueGetter, Action<ushort, byte> valueSetter)
            : base(valueGetter, valueSetter)
        {
            Joypad = new JoypadHandler(valueGetter, valueSetter);
            Timers = new TimerAndDividerHandlers(valueGetter, valueSetter);
            Audio = new AudioHandler(valueGetter, valueSetter);
        }

        public override byte ReadValue(ushort address)
        {
            switch (address)
            {
                case Addresses.JOYP:
                    return Joypad.ReadValue(address);
                case Addresses.DIV:
                case Addresses.TIMA:
                case Addresses.TMA:
                case Addresses.TAC:
                    return Timers.ReadValue(address);
                case Addresses.NR10:
                case Addresses.NR11:
                case Addresses.NR12:
                case Addresses.NR13:
                case Addresses.NR14:
                case Addresses.NR21:
                case Addresses.NR22:
                case Addresses.NR23:
                case Addresses.NR24:
                case Addresses.NR30:
                case Addresses.NR31:
                case Addresses.NR32:
                case Addresses.NR33:
                case Addresses.NR34:
                case Addresses.NR41:
                case Addresses.NR42:
                case Addresses.NR43:
                case Addresses.NR44:
                case Addresses.NR50:
                case Addresses.NR51:
                case Addresses.NR52:
                    return Audio.ReadValue(address);
                default:
                    return ValueGetter(address);
            }
        }

        public override void WriteValue(ushort address, byte requestedValue)
        {
            switch (address)
            {
                case Addresses.JOYP:
                    Joypad.WriteValue(address, requestedValue);
                    break;
                case Addresses.DIV:
                case Addresses.TIMA:
                case Addresses.TMA:
                case Addresses.TAC:
                    Timers.WriteValue(address, requestedValue);
                    break;
                
                case Addresses.NR10:
                case Addresses.NR11:
                case Addresses.NR12:
                case Addresses.NR13:
                case Addresses.NR14:
                case Addresses.NR21:
                case Addresses.NR22:
                case Addresses.NR23:
                case Addresses.NR24:
                case Addresses.NR30:
                case Addresses.NR31:
                case Addresses.NR32:
                case Addresses.NR33:
                case Addresses.NR34:
                case Addresses.NR41:
                case Addresses.NR42:
                case Addresses.NR43:
                case Addresses.NR44:
                case Addresses.NR50:
                case Addresses.NR51:
                case Addresses.NR52:
                    Audio.WriteValue(address, requestedValue);
                    break;
                // TODO: Add other registers
                default:
                    ValueSetter(address, requestedValue);
                    break;
            }
        }

        public static class Addresses
        {
            public const ushort JOYP = 0xFF00;
            public const ushort SB = 0xFF01;
            public const ushort SC = 0xFF02;
            public const ushort DIV = 0xFF04;
            public const ushort TIMA = 0xFF05;
            public const ushort TMA = 0xFF06;
            public const ushort TAC = 0xFF07;
            public const ushort IF = 0xFF0F;
            public const ushort NR10 = 0xFF10;
            public const ushort NR11 = 0xFF11;
            public const ushort NR12 = 0xFF12;
            public const ushort NR13 = 0xFF13;
            public const ushort NR14 = 0xFF14;
            public const ushort NR21 = 0xFF16;
            public const ushort NR22 = 0xFF17;
            public const ushort NR23 = 0xFF18;
            public const ushort NR24 = 0xFF19;
            public const ushort NR30 = 0xFF1A;
            public const ushort NR31 = 0xFF1B;
            public const ushort NR32 = 0xFF1C;
            public const ushort NR33 = 0xFF1D;
            public const ushort NR34 = 0xFF1E;
            public const ushort NR41 = 0xFF20;
            public const ushort NR42 = 0xFF21;
            public const ushort NR43 = 0xFF22;
            public const ushort NR44 = 0xFF23;
            public const ushort NR50 = 0xFF24;
            public const ushort NR51 = 0xFF25;
            public const ushort NR52 = 0xFF26;
            public const ushort WaveRAM = 0xFF30;
            public const ushort LCDC = 0xFF40;
            public const ushort STAT = 0xFF41;
            public const ushort SCY = 0xFF42;
            public const ushort SCX = 0xFF43;
            public const ushort LY = 0xFF44;
            public const ushort LYC = 0xFF45;
            public const ushort DMA = 0xFF46;
            public const ushort BGP = 0xFF47;
            public const ushort OBP0 = 0xFF48;
            public const ushort OBP1 = 0xFF49;
            public const ushort WY = 0xFF4A;
            public const ushort WX = 0xFF4B;
            public const ushort IE = 0xFFFF;
        }
    }
}