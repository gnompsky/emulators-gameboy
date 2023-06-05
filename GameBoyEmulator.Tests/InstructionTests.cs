using System.Collections;
using GameBoyEmulator.Core;
using GameBoyEmulator.Core.Components;

namespace GameBoyEmulator.Tests;

public class InstructionTests
{
    [TestCaseSource(nameof(InstructionSizesAndTimings))]
    public void TestInstructionSizesAndTimings(byte instruction, byte expectedSize, byte expectedCycles)
    {
        // // Arrange
        // CPU.Reset();
        // var instructionName = "Unknown Instruction";
        // Instructions.InstructionExecuting += name => instructionName = name;
        // var initPC = Registers.PC++; //We ++ here to simulate reading the instruction as our expected values include the instruction byte itself
        // var initCycles = Clock.Cycle;
        // Clock.Cycle += 4; //We +=4 here to simulate reading the instruction as our expected values include the instruction byte itself
        //
        // // Act
        // try
        // {
        //     Instructions.Execute(instruction);
        // }
        // catch (NotImplementedException)
        // {
        //     Assert.Inconclusive($"{instructionName} Not Implemented");
        // }
        //
        // // Assert
        // Assert.Multiple(() =>
        // {
        //     Assert.That(Registers.PC - initPC, Is.EqualTo(expectedSize), $"{instructionName} Size was incorrect");
        //     Assert.That(Clock.Cycle - initCycles, Is.EqualTo(expectedCycles), $"{instructionName} Cycles Taken was incorrect");
        // });
    }
    
    [TestCaseSource(nameof(CbInstructionSizesAndTimings))]
    public void TestCbInstructionSizesAndTimings(byte instruction, byte expectedSize, byte expectedCycles)
    {
        // // Arrange
        // var instructions = new Instructions(null, null, null);
        // var initPC = Registers.PC++; //We ++ here to simulate reading the instruction as our expected values include the instruction byte itself
        // var initCycles = Clock.Cycle;
        // Clock.Cycle += 4; //We +=4 here to simulate reading the instruction as our expected values include the instruction byte itself
        // Ram.SetN(Registers.PC, instruction); // Load our next instruction in
        // Clock.Cycle -= 4; // Undo the time it took to load that instruction as we wouldn't actually be loading it here in real life, it would already be on the ROM
        //
        // // Act
        // try
        // {
        //     Instructions.Execute(0xCB); // Run the 0xCB instruction which in turn will then run the extension instruction loaded into RAM above
        // }
        // catch (NotImplementedException)
        // {
        //     Assert.Inconclusive($"{instructionName} Not Implemented");
        // }
        //
        // // Assert
        // Assert.Multiple(() =>
        // {
        //     Assert.That(Registers.PC - initPC, Is.EqualTo(expectedSize), $"{instructionName} Size was incorrect");
        //     Assert.That(Clock.Cycle - initCycles, Is.EqualTo(expectedCycles), $"{instructionName} Cycles Taken was incorrect");
        // });
    }

