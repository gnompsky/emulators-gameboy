namespace GameBoyEmulator.Core.Components
{
    public sealed class Registers
    {
        public byte A;
        public byte B;
        public byte C;
        public byte D;
        public byte E;
        public byte F;
        public byte H;
        public byte L;
        
        public ushort SP;
        public ushort PC;

        public bool IME;

        public void Reset()
        {
            A = B = C = D = E = F = H = L = 0x00;
            SP = PC = 0x0000;
        }

        #region Combined Registers
        public ushort AF
        {
            get => Maths.CombineBytes(F, A);
            set => value.SplitToBytes(out F, out A);
        }
        public ushort BC
        {
            get => Maths.CombineBytes(C, B);
            set => value.SplitToBytes(out C, out B);
        }
        public ushort DE
        {
            get => Maths.CombineBytes(E, D);
            set => value.SplitToBytes(out E, out D);
        }
        public ushort HL
        {
            get => Maths.CombineBytes(L, H);
            set => value.SplitToBytes(out L, out H);
        }
        #endregion

        #region Flags
        public bool IsZero
        {
            get => GetFlag(7);
            set => SetFlag(7, value);
        }
        public bool IsSubtract
        {
            get => GetFlag(6);
            set => SetFlag(6, value);
        }
        public bool IsHalfCarry
        {
            get => GetFlag(5);
            set => SetFlag(5, value);
        }
        public bool IsCarry
        {
            get => GetFlag(4);
            set => SetFlag(4, value);
        }
        private bool GetFlag(int bit) => (F & (1 << bit)) != 0;
        private void SetFlag(int bit, bool value) => F = (byte)(value ? F | (1 << bit) : F & ~(1 << bit));
        #endregion
    }
}