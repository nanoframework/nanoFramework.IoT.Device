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
            SpanByte uint16Res = new byte[2];
            SpanByte int16Res = new byte[2];

            // Act
            BinaryPrimitives.WriteInt16BigEndian(int16Res, int16);
            BinaryPrimitives.WriteUInt16BigEndian(uint16Res, uint16);
            int16res = BinaryPrimitives.ReadInt16BigEndian(int16byte);
            uint16res = BinaryPrimitives.ReadUInt16BigEndian(uint16byte);

            // Assert
            Assert.Equal(int16byte[0], int16Res[0]);
            Assert.Equal(int16byte[1], int16Res[1]);
            Assert.Equal(uint16byte[0], uint16Res[0]);
            Assert.Equal(uint16byte[1], uint16Res[1]);
            Assert.Equal(int16, int16res);
            Assert.Equal(uint16, uint16res);
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
            SpanByte uint16Res = new byte[2];
            SpanByte int16Res = new byte[2];

            // Act
            BinaryPrimitives.WriteInt16LittleEndian(int16Res, int16);
            BinaryPrimitives.WriteUInt16LittleEndian(uint16Res, uint16);
            int16res = BinaryPrimitives.ReadInt16LittleEndian(int16byte);
            uint16res = BinaryPrimitives.ReadUInt16LittleEndian(uint16byte);

            // Assert
            Assert.Equal(int16byte[0], int16Res[0]);
            Assert.Equal(int16byte[1], int16Res[1]);
            Assert.Equal(uint16byte[0], uint16Res[0]);
            Assert.Equal(uint16byte[1], uint16Res[1]);
            Assert.Equal(int16, int16res);
            Assert.Equal(uint16, uint16res);
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
            SpanByte uint32Res = new byte[4];
            SpanByte intRes = new byte[4];

            // Act
            BinaryPrimitives.WriteInt32BigEndian(intRes, int32);
            BinaryPrimitives.WriteUInt32BigEndian(uint32Res, uint32);
            int32res = BinaryPrimitives.ReadInt32BigEndian(int32byte);
            uint32res = BinaryPrimitives.ReadUInt32BigEndian(uint32byte);

            // Assert
            Assert.Equal(int32byte[0], intRes[0]);
            Assert.Equal(int32byte[1], intRes[1]);
            Assert.Equal(int32byte[2], intRes[2]);
            Assert.Equal(int32byte[3], intRes[3]);
            Assert.Equal(uint32byte[0], uint32Res[0]);
            Assert.Equal(uint32byte[1], uint32Res[1]);
            Assert.Equal(uint32byte[2], uint32Res[2]);
            Assert.Equal(uint32byte[3], uint32Res[3]);
            Assert.Equal(int32, int32res);
            Assert.Equal(uint32, uint32res);
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
            SpanByte uint32Res = new byte[4];
            SpanByte intRes = new byte[4];

            // Act
            BinaryPrimitives.WriteInt32LittleEndian(intRes, int32);
            BinaryPrimitives.WriteUInt32LittleEndian(uint32Res, uint32);
            int32res = BinaryPrimitives.ReadInt32LittleEndian(int32byte);
            uint32res = BinaryPrimitives.ReadUInt32LittleEndian(uint32byte);

            // Assert
            Assert.Equal(int32byte[0], intRes[0]);
            Assert.Equal(int32byte[1], intRes[1]);
            Assert.Equal(int32byte[2], intRes[2]);
            Assert.Equal(int32byte[3], intRes[3]);
            Assert.Equal(uint32byte[0], uint32Res[0]);
            Assert.Equal(uint32byte[1], uint32Res[1]);
            Assert.Equal(uint32byte[2], uint32Res[2]);
            Assert.Equal(uint32byte[3], uint32Res[3]);
            Assert.Equal(int32, int32res);
            Assert.Equal(uint32, uint32res);
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
            SpanByte uint64Res = new byte[8];
            SpanByte int64Res = new byte[8];

            // Act
            BinaryPrimitives.WriteInt64BigEndian(int64Res, int64);
            BinaryPrimitives.WriteUInt64BigEndian(uint64Res, uint64);
            int64res = BinaryPrimitives.ReadInt64BigEndian(int64byte);
            uint64res = BinaryPrimitives.ReadUInt64BigEndian(uint64byte);

            // Assert
            Assert.Equal(int64byte[0], int64Res[0]);
            Assert.Equal(int64byte[1], int64Res[1]);
            Assert.Equal(int64byte[2], int64Res[2]);
            Assert.Equal(int64byte[3], int64Res[3]);
            Assert.Equal(int64byte[4], int64Res[4]);
            Assert.Equal(int64byte[5], int64Res[5]);
            Assert.Equal(int64byte[6], int64Res[6]);
            Assert.Equal(int64byte[7], int64Res[7]);
            Assert.Equal(uint64byte[0], uint64Res[0]);
            Assert.Equal(uint64byte[1], uint64Res[1]);
            Assert.Equal(uint64byte[2], uint64Res[2]);
            Assert.Equal(uint64byte[3], uint64Res[3]);
            Assert.Equal(uint64byte[4], uint64Res[4]);
            Assert.Equal(uint64byte[5], uint64Res[5]);
            Assert.Equal(uint64byte[6], uint64Res[6]);
            Assert.Equal(uint64byte[7], uint64Res[7]);
            Assert.Equal(int64, int64res);
            Assert.Equal(uint64, uint64res);
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
            SpanByte uint64Res = new byte[8];
            SpanByte int64Res = new byte[8];

            // Act
            BinaryPrimitives.WriteInt64LittleEndian(int64Res, int64);
            BinaryPrimitives.WriteUInt64LittleEndian(uint64Res, uint64);
            int64res = BinaryPrimitives.ReadInt64LittleEndian(int64byte);
            uint64res = BinaryPrimitives.ReadUInt64LittleEndian(uint64byte);

            // Assert
            Assert.Equal(int64byte[0], int64Res[0]);
            Assert.Equal(int64byte[1], int64Res[1]);
            Assert.Equal(int64byte[2], int64Res[2]);
            Assert.Equal(int64byte[3], int64Res[3]);
            Assert.Equal(int64byte[4], int64Res[4]);
            Assert.Equal(int64byte[5], int64Res[5]);
            Assert.Equal(int64byte[6], int64Res[6]);
            Assert.Equal(int64byte[7], int64Res[7]);
            Assert.Equal(uint64byte[0], uint64Res[0]);
            Assert.Equal(uint64byte[1], uint64Res[1]);
            Assert.Equal(uint64byte[2], uint64Res[2]);
            Assert.Equal(uint64byte[3], uint64Res[3]);
            Assert.Equal(uint64byte[4], uint64Res[4]);
            Assert.Equal(uint64byte[5], uint64Res[5]);
            Assert.Equal(uint64byte[6], uint64Res[6]);
            Assert.Equal(uint64byte[7], uint64Res[7]);
            Assert.Equal(int64, int64res);
            Assert.Equal(uint64, uint64res);
        }

        [TestMethod]
        public void TestExceptions()
        {
            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
            {
                SpanByte toosmall = new byte[3];
                BinaryPrimitives.WriteInt32LittleEndian(toosmall, 42);
            });

            Assert.Throws(typeof(ArgumentOutOfRangeException), () =>
            {
                SpanByte toosmall = new byte[1];
                BinaryPrimitives.WriteInt16LittleEndian(toosmall, 42);
            });
        }

        //[TestMethod]
        //public void TestBeSingle()
        //{
        //    // Arrange
        //    float floatValue = 3.141593f;
        //    byte[] floatValueInBe = new byte[] { 0x40, 0x49, 0x0F, 0xDC };
        //    float floatFromBytes;
        //    double doubleFromBytes;
        //    float floatValueFromBitConverter;
        //    SpanByte floatToBytes = new byte[4];

        //    // Act
        //    floatFromBytes = BinaryPrimitives.ReadSingleBigEndian(floatValueInBe);
        //    doubleFromBytes = BinaryPrimitives.ReadSingleBigEndian(floatValueInBe);
        //    BinaryPrimitives.WriteSingleBigEndian(floatToBytes, floatValue);
        //    floatValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToSingle(new byte[] { floatValueInBe[3], floatValueInBe[2], floatValueInBe[1], floatValueInBe[0] }, 0) : BitConverter.ToSingle(floatValueInBe, 0);

        //    // Assert
        //    Assert.Equal(floatValueInBe[0], floatToBytes[0]);
        //    Assert.Equal(floatValueInBe[1], floatToBytes[1]);
        //    Assert.Equal(floatValueInBe[2], floatToBytes[2]);
        //    Assert.Equal(floatValueInBe[3], floatToBytes[3]);
        //    Assert.Equal(floatValue, floatFromBytes);
        //    Assert.Equal(floatValue, floatValueFromBitConverter);
        //    Assert.Equal(floatValue, doubleFromBytes, "This assert fails when the CLR didn't properly convert the uint into a float");
        //}

        //[TestMethod]
        //public void TestLeSingle()
        //{
        //    // Arrange
        //    float floatValue = 3.141593f;
        //    byte[] floatValueInLe = new byte[] { 0xDC, 0x0F, 0x49, 0x40 };
        //    float floatFromBytes;
        //    double doubleFromBytes;
        //    float floatValueFromBitConverter;
        //    SpanByte floatToBytes = new byte[4];

        //    // Act
        //    floatFromBytes = BinaryPrimitives.ReadSingleLittleEndian(floatValueInLe);
        //    doubleFromBytes = BinaryPrimitives.ReadSingleLittleEndian(floatValueInLe);
        //    BinaryPrimitives.WriteSingleLittleEndian(floatToBytes, floatValue);
        //    floatValueFromBitConverter = BitConverter.IsLittleEndian ? BitConverter.ToSingle(floatValueInLe, 0) : BitConverter.ToSingle(new byte[] { floatValueInLe[3], floatValueInLe[2], floatValueInLe[1], floatValueInLe[0] }, 0);

        //    // Assert
        //    Assert.Equal(floatValueInLe[0], floatToBytes[0]);
        //    Assert.Equal(floatValueInLe[1], floatToBytes[1]);
        //    Assert.Equal(floatValueInLe[2], floatToBytes[2]);
        //    Assert.Equal(floatValueInLe[3], floatToBytes[3]);
        //    Assert.Equal(floatValue, floatFromBytes);
        //    Assert.Equal(floatValue, floatValueFromBitConverter);
        //    Assert.Equal(floatValue, doubleFromBytes, "This assert fails when the CLR didn't properly convert the uint into a float");
        //}
    }
}
