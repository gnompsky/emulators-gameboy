namespace GameBoyEmulator.Core.DataTypes
{
    public struct ObjPixel
    {
        public readonly Colors Color;
        public readonly ObjPalettes ObjPalette;
        public readonly BackgroundPriorities BackgroundPriority;

        public ObjPixel(Colors color, ObjPalettes objPalette, BackgroundPriorities backgroundPriority)
        {
            Color = color;
            ObjPalette = objPalette;
            BackgroundPriority = backgroundPriority;
        }

        public enum ObjPalettes : byte
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