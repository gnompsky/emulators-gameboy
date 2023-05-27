namespace GameBoyEmulator.Core
{
    public static class Registers
    {
        public static byte A;
        public static byte B;
        public static byte C;
        public static byte D;
        public static byte E;
        public static byte F;
        public static byte H;
        public static byte L;
        
        public static ushort SP;
        public static ushort PC;

        public static bool IME;

        public static void Reset()
        {
            A = B = C = D = E = F = H = L = 0x00;
            SP = PC = 0x0000;
        }

        #region Combined Registers
        public static ushort AF
        {
            get => Maths.CombineBytes(F, A);
            set => Maths.SplitShortTo(value, out F, out A);
        }
        public static ushort BC
        {
            get => Maths.CombineBytes(C, B);
            set => Maths.SplitShortTo(value, out C, out B);
        }
        public static ushort DE
        {
            get => Maths.CombineBytes(E, D);
            set => Maths.SplitShortTo(value, out E, out D);
        }
        public static ushort HL
        {
            get => Maths.CombineBytes(L, H);
            set => Maths.SplitShortTo(value, out L, out H);
        }
        #endregion

        #region Flags
        public static bool IsZero
        {
            get => GetFlag(7);
            set => SetFlag(7, value);
        }
        public static bool IsSubtract
        {
            get => GetFlag(6);
            set => SetFlag(6, value);
        }
        public static bool IsHalfCarry
        {
            get => GetFlag(5);
            set => SetFlag(5, value);
        }
        public static bool IsCarry
        {
            get => GetFlag(4);
            set => SetFlag(4, value);
        }
        private static bool GetFlag(int bit) => (F & (1 << bit)) != 0;
        private static void SetFlag(int bit, bool value) => F = (byte)(value ? F | (1 << bit) : F & ~(1 << bit));
        #endregion
    }
}