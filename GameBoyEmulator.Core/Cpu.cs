namespace GameBoyEmulator.Core
{
    public static class Cpu
    {
        public static bool Running = false;
        public static bool StopMode = false;

        public static void Step()
        {
            if (!Running || StopMode) return;
            
            var instruction = Ram.GetNextN();
            Instructions.Execute(instruction);
            
            Ram.HardwareRegisters.Timers.TickAll();
        }
        
        public static void Reset()
        {
            Running = false;
            StopMode = false;
            Registers.Reset();
            Clock.Reset();
            Ram.Reset();
            Running = true;
        }
    }
}