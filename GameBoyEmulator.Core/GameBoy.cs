using GameBoyEmulator.Core.Components;

namespace GameBoyEmulator.Core
{
    public class GameBoy
    {
        public static readonly GameBoy Instance = new GameBoy();

        private readonly Registers Registers;
        private readonly Memory Memory;
        private readonly Instructions Instructions;
        private readonly PPU PPU;
        
        public bool StopMode = false;

        public GameBoy()
        {
            Registers = new Registers();
            Memory = new Memory();
            Instructions = new Instructions(this, Registers, Memory);
            PPU = new PPU(Memory);
        }
        
        public void Reset()
        {
            StopMode = false;
            Registers.Reset();
            Memory.Reset();
        }

        public void Step()
        {
            var cyclesTaken = 0;

            if (!StopMode)
            {
                var instruction = Memory.GetN(Registers.PC, ref cyclesTaken);
                Instructions.Execute(instruction, ref cyclesTaken);
            }

            Memory.Step(cyclesTaken);
            PPU.Step();
        }
        
        public void LoadROM(byte[] romBytes)
        {
            Memory.LoadROM(romBytes);
            Reset();
        }
    }
}