namespace GameBoyEmulator.Core.Components
{
    public partial class Instructions
    {
        private static ExecuteDelegate LdN(Setter<byte> valueSetter, Getter<byte> valueGetter) =>
            (ref int cycles) =>
            {
                valueSetter(valueGetter(ref cycles), ref cycles);
            };

        private ExecuteDelegate LdIndirectN(Getter<ushort> addressGetter, Getter<byte> valueGetter) =>
            (ref int cycles) =>
            {
                _memory.SetN(addressGetter(ref cycles), valueGetter(ref cycles), ref cycles);
            };

        private ExecuteDelegate LdIndirectN(Getter<byte> addressGetter, Getter<byte> valueGetter) =>
            (ref int cycles) =>
            {
                _memory.SetN((ushort)(0xFF00 + addressGetter(ref cycles)), valueGetter(ref cycles), ref cycles);
            };

        private ExecuteDelegate LdNN(Setter<ushort> valueSetter, Getter<ushort> valueGetter) => 
            (ref int cycles) =>
            {
                valueSetter(valueGetter(ref cycles), ref cycles);
            };

        private ExecuteDelegate JumpRelative(Getter<sbyte> valueGetter) => 
            JumpRelativeIf((ref int _) => true, valueGetter);
        
        private ExecuteDelegate JumpRelativeIf(Getter<bool> condition, Getter<sbyte> valueGetter) => 
            (ref int cycles) =>
        {
            // We need to read the value regardless to move our PC on past the operand
            var r8 = valueGetter(ref cycles);

            if (!condition(ref cycles)) return;

            _registers.PC = r8 < 0
                ? _registers.PC.WrappingSubtract((ushort)Math.Abs(r8))
                : _registers.PC.WrappingAdd((ushort)r8);
        };
        
        private ExecuteDelegate Jump(Getter<ushort> valueGetter) => JumpIf((ref int _) => true, valueGetter);

        private ExecuteDelegate JumpIf(Getter<bool> condition, Getter<ushort> valueGetter) =>
            (ref int cycles) =>
            {
                if (condition(ref cycles)) _registers.PC = valueGetter(ref cycles);
            };

        private ExecuteDelegate Call(Getter<ushort> addressGetter) => CallIf((ref int _) => true, addressGetter);

        private ExecuteDelegate CallIf(Getter<bool> condition, Getter<ushort> addressGetter) =>
            (ref int cycles) =>
            {
                if (!condition(ref cycles)) return;

                Push(GetPC)(ref cycles);
                Jump(addressGetter)(ref cycles);
            };

        private ExecuteDelegate Push(Getter<ushort> valueGetter) =>
            (ref int cycles) =>
            {
                valueGetter(ref cycles).SplitToBytes(out var lower, out var upper);

                DecSP(ref cycles);
                _memory.SetN(GetSP(ref cycles), upper, ref cycles);

                DecSP(ref cycles);
                _memory.SetN(GetSP(ref cycles), lower, ref cycles);
            };

        private ExecuteDelegate ReturnIf(Getter<bool> condition) =>
            (ref int cycles) =>
            {
                if (!condition(ref cycles)) return;

                Pop((ushort val, ref int _) => _registers.PC = val)(ref cycles);
            };

        private ExecuteDelegate Pop(Setter<ushort> valueSetter) => 
            (ref int cycles) =>
        {
            var lower = _memory.GetN(GetSP(ref cycles), ref cycles);
            IncSP(ref cycles);
            
            var upper = _memory.GetN(GetSP(ref cycles), ref cycles);
            IncSP(ref cycles);

            valueSetter(Maths.CombineBytes(lower, upper), ref cycles);
        };

        private ExecuteDelegate Bit(int bit, Getter<byte> valueGetter) => 
            (ref int cycles) =>
        {
            _registers.IsZero = !valueGetter(ref cycles).BitIsSet(bit);
            _registers.IsSubtract = false;
            _registers.IsHalfCarry = true;
        };

        private ExecuteDelegate Set(int bit, Getter<byte> valueGetter, Setter<byte> valueSetter) => 
            (ref int cycles) => valueSetter(valueGetter(ref cycles).SetBit(bit), ref cycles);
        private ExecuteDelegate Unset(int bit, Getter<byte> valueGetter, Setter<byte> valueSetter) => 
            (ref int cycles) => valueSetter(valueGetter(ref cycles).UnsetBit(bit), ref cycles);

        private ExecuteDelegate Compare(Getter<byte> valueGetter) => 
            (ref int cycles) =>
        {
            var value = valueGetter(ref cycles);

            _registers.IsZero = _registers.A == value;
            _registers.IsSubtract = true;
            _registers.IsHalfCarry = (value & 0x0f) > (_registers.A & 0x0f);
            _registers.IsCarry = value > _registers.A;
        };
        
        private ExecuteDelegate RotateRight(Getter<byte> valueGetter, Setter<byte> valueSetter) 
            => Rotate(true, false, valueGetter, valueSetter);
        private ExecuteDelegate RotateRightThroughCarry(Getter<byte> valueGetter, Setter<byte> valueSetter) 
            => Rotate(true, true, valueGetter, valueSetter);
        private ExecuteDelegate RotateLeft(Getter<byte> valueGetter, Setter<byte> valueSetter) 
            => Rotate(false, false, valueGetter, valueSetter);
        private ExecuteDelegate RotateLeftThroughCarry(Getter<byte> valueGetter, Setter<byte> valueSetter) 
            => Rotate(false, true, valueGetter, valueSetter);

        private ExecuteDelegate Rotate(bool rotateRight, bool throughCarry, Getter<byte> valueGetter,
            Setter<byte> valueSetter)
            => (ref int cycles) =>
            {
                valueSetter(Maths.RotateByte(
                    valueGetter(ref cycles),
                    rotateRight,
                    throughCarry,
                    _registers.IsCarry,
                    out var didCarry
                ), ref cycles);
                _registers.IsCarry = didCarry;
            };
    }
}