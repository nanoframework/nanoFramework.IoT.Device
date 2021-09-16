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
            Bmm150TrimRegisterData trimRegisterData = new Bmm150TrimRegisterData()
            {
                dig_x1 = 0,
                dig_x2 = 26,
                dig_xy1 = 29,
                dig_xy2 = -3,
                dig_xyz1 = 7053,
                dig_y1 = 0,
                dig_y2 = 26,
                dig_z1 = 24747,
                dig_z2 = 763,
                dig_z3 = 0,
                dig_z4 = 0
            };

            double x = Bmm150Compensation.Compensate_x(rawMagnetormeterData.X, rhall, trimRegisterData);
            double y = Bmm150Compensation.Compensate_y(rawMagnetormeterData.Y, rhall, trimRegisterData);
            double z = Bmm150Compensation.Compensate_z(rawMagnetormeterData.Z, rhall, trimRegisterData);

            // Calculated value should be: -1549.91882323
            Assert.Equal(Math.Ceiling(x), Math.Ceiling(-1549.918823), "Unexpected x-axis value.");

            // Calculated value should be: 3201.80615234
            Assert.Equal(Math.Ceiling(y), Math.Ceiling(3201.80615234), "Unexpected y-axis value.");

            // Calculated value should be: 26.20077896
            Assert.Equal(Math.Ceiling(z), Math.Ceiling(26.20077896), "Unexpected z-axis value.");
        }
    }
}
