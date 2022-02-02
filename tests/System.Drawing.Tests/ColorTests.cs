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

        //
        // Method ParseHexChar is private change to public and uncomment tests
        [TestMethod]
        public void ParseHexCharTest()
        {
            //TODO: Rewrite to [DataTestMethod] [DataRow('1',1)] when become supported
            var result1 = Color.ParseHexChar('1');
            Assert.Equal(result1, 1);
            var result9 = Color.ParseHexChar('9');
            Assert.Equal(result9, 9);
            var resulta = Color.ParseHexChar('a');
            Assert.Equal(resulta, 10);

            var resultb = Color.ParseHexChar('b');
            Assert.Equal(resultb, 11);
            var resultc = Color.ParseHexChar('c');
            Assert.Equal(resultc, 12);
            var resultf = Color.ParseHexChar('f');
            Assert.Equal(resultf, 15);
            var resultA = Color.ParseHexChar('A');
            Assert.Equal(resultA, 10);
            var resultD = Color.ParseHexChar('D');
            Assert.Equal(resultD, 13);
            var resultE = Color.ParseHexChar('E');
            Assert.Equal(resultE, 14);
            var resultF = Color.ParseHexChar('F');
            Assert.Equal(resultF, 15);
        }

        [TestMethod]
        public void ParseHexChar_Exceptions_Test()
        {
            Assert.Throws(typeof(FormatException), () => Color.ParseHexChar('x'));
            Assert.Throws(typeof(FormatException), () => Color.ParseHexChar(Char.MinValue));
            Assert.Throws(typeof(FormatException), () => Color.ParseHexChar(Char.MaxValue));
        }

        [TestMethod]
        public void ParseHexColor_AARRGGBB_Test()
        {
            //TODO: Rewrite to [DataTestMethod] [DataRow("#FFFF0000",Color.Red)] when become supported
            var red = Color.ParseHex("#FFFF0000");
            Assert.True(red == Color.Red, "not Red");
            var green = Color.ParseHex("#FF008000");
            Assert.True(green == Color.Green, "not Green");
            var blue = Color.ParseHex("#FF0000FF");
            Assert.True(blue == Color.Blue, "not Blue");
            var black = Color.ParseHex("#FF000000");
            Assert.True(black == Color.Black, "not Black");
            var white = Color.ParseHex("#FFFFFFFF");
            Assert.True(white == Color.White, "not White");
        }

        [TestMethod]
        public void ParseHexColor_RRGGBB_Test()
        {
            //TODO: Rewrite to [DataTestMethod] [DataRow("#FF0000",Color.Red)] when become supported
            var red = Color.ParseHex("#FF0000");
            Assert.True(red == Color.Red, "not Red");
            var green = Color.ParseHex("#008000");
            Assert.True(green == Color.Green, "not Green");
            var blue = Color.ParseHex("#0000FF");
            Assert.True(blue == Color.Blue, "not Blue");
            var black = Color.ParseHex("#000000");
            Assert.True(black == Color.Black, "not Black");
            var white = Color.ParseHex("#FFFFFF");
            Assert.True(white == Color.White, "not White");
        }

        [TestMethod]
        public void ParseHexColor_ARGB_Test()
        {
            //TODO: Rewrite to [DataTestMethod] [DataRow("#FF00",Color.Red)] when become supported
            var red = Color.ParseHex("#FF00");
            Assert.True(red == Color.Red, "not Red");
            var green = Color.ParseHex("#F0F0");
            Assert.True(green == Color.Lime, "not Lime");
            var blue = Color.ParseHex("#F00F");
            Assert.True(blue == Color.Blue, "not Blue");
            var black = Color.ParseHex("#F000");
            Assert.True(black == Color.Black, "not Black");
            var white = Color.ParseHex("#FFFF");
            Assert.True(white == Color.White, "not White");
        }

        [TestMethod]
        public void ParseHexColor_RGB_Test()
        {
            //TODO: Rewrite to [DataTestMethod] [DataRow("#F00",Color.Red)] when become supported
            var red = Color.ParseHex("#F00");
            Assert.True(red == Color.Red, "not Red.");
            var green = Color.ParseHex("#0F0");
            Assert.True(green == Color.Lime, "not Lime.");
            var blue = Color.ParseHex("#00F");
            Assert.True(blue == Color.Blue, "not Blue.");
            var black = Color.ParseHex("#000");
            Assert.True(black == Color.Black, "not Black.");
            var white = Color.ParseHex("#FFF");
            Assert.True(white == Color.White, "not White.");
        }

        [TestMethod]
        public void ParseHexColor_Exceptions_Test()
        {
            Assert.Throws(typeof(FormatException), () => Color.ParseHex("#RRGGBB")); // ilegal characters
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("112233")); // leading # missing
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#")); // too short
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#1"));// too short
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#12"));// too short
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#12345"));// length of 6 not match
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#1234567"));// length of 8 not match
            Assert.Throws(typeof(ArgumentException), () => Color.ParseHex("#1234567890")); // too long
        }
    }
}