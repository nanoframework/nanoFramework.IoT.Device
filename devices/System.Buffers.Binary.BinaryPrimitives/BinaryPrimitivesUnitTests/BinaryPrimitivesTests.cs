//
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.TestFramework;
using System;
using System.Buffers.Binary;

namespace BinaryPrimitivesUnitTests
{
    [TestClass]
    public class BinaryPrimitivesTests
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
            Span<byte> uint16Res = new Span<byte>(new byte[2]);
            Span<byte> int16Res = new Span<byte>(new byte[2]);

            // Act
            BinaryPrimitives.WriteInt16BigEndian(int16Res, int16);
            BinaryPrimitives.WriteUInt16BigEndian(uint16Res, uint16);
            int16res = BinaryPrimitives.ReadInt16BigEndian(int16byte);
            uint16res = BinaryPrimitives.ReadUInt16BigEndian(uint16byte);

            // Assert
            CollectionAssert.AreEqual(int16byte, int16Res.ToArray());
            CollectionAssert.AreEqual(uint16byte, uint16Res.ToArray());
            Assert.AreEqual(int16, int16res);
            Assert.AreEqual(uint16, uint16res);
        }

        [TestMethod]
        public void TestLeInt16()
        {
            // Arrange
            ushort uint16 = 0x9876;
            ushort uint16res;
            short int16 = 0x3AB6;
            short int16res;
            byte[] uint16byte = new byte[] { 0x76, 0x98 };
            byte[] int16byte = new byte[] { 0xB6, 0x3A };
            Span<byte> uint16Res = new Span<byte>(new byte[2]);
            Span<byte> int16Res = new Span<byte>(new byte[2]);

            // Act
            BinaryPrimitives.WriteInt16LittleEndian(int16Res, int16);
            BinaryPrimitives.WriteUInt16LittleEndian(uint16Res, uint16);
            int16res = BinaryPrimitives.ReadInt16LittleEndian(int16byte);
            uint16res = BinaryPrimitives.ReadUInt16LittleEndian(uint16byte);

            // Assert
            CollectionAssert.AreEqual(int16byte, int16Res.ToArray());
            CollectionAssert.AreEqual(uint16byte, uint16Res.ToArray());
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
            Span<byte> uint32Res = new Span<byte>(new byte[4]);
            Span<byte> intRes = new Span<byte>(new byte[4]);

            // Act
            BinaryPrimitives.WriteInt32BigEndian(intRes, int32);
            BinaryPrimitives.WriteUInt32BigEndian(uint32Res, uint32);
            int32res = BinaryPrimitives.ReadInt32BigEndian(int32byte);
            uint32res = BinaryPrimitives.ReadUInt32BigEndian(uint32byte);

            // Assert
            CollectionAssert.AreEqual(int32byte, intRes.ToArray());
            CollectionAssert.AreEqual(uint32byte, uint32Res.ToArray());
            Assert.AreEqual(int32, int32res);
            Assert.AreEqual(uint32, uint32res);
        }

        [TestMethod]
        public void TestLeInt32()
        {
            // Arrange
            uint uint32 = 0x98765432;
            uint uint32res;
            int int32 = 0x3AB67ABC;
            int int32res;
            byte[] uint32byte = new byte[] { 0x32, 0x54, 0x76, 0x98 };
            byte[] int32byte = new byte[] { 0xBC, 0x7A, 0xB6, 0x3A };
            Span<byte> uint32Res = new Span<byte>(new byte[4]);
            Span<byte> intRes = new Span<byte>(new byte[4]);

            // Act
            BinaryPrimitives.WriteInt32LittleEndian(intRes, int32);
            BinaryPrimitives.WriteUInt32LittleEndian(uint32Res, uint32);
            int32res = BinaryPrimitives.ReadInt32LittleEndian(int32byte);
            uint32res = BinaryPrimitives.ReadUInt32LittleEndian(uint32byte);

            // Assert
            CollectionAssert.AreEqual(int32byte, intRes.ToArray());
            CollectionAssert.AreEqual(uint32byte, uint32Res.ToArray());
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
            Span<byte> uint64Res = new Span<byte>(new byte[8]);
            Span<byte> int64Res = new Span<byte>(new byte[8]);

            // Act
            BinaryPrimitives.WriteInt64BigEndian(int64Res, int64);
            BinaryPrimitives.WriteUInt64BigEndian(uint64Res, uint64);
            int64res = BinaryPrimitives.ReadInt64BigEndian(int64byte);
            uint64res = BinaryPrimitives.ReadUInt64BigEndian(uint64byte);

            // Assert
            CollectionAssert.AreEqual(int64byte, int64Res.ToArray());
            CollectionAssert.AreEqual(uint64byte, uint64Res.ToArray());
            Assert.AreEqual(int64, int64res);
            Assert.AreEqual(uint64, uint64res);
        }

        [TestMethod]
        public void TestLeInt64()
        {
            // Arrange
            ulong uint64 = 0x9876543210ABCDEF;
            ulong uint64res;
            long int64 = 0x3AB67ABC9874CAFE;
            long int64res;
            byte[] uint64byte = new byte[] { 0xEF, 0xCD, 0xAB, 0x10, 0x32, 0x54, 0x76, 0x98 };
            byte[] int64byte = new byte[] { 0xFE, 0xCA, 0x74, 0x98, 0xBC, 0x7A, 0xB6, 0x3A };
            Span<byte> uint64Res = new Span<byte>(new byte[8]);
            Span<byte> int64Res = new Span<byte>(new byte[8]);

            // Act
            BinaryPrimitives.WriteInt64LittleEndian(int64Res, int64);
            BinaryPrimitives.WriteUInt64LittleEndian(uint64Res, uint64);
            int64res = BinaryPrimitives.ReadInt64LittleEndian(int64byte);
            uint64res = BinaryPrimitives.ReadUInt64LittleEndian(uint64byte);

            // Assert
            CollectionAssert.AreEqual(int64byte, int64Res.ToArray());
            CollectionAssert.AreEqual(uint64byte, uint64Res.ToArray());
            Assert.AreEqual(int64, int64res);
            Assert.AreEqual(uint64, uint64res);
        }

        [TestMethod]
        public void TestExceptions()
        {
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () =>
            {
                Span<byte> toosmall = new Span<byte>(new byte[3]);
                BinaryPrimitives.WriteInt32LittleEndian(toosmall, 42);
            });

            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () =>
            {
                Span<byte> toosmall = new Span<byte>(new byte[1]);
                BinaryPrimitives.WriteInt16LittleEndian(toosmall, 42);
            });
        }

        [TestMethod]
        public void TestBeSingle()
        {
            // Arrange
            float floatValue = 3.141593f;
            byte[] floatValueInBe = new byte[] { 0x40, 0x49, 0x0F, 0xDC };
            float floatFromBytes;
            double doubleFromBytes;
            float floatValueFromBitConverter;
            Span<byte> floatToBytes = new Span<byte>(new byte[4]);

            // Act
            floatFromBytes = BinaryPrimitives.ReadSingleBigEndian(floatValueInBe);
            doubleFromBytes = BinaryPrimitives.ReadSingleBigEndian(floatValueInBe);
            BinaryPrimitives.WriteSingleBigEndian(floatToBytes, floatValue);
            floatValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToSingle(Reverse(floatValueInBe), 0) : BitConverter.ToSingle(floatValueInBe, 0);

            // Assert
            CollectionAssert.AreEqual(floatValueInBe, floatToBytes.ToArray());
            Assert.AreEqual(floatValue, floatFromBytes);
            Assert.AreEqual(floatValue, floatValueFromBitConverter);
            Assert.AreEqual(floatValue, doubleFromBytes, "This assert fails when the CLR didn't properly convert the uint into a float");
        }

        [TestMethod]
        public void TestLeSingle()
        {
            // Arrange
            float floatValue = 3.141593f;
            byte[] floatValueInLe = new byte[] { 0xDC, 0x0F, 0x49, 0x40 };
            float floatFromBytes;
            double doubleFromBytes;
            float floatValueFromBitConverter;
            Span<byte> floatToBytes = new Span<byte>(new byte[4]);

            // Act
            floatFromBytes = BinaryPrimitives.ReadSingleLittleEndian(floatValueInLe);
            doubleFromBytes = BinaryPrimitives.ReadSingleLittleEndian(floatValueInLe);
            BinaryPrimitives.WriteSingleLittleEndian(floatToBytes, floatValue);
            floatValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToSingle(floatValueInLe, 0) : BitConverter.ToSingle(Reverse(floatValueInLe), 0);

            // Assert
            CollectionAssert.AreEqual(floatValueInLe, floatToBytes.ToArray());
            Assert.AreEqual(floatValue, floatFromBytes);
            Assert.AreEqual(floatValue, floatValueFromBitConverter);
            Assert.AreEqual(floatValue, doubleFromBytes, "This assert fails when the CLR didn't properly convert the uint into a float");
        }

        [TestMethod]
        public void TestBeDouble()
        {
            // Arrange
            double doubleValue = 3.141592653589793;
            byte[] doubleValueInBe = new byte[] { 0x40, 0x09, 0x21, 0xFB, 0x54, 0x44, 0x2D, 0x18 };
            double doubleFromBytes;
            double doubleValueFromBitConverter;
            Span<byte> doubleToBytes = new Span<byte>(new byte[8]);

            // Act
            doubleFromBytes = BinaryPrimitives.ReadDoubleBigEndian(doubleValueInBe);
            BinaryPrimitives.WriteDoubleBigEndian(doubleToBytes, doubleValue);
            doubleValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToDouble(Reverse(doubleValueInBe), 0) : BitConverter.ToDouble(doubleValueInBe, 0);

            // Assert
            CollectionAssert.AreEqual(doubleValueInBe, doubleToBytes.ToArray());
            Assert.AreEqual(doubleValue, doubleFromBytes);
            Assert.AreEqual(doubleValue, doubleValueFromBitConverter);
        }

        [TestMethod]
        public void TestLeDouble()
        {
            // Arrange
            double doubleValue = 3.141592653589793;
            byte[] doubleValueInLe = new byte[] { 0x18, 0x2D, 0x44, 0x54, 0xFB, 0x21, 0x09, 0x40 };
            double doubleFromBytes;
            double doubleValueFromBitConverter;
            Span<byte> doubleToBytes = new Span<byte>(new byte[8]);

            // Act
            doubleFromBytes = BinaryPrimitives.ReadDoubleLittleEndian(doubleValueInLe);
            BinaryPrimitives.WriteDoubleLittleEndian(doubleToBytes, doubleValue);
            doubleValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToDouble(doubleValueInLe, 0) : BitConverter.ToDouble(Reverse(doubleValueInLe), 0);

            // Assert
            CollectionAssert.AreEqual(doubleValueInLe, doubleToBytes.ToArray());
            Assert.AreEqual(doubleValue, doubleFromBytes);
            Assert.AreEqual(doubleValue, doubleValueFromBitConverter);
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
