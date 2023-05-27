namespace GameBoyEmulator.Core
{
    public static class Events
    {
        public delegate void InstructionExecutingHandler(string instructionName);
    }
}