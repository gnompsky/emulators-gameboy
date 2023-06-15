namespace GameBoyEmulator.Core.Extensions
{
    public static class ByteExtensions
    {
        public static string ToHex(this byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
        public static string ToHex(this ushort u) => $"0x{Convert.ToString(u, 16).PadLeft(4, '0').ToUpperInvariant()}";
    }
}