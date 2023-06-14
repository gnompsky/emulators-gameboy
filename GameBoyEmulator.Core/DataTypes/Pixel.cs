namespace GameBoyEmulator.Core.DataTypes
{
    public struct Pixel
    {
        public readonly Colors Color;
        public readonly Palettes Palette;
        public readonly BackgroundPriorities BackgroundPriority;

        public Pixel(Colors color, Palettes palette, BackgroundPriorities backgroundPriority)
        {
            Color = color;
            Palette = palette;
            BackgroundPriority = backgroundPriority;
        }

        public enum Palettes : byte
        {
            OBP0 = 0b0,
            OBP1 = 0b1
        }

        public enum BackgroundPriorities : byte
        {
            ObjFirst = 0b0,
            BgAndWindowFirst = 0b1
        }
    }
}