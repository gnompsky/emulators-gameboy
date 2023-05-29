namespace GameBoyEmulator.Core
{
    public static class Maths
    {
        public static byte WrappingAdd(byte a, byte b) => WrappingAdd(a, b, out _);

        public static byte WrappingAdd(byte a, byte b, out bool halfCarried)
        {
            halfCarried = (((a & 0xf) + (b & 0xf)) & 0x10) == 0x10;
            return unchecked((byte)(a + b));
        }
        public static byte WrappingSubtract(byte a, byte b) => WrappingSubtract(a, b, out _);

        public static byte WrappingSubtract(byte a, byte b, out bool halfCarried)
        {
            halfCarried = (((a & 0xf) - (b & 0xf)) & 0x10) == 0x10;
            return unchecked((byte)(a - b));
        }

        public static ushort WrappingAdd(ushort aa, ushort bb) => unchecked((ushort)(aa + bb));

        public static ushort WrappingSubtract(ushort aa, ushort bb) => unchecked((ushort)(aa - bb));

        public static ushort CombineBytes(byte lower, byte upper) => (ushort)((upper << 8) + lower);

        public static void SplitShortTo(ushort value, out byte lower, out byte upper)
        {
            lower = (byte)(value & 0x00FF);
            upper = (byte)((value & 0xFF00) >> 8);
        }
        
        public static byte RotateByte(
            byte value,
            bool rotateRight,
            bool throughCarry,
            bool currentCarry,
            out bool didCarry
        )
        {
            var carryAmount = (byte)(value & 0x01);
            didCarry = carryAmount != 0;

            if (rotateRight) value >>= 1;
            else value <<= 1;

            if (throughCarry) value += (byte)((currentCarry ? 1 : 0) << 7);
            else if (didCarry) value |= 0x80;

            return value;
        }

        public static bool BitIsSet(int bit, byte value) => ((byte)(1 << bit) & value) != 0x00;
        public static byte SetBit(int bit, byte value) => (byte)(value | (1 << bit));
        public static byte UnsetBit(int bit, byte value) => (byte)(value & unchecked((byte)~(1 << bit)));
    }
}