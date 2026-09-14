// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AcmeBinding;
using nanoFramework.TestFramework;
using System.Drawing;
using System.Numerics;

namespace Iot.Device.NFUnitTest
{
    [TestClass]
    public class AcmeDeviceTests
    {
        [TestMethod]
        public void FirmwareVersion_Is_Reported()
        {
            AcmeDevice device = new AcmeDevice();

            Assert.AreEqual("1.0.0-synthetic", device.FirmwareVersion);
        }

        [TestMethod]
        public void SamplingRateHz_Round_Trips_Through_Property()
        {
            AcmeDevice device = new AcmeDevice();

            device.SamplingRateHz = 42;

            Assert.AreEqual(42, device.SamplingRateHz);
        }

        [TestMethod]
        public void Threshold_Round_Trips_Through_Paired_Getter_And_Setter_Methods()
        {
            AcmeDevice device = new AcmeDevice();

            device.SetThreshold(12.5);

            Assert.AreEqual(12.5, device.GetThreshold());
        }

        [TestMethod]
        public void GetUptimeSeconds_Increments_On_Each_Read()
        {
            AcmeDevice device = new AcmeDevice();

            int first = device.GetUptimeSeconds();
            int second = device.GetUptimeSeconds();

            Assert.IsTrue(second > first);
        }

        [TestMethod]
        public void TryReadOrientation_Returns_True_With_A_Unit_Vector()
        {
            AcmeDevice device = new AcmeDevice();

            bool result = device.TryReadOrientation(out Vector3 orientation);

            Assert.IsTrue(result);
            Assert.AreEqual(1.0, orientation.Z);
        }

        [TestMethod]
        public void SetMode_Changes_CurrentMode()
        {
            AcmeDevice device = new AcmeDevice();

            device.SetMode(AcmeMode.Active);

            Assert.AreEqual((int)AcmeMode.Active, (int)device.CurrentMode);
        }

        [TestMethod]
        public void Reset_Returns_Device_To_Idle_And_Zeroes_Uptime()
        {
            AcmeDevice device = new AcmeDevice();
            device.SetMode(AcmeMode.Active);
            device.GetUptimeSeconds();

            device.Reset();

            Assert.AreEqual((int)AcmeMode.Idle, (int)device.CurrentMode);
            Assert.AreEqual(0, device.GetUptimeSeconds());
        }

        [TestMethod]
        public void SetStatusLedColor_Updates_StatusLedColor()
        {
            AcmeDevice device = new AcmeDevice();

            device.SetStatusLedColor(Color.Red);

            Assert.AreEqual(Color.Red.ToArgb(), device.StatusLedColor.ToArgb());
        }

        [TestMethod]
        public void Calibrate_Records_All_Three_Parameters()
        {
            AcmeDevice device = new AcmeDevice();

            device.Calibrate(0.5, 1.02, 3);

            Assert.AreEqual(0.5, device.LastCalibrationOffset);
            Assert.AreEqual(1.02, device.LastCalibrationScale);
            Assert.AreEqual(3, device.LastCalibrationIterations);
            Assert.AreEqual((int)AcmeMode.Idle, (int)device.CurrentMode);
        }

        [TestMethod]
        public void Beeper_Component_Starts_And_Stops_Independently()
        {
            AcmeDevice device = new AcmeDevice();

            Assert.IsFalse(device.Beeper.IsBeeping);

            device.Beeper.Beep(100);
            Assert.IsTrue(device.Beeper.IsBeeping);

            device.Beeper.StopBeep();
            Assert.IsFalse(device.Beeper.IsBeeping);
        }
    }
}
