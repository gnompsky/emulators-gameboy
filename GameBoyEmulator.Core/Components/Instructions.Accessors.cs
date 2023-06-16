namespace GameBoyEmulator.Core.Components
{
    public partial class Instructions
    {
        private byte GetA(ref int _) => _registers.A;
        private byte GetB(ref int _) => _registers.B;
        private byte GetC(ref int _) => _registers.C;
        private byte GetD(ref int _) => _registers.D;
        private byte GetE(ref int _) => _registers.E;
        private byte GetH(ref int _) => _registers.H;
        private byte GetL(ref int _) => _registers.L;

        private ushort GetBC(ref int _) => _registers.BC;
        private ushort GetDE(ref int _) => _registers.DE;
        private ushort GetHL(ref int _) => _registers.HL;
        private ushort GetAF(ref int _) => _registers.AF;
        private byte GetHLI(ref int cycles) => GetIndirect(GetHL)(ref cycles);
        private Getter<byte> GetIndirect(Getter<ushort> addressGetter) 
            => (ref int cycles) => _memory.GetN(addressGetter(ref cycles), ref cycles);

        private ushort GetPC(ref int _) => _registers.PC;
        private ushort GetSP(ref int _) => _registers.SP;

        private static Getter<bool> Not(Getter<bool> condition) => (ref int cycles) => !condition(ref cycles);
        private bool GetZero(ref int _) => _registers.IsZero;
        private bool GetSubtract(ref int _) => _registers.IsSubtract;
        private bool GetaHalfCarry(ref int _) => _registers.IsHalfCarry;
        private bool GetCarry(ref int _) => _registers.IsCarry;

        private void SetA(byte value, ref int _) => _registers.A = value;
        private void SetB(byte value, ref int _) => _registers.B = value;
        private void SetC(byte value, ref int _) => _registers.C = value;
        private void SetD(byte value, ref int _) => _registers.D = value;
        private void SetE(byte value, ref int _) => _registers.E = value;
        private void SetH(byte value, ref int _) => _registers.H = value;
        private void SetL(byte value, ref int _) => _registers.L = value;

        private void IncA(ref int cycles) => IncN(SetA, GetA, ref cycles);
        private void IncB(ref int cycles) => IncN(SetB, GetB, ref cycles);
        private void IncC(ref int cycles) => IncN(SetC, GetC, ref cycles);
        private void IncD(ref int cycles) => IncN(SetD, GetD, ref cycles);
        private void IncE(ref int cycles) => IncN(SetE, GetE, ref cycles);
        private void IncH(ref int cycles) => IncN(SetH, GetH, ref cycles);
        private void IncL(ref int cycles) => IncN(SetL, GetL, ref cycles);

        private void IncN(Setter<byte> setter, Getter<byte> getter, ref int cycles)
        {
            var newVal = getter(ref cycles).WrappingAdd(1, out var halfCarried);
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = false;
            _registers.IsHalfCarry = halfCarried;
            setter(newVal, ref cycles);
        }

        private ExecuteDelegate AddN(Getter<byte> amountGetter) => AddN(SetA, GetA, amountGetter);

        private ExecuteDelegate AddN(Setter<byte> valueSetter, Getter<byte> valueGetter, Getter<byte> amountGetter) =>
            (ref int cycles) =>
            {
                // TODO: This is almost certainly wrong :(
                var newVal = valueGetter(ref cycles).WrappingAdd(amountGetter(ref cycles), out var halfCarried);
                _registers.IsZero = newVal == 0;
                _registers.IsSubtract = false;
                _registers.IsCarry = (newVal & 0xFF00) != 0;
                _registers.IsHalfCarry = halfCarried;
                valueSetter(newVal, ref cycles);
            };

        private ExecuteDelegate AddNN(Getter<ushort> amountGetter) => AddNN(SetHL, GetHL, amountGetter);

        private ExecuteDelegate AddNN(Setter<ushort> valueSetter, Getter<ushort> valueGetter,
            Getter<byte> amountGetter) =>
            AddNN(valueSetter, valueGetter, (ref int cycles) => (ushort)amountGetter(ref cycles));
        private ExecuteDelegate AddNN(Setter<ushort> valueSetter, Getter<ushort> valueGetter,
            Getter<ushort> amountGetter) => (ref int cycles) =>
        {
            var newVal = valueGetter(ref cycles).WrappingAdd(amountGetter(ref cycles), out var halfCarried);

            // Zero is left alone
            _registers.IsSubtract = false;
            _registers.IsCarry = (newVal & 0xFFFF0000) != 0;
            _registers.IsHalfCarry = halfCarried;
            valueSetter(newVal, ref cycles);
        };
        
        private ExecuteDelegate AdcN(Getter<byte> amountGetter) =>
            AddN((ref int cycles) => (byte)(amountGetter(ref cycles) + (_registers.IsCarry ? 1 : 0)));

        private void DecA(ref int cycles) => DecN(SetA, GetA, ref cycles);
        private void DecB(ref int cycles) => DecN(SetB, GetB, ref cycles);
        private void DecC(ref int cycles) => DecN(SetC, GetC, ref cycles);
        private void DecD(ref int cycles) => DecN(SetD, GetD, ref cycles);
        private void DecE(ref int cycles) => DecN(SetE, GetE, ref cycles);
        private void DecH(ref int cycles) => DecN(SetH, GetH, ref cycles);
        private void DecL(ref int cycles) => DecN(SetL, GetL, ref cycles);

        private void DecN(Setter<byte> setter, Getter<byte> getter, ref int cycles)
        {
            var newVal = Maths.WrappingSubtract(getter(ref cycles), 1, out var halfCarried);
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = true;
            _registers.IsHalfCarry = halfCarried;
            setter(newVal, ref cycles);
        }
        
        private ExecuteDelegate SubN(Getter<byte> amountGetter) => (ref int cycles) =>
        {
            // TODO: This is almost certainly wrong :(
            var curVal = _registers.A;
            var amount = amountGetter(ref cycles);
            var newVal = curVal.WrappingSubtract(amount, out var halfCarried);
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = true;
            _registers.IsCarry = amount > curVal;
            _registers.IsHalfCarry = halfCarried;
            _registers.A = newVal;
        };

        private ExecuteDelegate SbcN(Getter<byte> amountGetter) =>
            SubN((ref int cycles) => (byte)(amountGetter(ref cycles) + (_registers.IsCarry ? 1 : 0)));

        private ExecuteDelegate And(Getter<byte> getter) => (ref int cycles) =>
        {
            var newVal = (byte)(_registers.A & getter(ref cycles));
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = false;
            _registers.IsHalfCarry = true;
            _registers.IsCarry = false;
            _registers.A = newVal;
        };

        private ExecuteDelegate Xor(Getter<byte> getter) => (ref int cycles) =>
        {
            var newVal = (byte)(_registers.A ^ getter(ref cycles));
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = false;
            _registers.IsHalfCarry = false;
            _registers.IsCarry = false;
            _registers.A = newVal;
        };
        private ExecuteDelegate Or(Getter<byte> getter) => (ref int cycles) =>
        {
            var newVal = (byte)(_registers.A | getter(ref cycles));
            _registers.IsZero = newVal == 0;
            _registers.IsSubtract = false;
            _registers.IsHalfCarry = false;
            _registers.IsCarry = false;
            _registers.A = newVal;
        };

        private void SetBC(ushort value, ref int _) => _registers.BC = value;
        private void SetDE(ushort value, ref int _) => _registers.DE = value;
        private void SetHL(ushort value, ref int _) => _registers.HL = value;
        private void SetAF(ushort value, ref int _) => _registers.AF = value;
        private void SetHLI(byte value, ref int cycles) => SetIndirect(GetHL, (ref int _) => value)(ref cycles);

        private ExecuteDelegate SetIndirect(Getter<ushort> addressGetter, Getter<byte> valueGetter)
            => (ref int cycles) => _memory.SetN(addressGetter(ref cycles), valueGetter(ref cycles), ref cycles);
        private ExecuteDelegate SetIndirect(Getter<ushort> addressGetter, Getter<ushort> valueGetter)
            => (ref int cycles) => _memory.SetNN(addressGetter(ref cycles), valueGetter(ref cycles), ref cycles);
        
        private void IncBC(ref int cycles) => IncNN(SetBC, GetBC, ref cycles);
        private void IncDE(ref int cycles) => IncNN(SetDE, GetDE, ref cycles);
        private void IncHL(ref int cycles) => IncNN(SetHL, GetHL, ref cycles);
        private void IncSP(ref int cycles) => IncNN(SetSP, GetSP, ref cycles);

        private void IncNN(Setter<ushort> setter, Getter<ushort> getter, ref int cycles)
        {
            setter(Maths.WrappingAdd(getter(ref cycles), 1), ref cycles);
            cycles += 4;
        }

        private void DecBC(ref int cycles) => DecNN(SetBC, GetBC, ref cycles);
        private void DecDE(ref int cycles) => DecNN(SetDE, GetDE, ref cycles);
        private void DecHL(ref int cycles) => DecNN(SetHL, GetHL, ref cycles);
        private void DecSP(ref int cycles) => DecNN(SetSP, GetSP, ref cycles);

        private void DecNN(Setter<ushort> setter, Getter<ushort> getter, ref int cycles)
        {
            setter(Maths.WrappingSubtract(getter(ref cycles), 1), ref cycles);
            cycles += 4;
        }

        private void SetSP(ushort value, ref int _) => _registers.SP = value;
    }
}