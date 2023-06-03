namespace GameBoyEmulator.Core
{
    public static class Events
    {
        public delegate void InstructionExecutingHandler(string instructionName);
        public delegate void SerialCharacterWrittenHandler(byte character);
        public delegate void GpuPixelsUpdatedHandler(byte[] pixels);
        public delegate void AudioChannelTriggeredHandler(int channel, int pan, byte[] data);
    }
}