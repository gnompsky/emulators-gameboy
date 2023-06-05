namespace GameBoyEmulator.Core
{
    public static class Events
    {
        public delegate void GpuPixelsUpdatedHandler(byte[] pixels);
    }
}