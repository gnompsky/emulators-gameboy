namespace GameBoyEmulator.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string ToHex(this byte b, bool withPrefix = false) 
            => $"{(withPrefix ? "0x" : string.Empty)}{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
        public static string ToHex(this ushort u, bool withPrefix = false) 
            => $"{(withPrefix ? "0x" : string.Empty)}{Convert.ToString(u, 16).PadLeft(4, '0').ToUpperInvariant()}";
    }
}