    public static IEnumerable InstructionSizesAndTimings()
    {
        byte[][] data =
        {
#region 0x0_

            new byte[] { 0x00, 1, 4 },
            new byte[] { 0x01, 3, 12 },
            new byte[] { 0x02, 1, 8 },
            new byte[] { 0x03, 1, 8 },
            new byte[] { 0x04, 1, 4 },
            new byte[] { 0x05, 1, 4 },
            new byte[] { 0x06, 2, 8 },
            new byte[] { 0x07, 1, 4 },
            new byte[] { 0x08, 3, 20 },
            new byte[] { 0x09, 1, 8 },
            new byte[] { 0x0A, 1, 8 },
            new byte[] { 0x0B, 1, 8 },
            new byte[] { 0x0C, 1, 4 },
            new byte[] { 0x0D, 1, 4 },
            new byte[] { 0x0E, 2, 8 },
            new byte[] { 0x0F, 1, 4 },

#endregion

#region 0x1_

            new byte[] { 0x10, 2, 4 },
            new byte[] { 0x11, 3, 12 },
            new byte[] { 0x12, 1, 8 },
            new byte[] { 0x13, 1, 8 },
            new byte[] { 0x14, 1, 4 },
            new byte[] { 0x15, 1, 4 },
            new byte[] { 0x16, 2, 8 },
            new byte[] { 0x17, 1, 4 },
            new byte[] { 0x18, 2, 12 },
            new byte[] { 0x19, 1, 8 },
            new byte[] { 0x1A, 1, 8 },
            new byte[] { 0x1B, 1, 8 },
            new byte[] { 0x1C, 1, 4 },
            new byte[] { 0x1D, 1, 4 },
            new byte[] { 0x1E, 2, 8 },
            new byte[] { 0x1F, 1, 4 },

#endregion

#region 0x2_

            new byte[] { 0x20, 2, 12 }, // TODO: Check. https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html says 12/8 cycles
            new byte[] { 0x21, 3, 12 },
            new byte[] { 0x22, 1, 8 },
            new byte[] { 0x23, 1, 8 },
            new byte[] { 0x24, 1, 4 },
            new byte[] { 0x25, 1, 4 },
            new byte[] { 0x26, 2, 8 },
            new byte[] { 0x27, 1, 4 },
            new byte[] { 0x28, 2, 12 }, // TODO: Check. https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html says 12/8 cycles
            new byte[] { 0x29, 1, 8 },
            new byte[] { 0x2A, 1, 8 },
            new byte[] { 0x2B, 1, 8 },
            new byte[] { 0x2C, 1, 4 },
            new byte[] { 0x2D, 1, 4 },
            new byte[] { 0x2E, 2, 8 },
            new byte[] { 0x2F, 1, 4 },

#endregion

#region 0x3_

            new byte[] { 0x30, 2, 12 }, // TODO: Check. https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html says 12/8 cycles
            new byte[] { 0x31, 3, 12 },
            new byte[] { 0x32, 1, 8 },
            new byte[] { 0x33, 1, 8 },
            new byte[] { 0x34, 1, 12 },
            new byte[] { 0x35, 1, 12 },
            new byte[] { 0x36, 2, 12 },
            new byte[] { 0x37, 1, 4 },
            new byte[] { 0x38, 2, 12 }, // TODO: Check. https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html says 12/8 cycles
            new byte[] { 0x39, 1, 8 },
            new byte[] { 0x3A, 1, 8 },
            new byte[] { 0x3B, 1, 8 },
            new byte[] { 0x3C, 1, 4 },
            new byte[] { 0x3D, 1, 4 },
            new byte[] { 0x3E, 2, 8 },
            new byte[] { 0x3F, 1, 4 },

#endregion

#region 0x4_

            new byte[] { 0x40, 1, 4 },
            new byte[] { 0x41, 1, 4 },
            new byte[] { 0x42, 1, 4 },
            new byte[] { 0x43, 1, 4 },
            new byte[] { 0x44, 1, 4 },
            new byte[] { 0x45, 1, 4 },
            new byte[] { 0x46, 1, 8 },
            new byte[] { 0x47, 1, 4 },
            new byte[] { 0x48, 1, 4 },
            new byte[] { 0x49, 1, 4 },
            new byte[] { 0x4A, 1, 4 },
            new byte[] { 0x4B, 1, 4 },
            new byte[] { 0x4C, 1, 4 },
            new byte[] { 0x4D, 1, 4 },
            new byte[] { 0x4E, 1, 8 },
            new byte[] { 0x4F, 1, 4 },

#endregion

#region 0x5_

            new byte[] { 0x50, 1, 4 },
            new byte[] { 0x51, 1, 4 },
            new byte[] { 0x52, 1, 4 },
            new byte[] { 0x53, 1, 4 },
            new byte[] { 0x54, 1, 4 },
            new byte[] { 0x55, 1, 4 },
            new byte[] { 0x56, 1, 8 },
            new byte[] { 0x57, 1, 4 },
            new byte[] { 0x58, 1, 4 },
            new byte[] { 0x59, 1, 4 },
            new byte[] { 0x5A, 1, 4 },
            new byte[] { 0x5B, 1, 4 },
            new byte[] { 0x5C, 1, 4 },
            new byte[] { 0x5D, 1, 4 },
            new byte[] { 0x5E, 1, 8 },
            new byte[] { 0x5F, 1, 4 },

#endregion

#region 0x6_

            new byte[] { 0x60, 1, 4 },
            new byte[] { 0x61, 1, 4 },
            new byte[] { 0x62, 1, 4 },
            new byte[] { 0x63, 1, 4 },
            new byte[] { 0x64, 1, 4 },
            new byte[] { 0x65, 1, 4 },
            new byte[] { 0x66, 1, 8 },
            new byte[] { 0x67, 1, 4 },
            new byte[] { 0x68, 1, 4 },
            new byte[] { 0x69, 1, 4 },
            new byte[] { 0x6A, 1, 4 },
            new byte[] { 0x6B, 1, 4 },
            new byte[] { 0x6C, 1, 4 },
            new byte[] { 0x6D, 1, 4 },
            new byte[] { 0x6E, 1, 8 },
            new byte[] { 0x6F, 1, 4 },

#endregion

#region 0x7_

            new byte[] { 0x70, 1, 8 },
            new byte[] { 0x71, 1, 8 },
            new byte[] { 0x72, 1, 8 },
            new byte[] { 0x73, 1, 8 },
            new byte[] { 0x74, 1, 8 },
            new byte[] { 0x75, 1, 8 },
            new byte[] { 0x76, 1, 4 },
            new byte[] { 0x77, 1, 8 },
            new byte[] { 0x78, 1, 4 },
            new byte[] { 0x79, 1, 4 },
            new byte[] { 0x7A, 1, 4 },
            new byte[] { 0x7B, 1, 4 },
            new byte[] { 0x7C, 1, 4 },
            new byte[] { 0x7D, 1, 4 },
            new byte[] { 0x7E, 1, 8 },
            new byte[] { 0x7F, 1, 4 },

#endregion

#region 0x8_

            new byte[] { 0x80, 1, 4 },
            new byte[] { 0x81, 1, 4 },
            new byte[] { 0x82, 1, 4 },
            new byte[] { 0x83, 1, 4 },
            new byte[] { 0x84, 1, 4 },
            new byte[] { 0x85, 1, 4 },
            new byte[] { 0x86, 1, 8 },
            new byte[] { 0x87, 1, 4 },
            new byte[] { 0x88, 1, 4 },
            new byte[] { 0x89, 1, 4 },
            new byte[] { 0x8A, 1, 4 },
            new byte[] { 0x8B, 1, 4 },
            new byte[] { 0x8C, 1, 4 },
            new byte[] { 0x8D, 1, 4 },
            new byte[] { 0x8E, 1, 8 },
            new byte[] { 0x8F, 1, 4 },

#endregion

#region 0x9_

            new byte[] { 0x90, 1, 4 },
            new byte[] { 0x91, 1, 4 },
            new byte[] { 0x92, 1, 4 },
            new byte[] { 0x93, 1, 4 },
            new byte[] { 0x94, 1, 4 },
            new byte[] { 0x95, 1, 4 },
            new byte[] { 0x96, 1, 8 },
            new byte[] { 0x97, 1, 4 },
            new byte[] { 0x98, 1, 4 },
            new byte[] { 0x99, 1, 4 },
            new byte[] { 0x9A, 1, 4 },
            new byte[] { 0x9B, 1, 4 },
            new byte[] { 0x9C, 1, 4 },
            new byte[] { 0x9D, 1, 4 },
            new byte[] { 0x9E, 1, 8 },
            new byte[] { 0x9F, 1, 4 },

#endregion

#region 0xA_

            new byte[] { 0xA0, 1, 4 },
            new byte[] { 0xA1, 1, 4 },
            new byte[] { 0xA2, 1, 4 },
            new byte[] { 0xA3, 1, 4 },
            new byte[] { 0xA4, 1, 4 },
            new byte[] { 0xA5, 1, 4 },
            new byte[] { 0xA6, 1, 8 },
            new byte[] { 0xA7, 1, 4 },
            new byte[] { 0xA8, 1, 4 },
            new byte[] { 0xA9, 1, 4 },
            new byte[] { 0xAA, 1, 4 },
            new byte[] { 0xAB, 1, 4 },
            new byte[] { 0xAC, 1, 4 },
            new byte[] { 0xAD, 1, 4 },
            new byte[] { 0xAE, 1, 8 },
            new byte[] { 0xAF, 1, 4 },

#endregion

#region 0xB_

            new byte[] { 0xB0, 1, 4 },
            new byte[] { 0xB1, 1, 4 },
            new byte[] { 0xB2, 1, 4 },
            new byte[] { 0xB3, 1, 4 },
            new byte[] { 0xB4, 1, 4 },
            new byte[] { 0xB5, 1, 4 },
            new byte[] { 0xB6, 1, 8 },
            new byte[] { 0xB7, 1, 4 },
            new byte[] { 0xB8, 1, 4 },
            new byte[] { 0xB9, 1, 4 },
            new byte[] { 0xBA, 1, 4 },
            new byte[] { 0xBB, 1, 4 },
            new byte[] { 0xBC, 1, 4 },
            new byte[] { 0xBD, 1, 4 },
            new byte[] { 0xBE, 1, 8 },
            new byte[] { 0xBF, 1, 4 },

#endregion

            // TODO: Implement rest of cases from https://www.pastraiser.com/cpu/gameboy/gameboy_opcodes.html
        };

        foreach (var testCase in data)
        {
            var testCaseData = new TestCaseData(testCase[0], testCase[1], testCase[2]);
            testCaseData.SetName($"Instruction 0x{Convert.ToString(testCase[0], 16).PadLeft(2, '0').ToUpperInvariant()}");
            yield return testCaseData;
        }
    }
    
