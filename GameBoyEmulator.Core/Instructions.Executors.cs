namespace GameBoyEmulator.Core
{
    public static partial class Instructions
    {
        private static Action LdN(Action<byte> valueSetter, Func<byte> valueGetter) => () =>
        {
            valueSetter(valueGetter());
        };
        
        private static Action LdIndirectN(Func<ushort> addressGetter, Func<byte> valueGetter) => () =>
        {
            Ram.SetN(addressGetter(), valueGetter());
        };
        
        private static Action LdIndirectN(Func<byte> addressGetter, Func<byte> valueGetter) => () =>
        {
            Ram.SetN((ushort)(0xFF00 + addressGetter()), valueGetter());
        };

        private static Action LdNN(Action<ushort> valueSetter, Func<ushort> valueGetter) => () =>
        {
            valueSetter(valueGetter());
        };

        private static Action JumpRelative(Func<sbyte> valueGetter) => JumpRelativeIf(() => true, valueGetter);
        
        private static Action JumpRelativeIf(Func<bool> condition, Func<sbyte> valueGetter) => () =>
        {
            // We need to read the value regardless to move our PC on past the operand
            var r8 = valueGetter();

            if (!condition()) return;

            Registers.PC = r8 < 0
                ? Maths.WrappingSubtract(Registers.PC, (ushort)Math.Abs(r8))
                : Maths.WrappingAdd(Registers.PC, (ushort)r8);
        };
        
        private static Action Jump(Func<ushort> valueGetter) => JumpIf(() => true, valueGetter);
        
        private static Action JumpIf(Func<bool> condition, Func<ushort> valueGetter) => () =>
        {
            if (condition()) Registers.PC = valueGetter();
        };

        private static Action Call(Func<ushort> addressGetter) => CallIf(() => true, addressGetter);

        private static Action CallIf(Func<bool> condition, Func<ushort> addressGetter) => () =>
        {
            if (!condition()) return;

            Push(Getters.GetPC)();
            Jump(addressGetter)();
        };

        private static Action Push(Func<ushort> valueGetter) => () =>
        {
            Maths.SplitShortTo(valueGetter(), out var lower, out var upper);

            Setters.DecSP();
            Ram.SetN(Getters.GetSP(), upper);

            Setters.DecSP();
            Ram.SetN(Getters.GetSP(), lower);
        };

        private static Action ReturnIf(Func<bool> condition) => () =>
        {
            if (!condition()) return;

            Pop(val => Registers.PC = val)();
        };

        private static Action Pop(Action<ushort> valueSetter) => () =>
        {
            var lower = Ram.GetN(Getters.GetSP());
            Setters.IncSP();
            
            var upper = Ram.GetN(Getters.GetSP());
            Setters.IncSP();

            valueSetter(Maths.CombineBytes(lower, upper));
        };

        private static Action Bit(int bit, Func<byte> valueGetter) => () =>
        {
            Registers.IsZero = !Maths.BitIsSet(bit, valueGetter());
            Registers.IsSubtract = false;
            Registers.IsHalfCarry = true;
        };

        private static Action Set(int bit, Func<byte> valueGetter, Action<byte> valueSetter) => () => valueSetter(Maths.SetBit(bit, valueGetter()));
        private static Action Unset(int bit, Func<byte> valueGetter, Action<byte> valueSetter) => () => valueSetter(Maths.UnsetBit(bit, valueGetter()));

        private static Action Compare(Func<byte> valueGetter) => () =>
        {
            var value = valueGetter();

            Registers.IsZero = Registers.A == value;
            Registers.IsSubtract = true;
            Registers.IsHalfCarry = (value & 0x0f) > (Registers.A & 0x0f);
            Registers.IsCarry = value > Registers.A;
        };
        
        private static Action RotateRight(Func<byte> valueGetter, Action<byte> valueSetter) => Rotate(true, false, valueGetter, valueSetter);
        private static Action RotateRightThroughCarry(Func<byte> valueGetter, Action<byte> valueSetter) => Rotate(true, true, valueGetter, valueSetter);
        private static Action RotateLeft(Func<byte> valueGetter, Action<byte> valueSetter) => Rotate(false, false, valueGetter, valueSetter);
        private static Action RotateLeftThroughCarry(Func<byte> valueGetter, Action<byte> valueSetter) => Rotate(false, true, valueGetter, valueSetter);
        private static Action Rotate(bool rotateRight, bool throughCarry, Func<byte> valueGetter, Action<byte> valueSetter) => () =>
        {
            valueSetter(Maths.RotateByte(
                valueGetter(),
                rotateRight,
                throughCarry,
                Registers.IsCarry,
                out var didCarry
            ));
            Registers.IsCarry = didCarry;
        };
    }
}