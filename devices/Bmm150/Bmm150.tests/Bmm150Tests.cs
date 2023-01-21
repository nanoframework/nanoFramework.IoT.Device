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
        //[TestMethod]
        /*public void TestCompensateVector3()
        {
            uint rhall = 42;
            Vector3 rawMagnetormeterData = new Vector3 { X = 13.91375923, Y = -28.74289894, Z = 10.16711997 };
            Bmm150TrimRegisterData trimRegisterData = new Bmm150TrimRegisterData()
            {
                DigX1 = 0,
                DigX2 = 26,
                DigXy1 = 29,
                DigXy2 = -3,
                DigXyz1 = 7053,
                DigY1 = 0,
                DigY2 = 26,
                DigZ1 = 24747,
                DigZ2 = 763,
                DigZ3 = 0,
                DigZ4 = 0
            };

            double x = Bmm150Compensation.CompensateX(rawMagnetormeterData.X, rhall, trimRegisterData);
            double y = Bmm150Compensation.CompensateY(rawMagnetormeterData.Y, rhall, trimRegisterData);
            double z = Bmm150Compensation.CompensateZ(rawMagnetormeterData.Z, rhall, trimRegisterData);

            // Calculated value should be: -1549.91882323
            Assert.Equal(Math.Ceiling(x), Math.Ceiling(-1549.918823), "Unexpected x-axis value.");

            // Calculated value should be: 3201.80615234
            Assert.Equal(Math.Ceiling(y), Math.Ceiling(3201.80615234), "Unexpected y-axis value.");

            // Calculated value should be: 26.20077896
            Assert.Equal(Math.Ceiling(z), Math.Ceiling(26.20077896), "Unexpected z-axis value.");
        }*/
    }
}