    public static IEnumerable CbInstructionSizesAndTimings()
    {
        byte[][] data =
        {
#region 0x0_

            new byte[] { 0x00, 2, 8 },
            new byte[] { 0x01, 2, 8 },
            new byte[] { 0x02, 2, 8 },
            new byte[] { 0x03, 2, 8 },
            new byte[] { 0x04, 2, 8 },
            new byte[] { 0x05, 2, 8 },
            new byte[] { 0x06, 2, 16 },
            new byte[] { 0x07, 2, 8 },
            new byte[] { 0x08, 2, 8 },
            new byte[] { 0x09, 2, 8 },
            new byte[] { 0x0A, 2, 8 },
            new byte[] { 0x0B, 2, 8 },
            new byte[] { 0x0C, 2, 8 },
            new byte[] { 0x0D, 2, 8 },
            new byte[] { 0x0E, 2, 16 },
            new byte[] { 0x0F, 2, 8 },

#endregion

#region 0x1_

            new byte[] { 0x10, 2, 8 },
            new byte[] { 0x11, 2, 8 },
            new byte[] { 0x12, 2, 8 },
            new byte[] { 0x13, 2, 8 },
            new byte[] { 0x14, 2, 8 },
            new byte[] { 0x15, 2, 8 },
            new byte[] { 0x16, 2, 16 },
            new byte[] { 0x17, 2, 8 },
            new byte[] { 0x18, 2, 8 },
            new byte[] { 0x19, 2, 8 },
            new byte[] { 0x1A, 2, 8 },
            new byte[] { 0x1B, 2, 8 },
            new byte[] { 0x1C, 2, 8 },
            new byte[] { 0x1D, 2, 8 },
            new byte[] { 0x1E, 2, 16 },
            new byte[] { 0x1F, 2, 8 },

#endregion

#region 0x2_

            new byte[] { 0x20, 2, 8 },
            new byte[] { 0x21, 2, 8 },
            new byte[] { 0x22, 2, 8 },
            new byte[] { 0x23, 2, 8 },
            new byte[] { 0x24, 2, 8 },
            new byte[] { 0x25, 2, 8 },
            new byte[] { 0x26, 2, 16 },
            new byte[] { 0x27, 2, 8 },
            new byte[] { 0x28, 2, 8 },
            new byte[] { 0x29, 2, 8 },
            new byte[] { 0x2A, 2, 8 },
            new byte[] { 0x2B, 2, 8 },
            new byte[] { 0x2C, 2, 8 },
            new byte[] { 0x2D, 2, 8 },
            new byte[] { 0x2E, 2, 16 },
            new byte[] { 0x2F, 2, 8 },

#endregion

#region 0x3_

            new byte[] { 0x30, 2, 8 },
            new byte[] { 0x31, 2, 8 },
            new byte[] { 0x32, 2, 8 },
            new byte[] { 0x33, 2, 8 },
            new byte[] { 0x34, 2, 8 },
            new byte[] { 0x35, 2, 8 },
            new byte[] { 0x36, 2, 16 },
            new byte[] { 0x37, 2, 8 },
            new byte[] { 0x38, 2, 8 },
            new byte[] { 0x39, 2, 8 },
            new byte[] { 0x3A, 2, 8 },
            new byte[] { 0x3B, 2, 8 },
            new byte[] { 0x3C, 2, 8 },
            new byte[] { 0x3D, 2, 8 },
            new byte[] { 0x3E, 2, 16 },
            new byte[] { 0x3F, 2, 8 },

#endregion

#region 0x4_

            new byte[] { 0x40, 2, 8 },
            new byte[] { 0x41, 2, 8 },
            new byte[] { 0x42, 2, 8 },
            new byte[] { 0x43, 2, 8 },
            new byte[] { 0x44, 2, 8 },
            new byte[] { 0x45, 2, 8 },
            new byte[] { 0x46, 2, 12 },
            new byte[] { 0x47, 2, 8 },
            new byte[] { 0x48, 2, 8 },
            new byte[] { 0x49, 2, 8 },
            new byte[] { 0x4A, 2, 8 },
            new byte[] { 0x4B, 2, 8 },
            new byte[] { 0x4C, 2, 8 },
            new byte[] { 0x4D, 2, 8 },
            new byte[] { 0x4E, 2, 12 },
            new byte[] { 0x4F, 2, 8 },

#endregion

#region 0x5_

            new byte[] { 0x50, 2, 8 },
            new byte[] { 0x51, 2, 8 },
            new byte[] { 0x52, 2, 8 },
            new byte[] { 0x53, 2, 8 },
            new byte[] { 0x54, 2, 8 },
            new byte[] { 0x55, 2, 8 },
            new byte[] { 0x56, 2, 12 },
            new byte[] { 0x57, 2, 8 },
            new byte[] { 0x58, 2, 8 },
            new byte[] { 0x59, 2, 8 },
            new byte[] { 0x5A, 2, 8 },
            new byte[] { 0x5B, 2, 8 },
            new byte[] { 0x5C, 2, 8 },
            new byte[] { 0x5D, 2, 8 },
            new byte[] { 0x5E, 2, 12 },
            new byte[] { 0x5F, 2, 8 },

#endregion

#region 0x6_

            new byte[] { 0x60, 2, 8 },
            new byte[] { 0x61, 2, 8 },
            new byte[] { 0x62, 2, 8 },
            new byte[] { 0x63, 2, 8 },
            new byte[] { 0x64, 2, 8 },
            new byte[] { 0x65, 2, 8 },
            new byte[] { 0x66, 2, 12 },
            new byte[] { 0x67, 2, 8 },
            new byte[] { 0x68, 2, 8 },
            new byte[] { 0x69, 2, 8 },
            new byte[] { 0x6A, 2, 8 },
            new byte[] { 0x6B, 2, 8 },
            new byte[] { 0x6C, 2, 8 },
            new byte[] { 0x6D, 2, 8 },
            new byte[] { 0x6E, 2, 12 },
            new byte[] { 0x6F, 2, 8 },

#endregion

#region 0x7_

            new byte[] { 0x70, 2, 8 },
            new byte[] { 0x71, 2, 8 },
            new byte[] { 0x72, 2, 8 },
            new byte[] { 0x73, 2, 8 },
            new byte[] { 0x74, 2, 8 },
            new byte[] { 0x75, 2, 8 },
            new byte[] { 0x76, 2, 12 },
            new byte[] { 0x77, 2, 8 },
            new byte[] { 0x78, 2, 8 },
            new byte[] { 0x79, 2, 8 },
            new byte[] { 0x7A, 2, 8 },
            new byte[] { 0x7B, 2, 8 },
            new byte[] { 0x7C, 2, 8 },
            new byte[] { 0x7D, 2, 8 },
            new byte[] { 0x7E, 2, 12 },
            new byte[] { 0x7F, 2, 8 },

#endregion

#region 0x8_

            new byte[] { 0x80, 2, 8 },
            new byte[] { 0x81, 2, 8 },
            new byte[] { 0x82, 2, 8 },
            new byte[] { 0x83, 2, 8 },
            new byte[] { 0x84, 2, 8 },
            new byte[] { 0x85, 2, 8 },
            new byte[] { 0x86, 2, 16 },
            new byte[] { 0x87, 2, 8 },
            new byte[] { 0x88, 2, 8 },
            new byte[] { 0x89, 2, 8 },
            new byte[] { 0x8A, 2, 8 },
            new byte[] { 0x8B, 2, 8 },
            new byte[] { 0x8C, 2, 8 },
            new byte[] { 0x8D, 2, 8 },
            new byte[] { 0x8E, 2, 16 },
            new byte[] { 0x8F, 2, 8 },

#endregion

#region 0x9_

            new byte[] { 0x90, 2, 8 },
            new byte[] { 0x91, 2, 8 },
            new byte[] { 0x92, 2, 8 },
            new byte[] { 0x93, 2, 8 },
            new byte[] { 0x94, 2, 8 },
            new byte[] { 0x95, 2, 8 },
            new byte[] { 0x96, 2, 16 },
            new byte[] { 0x97, 2, 8 },
            new byte[] { 0x98, 2, 8 },
            new byte[] { 0x99, 2, 8 },
            new byte[] { 0x9A, 2, 8 },
            new byte[] { 0x9B, 2, 8 },
            new byte[] { 0x9C, 2, 8 },
            new byte[] { 0x9D, 2, 8 },
            new byte[] { 0x9E, 2, 16 },
            new byte[] { 0x9F, 2, 8 },

#endregion

#region 0xA_

            new byte[] { 0xA0, 2, 8 },
            new byte[] { 0xA1, 2, 8 },
            new byte[] { 0xA2, 2, 8 },
            new byte[] { 0xA3, 2, 8 },
            new byte[] { 0xA4, 2, 8 },
            new byte[] { 0xA5, 2, 8 },
            new byte[] { 0xA6, 2, 16 },
            new byte[] { 0xA7, 2, 8 },
            new byte[] { 0xA8, 2, 8 },
            new byte[] { 0xA9, 2, 8 },
            new byte[] { 0xAA, 2, 8 },
            new byte[] { 0xAB, 2, 8 },
            new byte[] { 0xAC, 2, 8 },
            new byte[] { 0xAD, 2, 8 },
            new byte[] { 0xAE, 2, 16 },
            new byte[] { 0xAF, 2, 8 },

#endregion

#region 0xB_

            new byte[] { 0xB0, 2, 8 },
            new byte[] { 0xB1, 2, 8 },
            new byte[] { 0xB2, 2, 8 },
            new byte[] { 0xB3, 2, 8 },
            new byte[] { 0xB4, 2, 8 },
            new byte[] { 0xB5, 2, 8 },
            new byte[] { 0xB6, 2, 16 },
            new byte[] { 0xB7, 2, 8 },
            new byte[] { 0xB8, 2, 8 },
            new byte[] { 0xB9, 2, 8 },
            new byte[] { 0xBA, 2, 8 },
            new byte[] { 0xBB, 2, 8 },
            new byte[] { 0xBC, 2, 8 },
            new byte[] { 0xBD, 2, 8 },
            new byte[] { 0xBE, 2, 16 },
            new byte[] { 0xBF, 2, 8 },

#endregion

#region 0xC_

            new byte[] { 0xC0, 2, 8 },
            new byte[] { 0xC1, 2, 8 },
            new byte[] { 0xC2, 2, 8 },
            new byte[] { 0xC3, 2, 8 },
            new byte[] { 0xC4, 2, 8 },
            new byte[] { 0xC5, 2, 8 },
            new byte[] { 0xC6, 2, 16 },
            new byte[] { 0xC7, 2, 8 },
            new byte[] { 0xC8, 2, 8 },
            new byte[] { 0xC9, 2, 8 },
            new byte[] { 0xCA, 2, 8 },
            new byte[] { 0xCB, 2, 8 },
            new byte[] { 0xCC, 2, 8 },
            new byte[] { 0xCD, 2, 8 },
            new byte[] { 0xCE, 2, 16 },
            new byte[] { 0xCF, 2, 8 },

#endregion

#region 0xD_

            new byte[] { 0xD0, 2, 8 },
            new byte[] { 0xD1, 2, 8 },
            new byte[] { 0xD2, 2, 8 },
            new byte[] { 0xD3, 2, 8 },
            new byte[] { 0xD4, 2, 8 },
            new byte[] { 0xD5, 2, 8 },
            new byte[] { 0xD6, 2, 16 },
            new byte[] { 0xD7, 2, 8 },
            new byte[] { 0xD8, 2, 8 },
            new byte[] { 0xD9, 2, 8 },
            new byte[] { 0xDA, 2, 8 },
            new byte[] { 0xDB, 2, 8 },
            new byte[] { 0xDC, 2, 8 },
            new byte[] { 0xDD, 2, 8 },
            new byte[] { 0xDE, 2, 16 },
            new byte[] { 0xDF, 2, 8 },

#endregion

#region 0xE_

            new byte[] { 0xE0, 2, 8 },
            new byte[] { 0xE1, 2, 8 },
            new byte[] { 0xE2, 2, 8 },
            new byte[] { 0xE3, 2, 8 },
            new byte[] { 0xE4, 2, 8 },
            new byte[] { 0xE5, 2, 8 },
            new byte[] { 0xE6, 2, 16 },
            new byte[] { 0xE7, 2, 8 },
            new byte[] { 0xE8, 2, 8 },
            new byte[] { 0xE9, 2, 8 },
            new byte[] { 0xEA, 2, 8 },
            new byte[] { 0xEB, 2, 8 },
            new byte[] { 0xEC, 2, 8 },
            new byte[] { 0xED, 2, 8 },
            new byte[] { 0xEE, 2, 16 },
            new byte[] { 0xEF, 2, 8 },

#endregion

#region 0xF_

            new byte[] { 0xF0, 2, 8 },
            new byte[] { 0xF1, 2, 8 },
            new byte[] { 0xF2, 2, 8 },
            new byte[] { 0xF3, 2, 8 },
            new byte[] { 0xF4, 2, 8 },
            new byte[] { 0xF5, 2, 8 },
            new byte[] { 0xF6, 2, 16 },
            new byte[] { 0xF7, 2, 8 },
            new byte[] { 0xF8, 2, 8 },
            new byte[] { 0xF9, 2, 8 },
            new byte[] { 0xFA, 2, 8 },
            new byte[] { 0xFB, 2, 8 },
            new byte[] { 0xFC, 2, 8 },
            new byte[] { 0xFD, 2, 8 },
            new byte[] { 0xFE, 2, 16 },
            new byte[] { 0xFF, 2, 8 },

#endregion

        };

        foreach (var testCase in data)
        {
            // Subtract 1 from the expected size here and 4 from the cycles because this test does not include the initial 0xCB instruction that instructs
            // us to look at the extension table. We need to pretend we've executed that prior to each case here in order to get the right result
            var testCaseData = new TestCaseData(testCase[0], testCase[1], testCase[2]);
            testCaseData.SetName($"CB Instruction 0x{Convert.ToString(testCase[0], 16).PadLeft(2, '0').ToUpperInvariant()}");
            yield return testCaseData;
        }
    }
}