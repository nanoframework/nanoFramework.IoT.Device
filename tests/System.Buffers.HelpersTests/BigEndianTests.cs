//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System.Buffers.Helpers.BitConverter;

namespace BinaryPrimitivesUnitTests
{
    [TestClass]
    public class BigEndianTests
    {
        [TestMethod]
        public void TestBeInt16()
        {
            // Arrange
            ushort uint16 = 0x9876;
            ushort uint16res;
            short int16 = 0x3AB6;
            short int16res;
            byte[] uint16byte = new byte[] { 0x98, 0x76 };
            byte[] int16byte = new byte[] { 0x3A, 0xB6 };
            byte[] uint16Res;
            byte[] int16Res;
            IBitConverter bitConverter = EndianBitConverter.Big;

            // Act
            int16res = bitConverter.ToInt16(int16byte);
            uint16res = bitConverter.ToUInt16(uint16byte);
            int16Res = bitConverter.GetBytes(int16);
            uint16Res = bitConverter.GetBytes(uint16);

            // Assert
            CollectionAssert.AreEqual(int16byte, int16Res);
            CollectionAssert.AreEqual(uint16byte, uint16Res);
            Assert.AreEqual(int16, int16res);
            Assert.AreEqual(uint16, uint16res);
        }

        [TestMethod]
        public void TestBeInt32()
        {
            // Arrange
            uint uint32 = 0x98765432;
            uint uint32res;
            int int32 = 0x3AB67ABC;
            int int32res;
            byte[] uint32byte = new byte[] { 0x98, 0x76, 0x54, 0x32 };
            byte[] int32byte = new byte[] { 0x3A, 0xB6, 0x7A, 0xBC };
            byte[] uint32Res;
            byte[] int32Res;
            IBitConverter bitConverter = EndianBitConverter.Big;

            // Act
            int32res = bitConverter.ToInt32(int32byte);
            uint32res = bitConverter.ToUInt32(uint32byte);
            int32Res = bitConverter.GetBytes(int32);
            uint32Res = bitConverter.GetBytes(uint32);

            // Assert
            CollectionAssert.AreEqual(int32byte, int32Res);
            CollectionAssert.AreEqual(uint32byte, uint32Res);
            Assert.AreEqual(int32, int32res);
            Assert.AreEqual(uint32, uint32res);
        }

        [TestMethod]
        public void TestBeInt64()
        {
            // Arrange
            ulong uint64 = 0x9876543210ABCDEF;
            ulong uint64res;
            long int64 = 0x3AB67ABC9874CAFE;
            long int64res;
            byte[] uint64byte = new byte[] { 0x98, 0x76, 0x54, 0x32, 0x10, 0xAB, 0xCD, 0xEF };
            byte[] int64byte = new byte[] { 0x3A, 0xB6, 0x7A, 0xBC, 0x98, 0x74, 0xCA, 0xFE };
            byte[] uint64Res = new byte[8];
            byte[] int64Res = new byte[8];
            IBitConverter bitConverter = EndianBitConverter.Big;

            // Act
            int64res = bitConverter.ToInt64(int64byte);
            uint64res = bitConverter.ToUInt64(uint64byte);
            int64Res = bitConverter.GetBytes(int64);
            uint64Res = bitConverter.GetBytes(uint64);

            // Assert
            CollectionAssert.AreEqual(int64byte, int64Res);
            CollectionAssert.AreEqual(uint64byte, uint64Res);
            Assert.AreEqual(int64, int64res);
            Assert.AreEqual(uint64, uint64res);
        }

        [TestMethod]
        public void TestBeSingle()
        {
            // Arrange
            float floatValue = 3.141593f;
            byte[] floatValueInBytes = new byte[] { 0xDC, 0x0F, 0x49, 0x40 };
            float floatFromBytes;
            byte[] floatToBytes;
            IBitConverter bitConverter = EndianBitConverter.Big;

            // Act
            floatValueInBytes = Reverse(floatValueInBytes);
            floatFromBytes = bitConverter.ToSingle(floatValueInBytes);
            floatToBytes = bitConverter.GetBytes(floatValue);

            // Assert
            CollectionAssert.AreEqual(floatValueInBytes, floatToBytes);
            Assert.AreEqual(floatValue, floatFromBytes);
        }

        [TestMethod]
        public void TestBeDouble()
        {
            // Arrange
            double doubleValue = 3.141592653589793;
            byte[] doubleValueInBytes = new byte[] { 0x18, 0x2D, 0x44, 0x54, 0xFB, 0x21, 0x09, 0x40 };
            double doubleFromBytes;
            byte[] doubleToBytes;
            IBitConverter bitConverter = EndianBitConverter.Big;

            // Act
            doubleValueInBytes = Reverse(doubleValueInBytes);
            doubleFromBytes = bitConverter.ToDouble(doubleValueInBytes);
            doubleToBytes = bitConverter.GetBytes(doubleValue);

            // Assert
            CollectionAssert.AreEqual(doubleValueInBytes, doubleToBytes);
            Assert.AreEqual(doubleValue, doubleFromBytes);
        }

        private byte[] Reverse(byte[] array)
        {
            if (array == null || array.Length <= 1)
                return new byte[0];

            var newArray = new byte[array.Length];

            int start = 0;
            int end = array.Length - 1;

            while (start < end)
            {
                newArray[start] = array[end];
                newArray[end] = array[start];

                start++;
                end--;
            }

            return newArray;
        }
    }
}
