using GameBoyEmulator.Core.DataTypes;
using GameBoyEmulator.Core.RamHandlers;
using GameBoyEmulator.Core.RamHandlers.HardwareRegisters;

namespace GameBoyEmulator.Core.Components
{
    public class Memory
    {
        public readonly VRamHandler VRamHandler;
        public readonly OamHandler OamHandler;
        public readonly InterruptHandler InterruptHandler;
        public readonly TimerAndDividerHandler TimerAndDividerHandler;
        public readonly LcdHandler LcdHandler;
        
        private readonly byte[] _memory = new byte[0xFFFF + 1];
        private readonly RangeList<ushort, RamWindowHandler> _windowHandlers;

        public Memory(Registers registers)
        {
            byte ReadByte(ushort address) => _memory[address];
            void WriteByte(ushort address, byte value) => _memory[address] = value;

            InterruptHandler = new InterruptHandler(ReadByte, WriteByte, registers);
            TimerAndDividerHandler = new TimerAndDividerHandler(ReadByte, WriteByte, InterruptHandler);
            LcdHandler = new LcdHandler(ReadByte, WriteByte, InterruptHandler);
            VRamHandler = new VRamHandler(ReadByte, WriteByte, LcdHandler);
            OamHandler = new OamHandler(ReadByte, WriteByte, LcdHandler);

            _windowHandlers = new RangeList<ushort, RamWindowHandler>
            {
                { (0x0000, 0x3FFF), new RomBank00Handler(ReadByte) },
                { (0x4000, 0x7FFF), new RomBankNNHandler(ReadByte) },
                { (0x8000, 0x9FFF), VRamHandler },
                // { (0xA000, 0xBFFF), new ExternalRamHandler(ReadByte) },
                { (0xC000, 0xFDFF), new WRamHandler(ReadByte, WriteByte, 0xE000) },
                { (0xFE00, 0xFE9F), OamHandler },
                { (0xFEA0, 0xFEFF), new ProhibitedAddressHandler() },
                { (0xFF00, 0xFF00), new JoypadHandler(ReadByte, WriteByte) },
                { (0xFF01, 0xFF02), new SerialHandler(ReadByte, WriteByte) },
                { (0xFF04, 0xFF07), TimerAndDividerHandler },
                { (0xFF10, 0xFF26), new AudioHandler(ReadByte, WriteByte) },
                { (0xFF30, 0xFF3F), new WavePatternHandler(ReadByte, WriteByte) },
                { (0xFF40, 0xFF4B), LcdHandler },
                { (0xFF80, 0xFFFE), new HRamHandler(ReadByte, WriteByte) },
                { (0xFF0F, 0xFF0F), InterruptHandler },
                { (0xFFFF, 0xFFFF), InterruptHandler },
            };
        }

        public void Reset()
        {
            Array.Fill(_memory, (byte)0x00);
            TimerAndDividerHandler.Reset();
        }

        public void Step(int deltaCycles)
        {
            TimerAndDividerHandler.Step(deltaCycles);
        }

        public void LoadROM(byte[] romBytes)
        {
            // TODO: This needs work
            Array.Copy(romBytes, _memory, Math.Min(romBytes.Length, _memory.Length));
        }
        
        public byte GetN(ushort address, ref int cycles)
        {
            cycles += 4;

            return _windowHandlers.TryGetValue(address, out var handler)
                ? handler.ReadValue(address)
                : _memory[address];
        }

        public ushort GetNN(ushort address, ref int cycles) => Maths.CombineBytes(GetN(address, ref cycles), GetN((ushort)(address + 1), ref cycles));

        public void SetNN(ushort address, ushort value, ref int cycles)
        {
            value.SplitToBytes(out var lower, out var upper);
            SetN(address, lower, ref cycles);
            SetN((ushort)(address + 1), upper, ref cycles);
        }

        public void SetN(ushort address, byte value, ref int cycles)
        {
            cycles += 4;
            
            if (_windowHandlers.TryGetValue(address, out var handler))
            {
                handler.WriteValue(address, value);
                return;
            }

            _memory[address] = value;
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