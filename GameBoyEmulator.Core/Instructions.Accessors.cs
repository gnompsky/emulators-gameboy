namespace GameBoyEmulator.Core
{
    public static partial class Instructions
    {
        private static class Getters
        {
            public static byte GetA() => Registers.A;
            public static byte GetB() => Registers.B;
            public static byte GetC() => Registers.C;
            public static byte GetD() => Registers.D;
            public static byte GetE() => Registers.E;
            public static byte GetH() => Registers.H;
            public static byte GetL() => Registers.L;

            public static ushort GetBC() => Registers.BC;
            public static ushort GetDE() => Registers.DE;
            public static ushort GetHL() => Registers.HL;
            public static ushort GetAF() => Registers.AF;
            public static byte GetHLI() => GetIndirect(GetHL)();
            public static Func<byte> GetIndirect(Func<ushort> addressGetter) => () => Ram.GetN(addressGetter());

            public static ushort GetPC() => Registers.PC;
            public static ushort GetSP() => Registers.SP;

            public static Func<bool> Not(Func<bool> condition) => () => !condition();
            public static bool GetZero() => Registers.IsZero;
            public static bool GetSubtract() => Registers.IsSubtract;
            public static bool GetaHalfCarry() => Registers.IsHalfCarry;
            public static bool GetCarry() => Registers.IsCarry;
        }
        
        private static class Setters
        {
            public static void SetA(byte value) => Registers.A = value;
            public static void SetB(byte value) => Registers.B = value;
            public static void SetC(byte value) => Registers.C = value;
            public static void SetD(byte value) => Registers.D = value;
            public static void SetE(byte value) => Registers.E = value;
            public static void SetH(byte value) => Registers.H = value;
            public static void SetL(byte value) => Registers.L = value;

            public static void IncA() => IncN(SetA, Getters.GetA)();
            public static void IncB() => IncN(SetB, Getters.GetB)();
            public static void IncC() => IncN(SetC, Getters.GetC)();
            public static void IncD() => IncN(SetD, Getters.GetD)();
            public static void IncE() => IncN(SetE, Getters.GetE)();
            public static void IncH() => IncN(SetH, Getters.GetH)();
            public static void IncL() => IncN(SetL, Getters.GetL)();

            public static Action IncN(Action<byte> setter, Func<byte> getter) => () =>
            {
                var newVal = getter().WrappingAdd(1, out var halfCarried);
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = false;
                Registers.IsHalfCarry = halfCarried;
                setter(newVal);
            };

            public static Action AddN(Func<byte> amountGetter) => () =>
            {
                // TODO: This is almost certainly wrong :(
                var newVal = Registers.A.WrappingAdd(amountGetter(), out var halfCarried);
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = false;
                Registers.IsCarry = (newVal & 0xFF00) != 0;
                Registers.IsHalfCarry = halfCarried;
                Registers.A = newVal;
            };
            
            public static Action AdcN(Func<byte> amountGetter) =>
                AddN(() => (byte)(amountGetter() + (Registers.IsCarry ? 1 : 0)));

            public static void DecA() => DecN(SetA, Getters.GetA)();
            public static void DecB() => DecN(SetB, Getters.GetB)();
            public static void DecC() => DecN(SetC, Getters.GetC)();
            public static void DecD() => DecN(SetD, Getters.GetD)();
            public static void DecE() => DecN(SetE, Getters.GetE)();
            public static void DecH() => DecN(SetH, Getters.GetH)();
            public static void DecL() => DecN(SetL, Getters.GetL)();

            public static Action DecN(Action<byte> setter, Func<byte> getter) => () =>
            {
                var newVal = Maths.WrappingSubtract(getter(), 1, out var halfCarried);
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = true;
                Registers.IsHalfCarry = halfCarried;
                setter(newVal);
            };
            
            public static Action SubN(Func<byte> amountGetter) => () =>
            {
                // TODO: This is almost certainly wrong :(
                var curVal = Registers.A;
                var amount = amountGetter();
                var newVal = curVal.WrappingSubtract(amount, out var halfCarried);
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = true;
                Registers.IsCarry = amount > curVal;
                Registers.IsHalfCarry = halfCarried;
                Registers.A = newVal;
            };

            public static Action SbcN(Func<byte> amountGetter) =>
                SubN(() => (byte)(amountGetter() + (Registers.IsCarry ? 1 : 0)));

            public static Action And(Func<byte> getter) => () =>
            {
                var newVal = (byte)(Registers.A & getter());
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = false;
                Registers.IsHalfCarry = true;
                Registers.IsCarry = false;
                Registers.A = newVal;
            };

            public static Action Xor(Func<byte> getter) => () =>
            {
                var newVal = (byte)(Registers.A ^ getter());
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = false;
                Registers.IsHalfCarry = false;
                Registers.IsCarry = false;
                Registers.A = newVal;
            };
            public static Action Or(Func<byte> getter) => () =>
            {
                var newVal = (byte)(Registers.A | getter());
                Registers.IsZero = newVal == 0;
                Registers.IsSubtract = false;
                Registers.IsHalfCarry = false;
                Registers.IsCarry = false;
                Registers.A = newVal;
            };

            public static void SetBC(ushort value) => Registers.BC = value;
            public static void SetDE(ushort value) => Registers.DE = value;
            public static void SetHL(ushort value) => Registers.HL = value;
            public static void SetAF(ushort value) => Registers.AF = value;
            public static void SetHLI(byte value) => SetIndirect(Getters.GetHL, () => value);
            public static void SetIndirect(Func<ushort> addressGetter, Func<byte> valueGetter) => Ram.SetN(addressGetter(), valueGetter());
            
            public static void IncBC() => IncNN(SetBC, Getters.GetBC);
            public static void IncDE() => IncNN(SetDE, Getters.GetDE);
            public static void IncHL() => IncNN(SetHL, Getters.GetHL);
            public static void IncSP() => IncNN(SetSP, Getters.GetSP);

            private static void IncNN(Action<ushort> setter, Func<ushort> getter)
            {
                setter(Maths.WrappingAdd(getter(), 1));
                Clock.Cycle += 4;
            }
            
            public static void DecBC() => DecNN(SetBC, Getters.GetBC);
            public static void DecDE() => DecNN(SetDE, Getters.GetDE);
            public static void DecHL() => DecNN(SetHL, Getters.GetHL);
            public static void DecSP() => DecNN(SetSP, Getters.GetSP);
            private static void DecNN(Action<ushort> setter, Func<ushort> getter)
            {
                setter(Maths.WrappingSubtract(getter(), 1));
                Clock.Cycle += 4;
            }

            public static void SetSP(ushort value) => Registers.SP = value;
        }
    }
}