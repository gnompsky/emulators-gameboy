namespace GameBoyEmulator.Core
{
    public static class Interrupts
    {
        public static void Fire(byte interrupt)
        {
            // TODO: Implement interrupts
        }

        public static class Addresses
        {
            public const byte VBlank = 0x40;
            public const byte STAT = 0x48;
            public const byte Timer = 0x50;
            public const byte Serial = 0x58;
            public const byte Joypad = 0x60;
        }
    }
}