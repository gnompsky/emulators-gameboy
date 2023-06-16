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

        private GameBoy()
        {
            _registers = new Registers();
            _memory = new Memory(_registers);
            _instructions = new Instructions(this, _registers, _memory);
            _ppu = new PPU(_memory);
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
                _instructions.ExecuteNext(ref cyclesTaken);
            }

            _memory.Step(cyclesTaken);
            _ppu.Step(cyclesTaken);
        }
        
        public void LoadRom(byte[] romBytes)
        {
            _memory.LoadROM(romBytes);
            Reset();
        }
    }
}