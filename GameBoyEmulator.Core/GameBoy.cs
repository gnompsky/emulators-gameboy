using GameBoyEmulator.Core.Components;
using GameBoyEmulator.Core.DataTypes;

namespace GameBoyEmulator.Core
{
    public class GameBoy
    {
        public static readonly GameBoy Instance = new GameBoy();
        public Fifo<Colors> LcdPixels => _ppu.LcdFifo;

        private readonly Registers _registers;
        private readonly Memory _memory;
        private readonly Instructions _instructions;
        private readonly PPU _ppu;
        
        public bool StopMode = false;
        public bool HaltMode = false;

        private readonly StreamWriter logStream;

        private GameBoy()
        {
            _registers = new Registers();
            _memory = new Memory(_registers);
            _instructions = new Instructions(this, _registers, _memory);
            _ppu = new PPU(_memory);

            File.Delete("gb-log.txt");
            logStream = new StreamWriter(File.OpenWrite("gb-log.txt"));
        }
        
        public void Reset()
        {
            StopMode = false;
            HaltMode = false;
            _registers.Reset();
            _memory.Reset();
        }

        public void Step()
        {
            var cyclesTaken = 0;

            if (!StopMode || !HaltMode)
            {
                _instructions.ExecuteNext(logStream, ref cyclesTaken);
            }

            _memory.Step(cyclesTaken);
            _ppu.Step(cyclesTaken);
        }
        
        public void LoadRom(byte[] romBytes, bool skipBootstrap)
        {
            Reset();
            _memory.LoadROM(romBytes);

            if (skipBootstrap)
            {
                _registers.A = 0x01;
                _registers.F = 0xB0;
                _registers.B = 0x00;
                _registers.C = 0x13;
                _registers.D = 0x00;
                _registers.E = 0xD8;
                _registers.H = 0x01;
                _registers.L = 0x4D;
                _registers.SP = 0xFFFE;
                _registers.PC = 0x0100;
                var dummy = 0;
                _memory.SetN((ushort)0xFF50, (byte)0x01, ref dummy);
            }
        }
    }
}