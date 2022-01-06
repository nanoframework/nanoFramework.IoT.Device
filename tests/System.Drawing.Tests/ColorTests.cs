using nanoFramework.TestFramework;
using System;

namespace System.Drawing.Tests
{
    [TestClass]
    public class ColorTests
    {
        [TestMethod]
        public void ColorFromARGBTest()
        {
            // Arrange
            Color color = Color.FromArgb(0x12345678);

            // Assert
            Assert.Equal((byte)0x12, color.A, "A should be 0x12");
            Assert.Equal((byte)0x34, color.R, "R should be 0x34");
            Assert.Equal((byte)0x56, color.G, "G should be 0x56");
            Assert.Equal((byte)0x78, color.B, "B should be 0x78");
        }        
        
        [TestMethod]
        public void ColorFromARGBIndividualTest()
        {
            // Arrange
            Color color = Color.FromArgb(0x12, 0x34, 0x56, 0x78);

            // Assert
            Assert.Equal((byte)0x12, color.A, "A should be 0x12");
            Assert.Equal((byte)0x34, color.R, "R should be 0x34");
            Assert.Equal((byte)0x56, color.G, "G should be 0x56");
            Assert.Equal((byte)0x78, color.B, "B should be 0x78");
        }        

        [TestMethod]
        public void ColorFromARGBWithoutATest()
        {
            // Arrange
            Color color = Color.FromArgb(0x34, 0x56, 0x78);

            // Assert
            Assert.Equal((byte)0xFF, color.A, "A should be 0xFF");
            Assert.Equal((byte)0x34, color.R, "R should be 0x34");
            Assert.Equal((byte)0x56, color.G, "G should be 0x56");
            Assert.Equal((byte)0x78, color.B, "B should be 0x78");
        }

        [TestMethod]
        public void ColorFromKnownColorTest()
        {
            // Arrange
            Color color = Color.Red;

            // Assert
            Assert.Equal((byte)0xFF, color.A, "A should be 0xFF");
            Assert.Equal((byte)0xFF, color.R, "R should be 0xFF");
            Assert.Equal((byte)0x00, color.G, "G should be 0x00");
            Assert.Equal((byte)0x00, color.B, "B should be 0x00");
        }
    }
}
