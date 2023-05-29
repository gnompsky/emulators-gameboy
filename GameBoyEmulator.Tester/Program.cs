// See https://aka.ms/new-console-template for more information

using GameBoyEmulator.Core;

string _lastInstructionName = string.Empty;
Instructions.InstructionExecuting += name => _lastInstructionName = name;

Cpu.Reset();

var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/cpu_instrs.gb");
//var romBytes = File.ReadAllBytes("../../../../GameBoyEmulator.Tests/ROMs/Tetris (World) (Rev 1).gb");
Ram.LoadROM(romBytes);
Ram.SerialCharacterWritten += b => Console.Write((char)b);

while (true)
{
    Cpu.Step();
    Gpu.Step();

    DebugPrint();
    //Thread.Sleep(1000);
}

void DebugPrint()
{
    string ToHexN(byte b) => $"0x{Convert.ToString(b, 16).PadLeft(2, '0').ToUpperInvariant()}";
    string ToHexNN(ushort w) => $"0x{Convert.ToString(w, 16).PadLeft(4, '0').ToUpperInvariant()}";
    string ToBinaryN(byte b) => Convert.ToString(b, 2).PadLeft(8, '0')[..4];

    if (Registers.PC >= 0x6B)
    {
        var spValue = "";
        try
        {
            spValue = ToHexNN(Ram.GetNN(Registers.SP));
            Clock.Cycle -= 8;
        }
        catch (IndexOutOfRangeException) {}
        
        Console.WriteLine(
            $"{_lastInstructionName.PadRight(19)} - PC: {ToHexNN(Registers.PC)}, LY: {ToHexNN(Ram.LY)}, SP: {ToHexNN(Registers.SP)} ({spValue}), A: {ToHexN(Registers.A)}, B: {ToHexN(Registers.B)}, C: {ToHexN(Registers.C)}, D: {ToHexN(Registers.D)}, E: {ToHexN(Registers.E)}, H: {ToHexN(Registers.H)}, L: {ToHexN(Registers.L)}, F: {ToBinaryN(Registers.F)}");
    }
    
    if (_lastInstructionName.StartsWith("0x00E9") && !Registers.IsZero) throw new ApplicationException("Cart check failed. Exiting");
}