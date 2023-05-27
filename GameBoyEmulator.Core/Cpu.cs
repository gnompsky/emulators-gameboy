using System.Diagnostics;

namespace GameBoyEmulator.Core
{
    public static class Cpu
    {
        public static bool Running = false;

        private static readonly Stopwatch Stopwatch = new Stopwatch();
        private const int MsPerCycle = (int)(1000 / 105000d);

        public static void Step()
        {
            if (!Running) return;
            
            var instruction = Ram.GetNextN();
            Instructions.Execute(instruction);
        }
        
        public static void Reset()
        {
            Running = false;
            Registers.Reset();
            Clock.Reset();
            Ram.Reset();
            Running = true;
        }
    }
}