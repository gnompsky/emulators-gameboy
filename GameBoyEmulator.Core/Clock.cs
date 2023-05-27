namespace GameBoyEmulator.Core
{
    public static class Clock
    {
        public static ulong Cycle;

        public static void Reset()
        {
            Cycle = 0x0000;
        }
    }
}