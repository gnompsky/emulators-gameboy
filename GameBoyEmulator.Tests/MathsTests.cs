using GameBoyEmulator.Core;

namespace GameBoyEmulator.Tests
{
    public class MathsTests
    {
        [TestCase(0x00, 0x01, 0x01, false)]
        [TestCase(0x00, 0x02, 0x02, false)]
        [TestCase(0x02, 0x02, 0x04, false)]
        [TestCase(0xFF, 0x01, 0x00, true)]
        [TestCase(0xFF, 0x02, 0x01, true)]
        [TestCase(0xFF, 0xFF, 0xFE, true)]
        public void WrappingAdd(byte a, byte b, byte expectedResult, bool expectedHalfCarry)
        {
            // Arrange

            // Act
            var result = Maths.WrappingAdd(a, b, out var halfCarried);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expectedResult));
                Assert.That(halfCarried, Is.EqualTo(expectedHalfCarry));
            });
        }
        
        [TestCase((ushort)0x0000, (ushort)0x0001, (ushort)0x0001)]
        [TestCase((ushort)0x0000, (ushort)0x0002, (ushort)0x0002)]
        [TestCase((ushort)0x0002, (ushort)0x0002, (ushort)0x0004)]
        [TestCase((ushort)0x00FF, (ushort)0x0001, (ushort)0x0100)]
        [TestCase((ushort)0x00FF, (ushort)0x0002, (ushort)0x0101)]
        [TestCase((ushort)0x00FF, (ushort)0x00FF, (ushort)0x01FE)]
        [TestCase((ushort)0xFFFF, (ushort)0x0001, (ushort)0x0000)]
        public void WrappingAdd(ushort a, ushort b, ushort expectedResult)
        {
            // Arrange

            // Act
            var result = Maths.WrappingAdd(a, b);

            // Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        
        // TODO: Add remaining tests
    }
}