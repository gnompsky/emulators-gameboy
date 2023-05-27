// See https://aka.ms/new-console-template for more information

using GameBoyEmulator.Core;

string ToHexN(byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
string ToHexNN(ushort w) => $"0x{Convert.ToString(w, 16).PadLeft(4, '0').ToUpperInvariant()}";
string ToBinaryN(byte b) => Convert.ToString(b, 2).PadLeft(8, '0')[..4];

Instructions.InstructionExecuting += Console.WriteLine;

Cpu.Reset();

while (true)
{
    Cpu.Step();

    string spValue = "";
    try
    {
        spValue = ToHexNN(Maths.CombineBytes(Ram.GetN(Registers.SP), Ram.GetN((ushort)(Registers.SP + 1))));
        Clock.Cycle -= 8;
    }
    catch (IndexOutOfRangeException) {}

    Console.WriteLine($" PC: {ToHexNN(Registers.PC)} SP: {ToHexNN(Registers.SP)} ({spValue}), A: {ToHexN(Registers.A)}, B: {ToHexN(Registers.B)}, C: {ToHexN(Registers.C)}, D: {ToHexN(Registers.D)}, E: {ToHexN(Registers.E)}, H: {ToHexN(Registers.H)}, L: {ToHexN(Registers.L)}, F: {ToBinaryN(Registers.F)}");
    //Thread.Sleep(1000);
}