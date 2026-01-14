// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Magnetometer;
using nanoFramework.TestFramework;
using System;
using System.Numerics;

namespace Bmm150.tests
{
    [TestClass]
    public class Bmm150Tests
    {
        [TestMethod]
        public void TestCompensateVector3()
        {
            uint rhall = 42;
            Vector3 rawMagnetormeterData = new Vector3 { X = 13.91375923, Y = -28.74289894, Z = 10.16711997 };

            // Arrange

            // DigX1 = 0, DigY1 = 0
            Span<byte> trimX1y1Data = new byte[] { 0x00, 0x00 };
            // DigX2 = 26, DigY2 = 26, DigZ4 = 0
            Span<byte> trimXyzData = new byte[] { 0x00, 0x00, 0x1A, 0x1A };
            // DigZ1 = 24747, DigZ2 = 763, DigZ3 = 0, DigXy1 = 29, DigXy2 = -3, DigXyz1 = 7053
            Span<byte> trimXy1Xy2Data = new byte[] { 0xFB, 0x02, 0xAB, 0x60, 0x8D, 0x1B, 0x00, 0x00, 0xFD, 0x1D };

            // Act
            Bmm150TrimRegisterData trimRegisterData = new Bmm150TrimRegisterData(trimX1y1Data, trimXyzData, trimXy1Xy2Data);

            // Assert
            Assert.AreEqual((byte)0x00, trimRegisterData.DigX1);
            Assert.AreEqual((byte)0x00, trimRegisterData.DigY1);
            Assert.AreEqual((byte)0x1A, trimRegisterData.DigX2);
            Assert.AreEqual((byte)0x1A, trimRegisterData.DigY2);
            Assert.AreEqual(24747, trimRegisterData.DigZ1);
            Assert.AreEqual(763, trimRegisterData.DigZ2);
            Assert.AreEqual(0, trimRegisterData.DigZ3);
            Assert.AreEqual(0, trimRegisterData.DigZ4);
            Assert.AreEqual(29, trimRegisterData.DigXy1);
            Assert.AreEqual(-3, trimRegisterData.DigXy2);
            Assert.AreEqual(7053, trimRegisterData.DigXyz1);

            double x = Bmm150Compensation.CompensateX(rawMagnetormeterData.X, rhall, trimRegisterData);
            double y = Bmm150Compensation.CompensateY(rawMagnetormeterData.Y, rhall, trimRegisterData);
            double z = Bmm150Compensation.CompensateZ(rawMagnetormeterData.Z, rhall, trimRegisterData);

            // Calculated value should be: -1549.91882323
            Assert.AreEqual(
                Math.Ceiling(x),
                Math.Ceiling(-1549.918823),
                "Unexpected x-axis value.");

            // Calculated value should be: 3201.80615234
            Assert.AreEqual(
                Math.Ceiling(y),
                Math.Ceiling(3201.80615234),
                "Unexpected y-axis value.");

            // Calculated value should be: 26.20077896
            Assert.AreEqual(
                Math.Ceiling(z),
                Math.Ceiling(26.20077896),
                "Unexpected z-axis value.");
        }
    }
}
