// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.TestFramework;
using UnitsNet;

namespace Iot.Device.Adc.Tests
{
    [TestClass]
    public class Ina236Tests
    {
        private static SimulatedIna236 _i2cDevice;
        private static Ina236 _ina236;

        [Setup]
        public void Setup()
        {
            _i2cDevice = new SimulatedIna236();
            _ina236 = new Ina236(_i2cDevice, ElectricResistance.FromMilliohms(8), ElectricCurrent.FromAmperes(8.192));
        }

        [TestMethod]
        public void InitialValuesAndCalibrationAreCorrect()
        {
            Assert.AreEqual((int)Ina236OperatingMode.ContinuousShuntAndBusVoltage, (int)_ina236.OperatingMode);
            Assert.AreEqual((uint)1, _ina236.AverageOverNoSamples);
            Assert.AreEqual(1100, _ina236.BusConversionTime);
            Assert.AreEqual(1100, _ina236.ShuntConversionTime);
            Assert.AreEqual((ushort)1280, _i2cDevice.GetRegister(5));
        }

        [TestMethod]
        public void ReadsDatasheetExampleValues()
        {
            Assert.AreEqual(12.0, _ina236.ReadBusVoltage().Volts);
            double shuntMillivolts = _ina236.ReadShuntVoltage().Millivolts;
            Assert.IsTrue(shuntMillivolts > 47.999 && shuntMillivolts < 48.001);
            Assert.AreEqual(6.0, _ina236.ReadCurrent().Amperes);
            Assert.AreEqual(72.0, _ina236.ReadPower().Watts);
        }

        [TestMethod]
        public void ReadsNegativeShuntVoltage()
        {
            _i2cDevice.SetRegister(1, 0xFFFF);
            Assert.AreEqual(-2.5, _ina236.ReadShuntVoltage().Microvolts);
        }

        [TestMethod]
        public void RoundsConfigurationValuesUp()
        {
            _ina236.AverageOverNoSamples = 5;
            _ina236.BusConversionTime = 205;
            _ina236.ShuntConversionTime = 589;

            Assert.AreEqual((uint)16, _ina236.AverageOverNoSamples);
            Assert.AreEqual(332, _ina236.BusConversionTime);
            Assert.AreEqual(1100, _ina236.ShuntConversionTime);
        }
    }
}