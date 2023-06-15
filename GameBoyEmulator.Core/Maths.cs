namespace GameBoyEmulator.Core
{
    public static class Maths
    {
        public static byte WrappingAdd(this byte a, byte b) => a.WrappingAdd(b, out _);

        public static byte WrappingAdd(this byte a, byte b, out bool halfCarried)
        {
            halfCarried = (((a & 0xf) + (b & 0xf)) & 0x10) == 0x10;
            return unchecked((byte)(a + b));
        }
        public static byte WrappingSubtract(this byte a, byte b) => a.WrappingSubtract(b, out _);

        public static byte WrappingSubtract(this byte a, byte b, out bool halfCarried)
        {
            halfCarried = (((a & 0xf) - (b & 0xf)) & 0x10) == 0x10;
            return unchecked((byte)(a - b));
        }

        public static ushort WrappingAdd(this ushort aa, ushort bb) => unchecked((ushort)(aa + bb));

        public static ushort WrappingSubtract(this ushort aa, ushort bb) => unchecked((ushort)(aa - bb));

        public static ushort CombineBytes(byte lower, byte upper) => (ushort)((upper << 8) + lower);

        public static void SplitToBytes(this ushort value, out byte lower, out byte upper)
        {
            lower = (byte)(value & 0x00FF);
            upper = (byte)((value & 0xFF00) >> 8);
        }
        
        public static byte RotateByte(
            this byte value,
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

        public static bool BitIsSet(this byte value, int bit) => ((byte)(1 << bit) & value) != 0x00;
        public static byte SetBit(this byte value, int bit) => (byte)(value | (1 << bit));
        public static byte UnsetBit(this byte value, int bit) => (byte)(value & unchecked((byte)~(1 << bit)));

        public static byte AdjustByte(
            this byte value,
            bool? overrideBit7 = null, 
            bool? overrideBit6 = null,
            bool? overrideBit5 = null,
            bool? overrideBit4 = null,
            bool? overrideBit3 = null,
            bool? overrideBit2 = null,
            bool? overrideBit1 = null,
            bool? overrideBit0 = null
        )
        {
            return value
                .OverrideBit(7, overrideBit7)
                .OverrideBit(6, overrideBit6)
                .OverrideBit(5, overrideBit5)
                .OverrideBit(4, overrideBit4)
                .OverrideBit(3, overrideBit3)
                .OverrideBit(2, overrideBit2)
                .OverrideBit(1, overrideBit1)
                .OverrideBit(0, overrideBit0);

        }
        
        private static byte OverrideBit(this byte value, int bit, bool? overrideBit)
        {
            if (!overrideBit.HasValue) return value;
            return overrideBit.Value ? value.SetBit(bit) : value.UnsetBit(bit);
        }
    }
}