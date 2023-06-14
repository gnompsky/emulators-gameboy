namespace GameBoyEmulator.Core.DataTypes
{
    public class Tile
    {
        public readonly IReadOnlyCollection<Colors> Colors;
        
        public Tile(ref byte[] sixteenBytes)
        {
            var colors = new Colors[8*8];
            var pixel = 0;
            
            for (var i = 0; i < 16; i += 2)
            {
                var lsb = sixteenBytes[i];
                var msb = sixteenBytes[i + 1];

                for (var bit = 0; bit < 8; bit++)
                {
                    colors[pixel++] = GetPixelColor(lsb, msb, bit);
                }
            }

            Colors = colors;
        }

        private static Colors GetPixelColor(byte lsb, byte msb, int bit) =>
            (Colors)((byte)0b0).AdjustByte(
                overrideBit1: msb.BitIsSet(bit),
                overrideBit0: lsb.BitIsSet(bit)
            );
    }
}