// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Numerics;
using nanoFramework.TestFramework;

namespace Iot.Device.Qmi8658.Tests
{
    [TestClass]
    public class Qmi8658Tests
    {
        [TestMethod]
        public void ConstructorRejectsNullDevice()
        {
            Assert.ThrowsException(typeof(ArgumentNullException), () => new Qmi8658AccelerometerGyroscope(null));
        }

        [TestMethod]
        public void DecodesSignedLittleEndianVectors()
        {
            Vector3 vector = Qmi8658AccelerometerGyroscope.DecodeVector(new byte[] { 0x34, 0x12, 0x00, 0x80, 0xFF, 0x7F }, 1.0);
            Assert.AreEqual(4660.0, vector.X);
            Assert.AreEqual(-32768.0, vector.Y);
            Assert.AreEqual(32767.0, vector.Z);
        }

        [TestMethod]
        public void DecodesZeroAndNegativeOne()
        {
            Vector3 vector = Qmi8658AccelerometerGyroscope.DecodeVector(new byte[] { 0, 0, 0xFF, 0xFF, 0xFE, 0xFF }, 1.0);
            Assert.AreEqual(0.0, vector.X);
            Assert.AreEqual(-1.0, vector.Y);
            Assert.AreEqual(-2.0, vector.Z);
        }

        [TestMethod]
        public void ConvertsEveryAccelerometerRangeIncludingSignedLimits()
        {
            double[] ranges = new double[] { 2, 4, 8, 16 };
            for (int i = 0; i < ranges.Length; i++)
            {
                double resolution = Qmi8658AccelerometerGyroscope.GetAccelerometerResolution((AccelerometerRange)i);
                Vector3 vector = Qmi8658AccelerometerGyroscope.DecodeVector(new byte[] { 0, 0x80, 0xFF, 0x7F, 0, 0x40 }, resolution);
                Assert.AreEqual(-ranges[i], vector.X);
                Assert.AreEqual(32767.0 * ranges[i] / 32768.0, vector.Y);
                Assert.AreEqual(ranges[i] / 2, vector.Z);
            }
        }

        [TestMethod]
        public void ConvertsEveryGyroscopeRangeIncluding2048Dps()
        {
            double[] ranges = new double[] { 16, 32, 64, 128, 256, 512, 1024, 2048 };
            for (int i = 0; i < ranges.Length; i++)
            {
                double resolution = Qmi8658AccelerometerGyroscope.GetGyroscopeResolution((GyroscopeRange)i);
                Vector3 vector = Qmi8658AccelerometerGyroscope.DecodeVector(new byte[] { 0, 0x80, 0xFF, 0x7F, 0, 0x40 }, resolution);
                Assert.AreEqual(-ranges[i], vector.X);
                Assert.AreEqual(32767.0 * ranges[i] / 32768.0, vector.Y);
                Assert.AreEqual(ranges[i] / 2, vector.Z);
            }
        }

        [TestMethod]
        public void CombinedReportUsesSeparateSensorOffsetsAndScales()
        {
            byte[] report = new byte[] { 0, 0x20, 0, 0xE0, 0, 0x10, 0, 0x40, 0, 0xC0, 0, 0x20 };
            Vector3 acceleration = Qmi8658AccelerometerGyroscope.DecodeVector(new SpanByte(report, 0, 6), Qmi8658AccelerometerGyroscope.GetAccelerometerResolution(AccelerometerRange.Range4G));
            Vector3 angularRate = Qmi8658AccelerometerGyroscope.DecodeVector(new SpanByte(report, 6, 6), Qmi8658AccelerometerGyroscope.GetGyroscopeResolution(GyroscopeRange.Range512Dps));
            Assert.AreEqual(1.0, acceleration.X);
            Assert.AreEqual(-1.0, acceleration.Y);
            Assert.AreEqual(0.5, acceleration.Z);
            Assert.AreEqual(256.0, angularRate.X);
            Assert.AreEqual(-256.0, angularRate.Y);
            Assert.AreEqual(128.0, angularRate.Z);
        }

        [TestMethod]
        public void TemperatureIsSignedWith256LsbPerDegreeAndNoOffset()
        {
            Assert.AreEqual(0.0, Qmi8658AccelerometerGyroscope.DecodeTemperature(new byte[] { 0, 0 }).DegreesCelsius);
            Assert.AreEqual(25.5, Qmi8658AccelerometerGyroscope.DecodeTemperature(new byte[] { 0x80, 0x19 }).DegreesCelsius);
            Assert.AreEqual(-1.5, Qmi8658AccelerometerGyroscope.DecodeTemperature(new byte[] { 0x80, 0xFE }).DegreesCelsius);
            Assert.AreEqual(-128.0, Qmi8658AccelerometerGyroscope.DecodeTemperature(new byte[] { 0, 0x80 }).DegreesCelsius);
            Assert.AreEqual(32767.0 / 256.0, Qmi8658AccelerometerGyroscope.DecodeTemperature(new byte[] { 0xFF, 0x7F }).DegreesCelsius);
        }

        [TestMethod]
        public void EncodesEveryAccelerometerRangeAndNormalRate()
        {
            int[] rangeBits = new int[] { 0x00, 0x10, 0x20, 0x30 };
            for (int range = 0; range < rangeBits.Length; range++)
            {
                for (int rate = 0; rate <= 8; rate++)
                {
                    int configuration = Qmi8658AccelerometerGyroscope.GetAccelerometerConfiguration((AccelerometerRange)range, (OutputDataRate)rate);
                    Assert.AreEqual(rangeBits[range] + rate, configuration);
                    Assert.AreEqual(0, configuration & 0x80);
                }
            }
        }

        [TestMethod]
        public void EncodesEveryGyroscopeRangeAndNormalRate()
        {
            int[] rangeBits = new int[] { 0x00, 0x10, 0x20, 0x30, 0x40, 0x50, 0x60, 0x70 };
            for (int range = 0; range < rangeBits.Length; range++)
            {
                for (int rate = 0; rate <= 8; rate++)
                {
                    int configuration = Qmi8658AccelerometerGyroscope.GetGyroscopeConfiguration((GyroscopeRange)range, (OutputDataRate)rate);
                    Assert.AreEqual(rangeBits[range] + rate, configuration);
                    Assert.AreEqual(0, configuration & 0x80);
                }
            }
        }

        [TestMethod]
        public void DefaultConfigurationMatchesWaveshareRangesWithoutSelfTest()
        {
            Assert.AreEqual(0x15, (int)Qmi8658AccelerometerGyroscope.GetAccelerometerConfiguration(AccelerometerRange.Range4G, OutputDataRate.Odr250Hz));
            Assert.AreEqual(0x55, (int)Qmi8658AccelerometerGyroscope.GetGyroscopeConfiguration(GyroscopeRange.Range512Dps, OutputDataRate.Odr250Hz));
        }

        [TestMethod]
        public void InvalidAccelerometerRangesAreRejected()
        {
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetAccelerometerConfiguration((AccelerometerRange)4, OutputDataRate.Odr250Hz));
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetAccelerometerResolution((AccelerometerRange)255));
        }

        [TestMethod]
        public void InvalidGyroscopeRangesAreRejected()
        {
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetGyroscopeConfiguration((GyroscopeRange)8, OutputDataRate.Odr250Hz));
            Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetGyroscopeResolution((GyroscopeRange)255));
        }

        [TestMethod]
        public void ReservedAndLowPowerRatesAreRejectedForBothSensors()
        {
            int[] invalidRates = new int[] { 9, 10, 11, 12, 13, 14, 15, 255 };
            for (int i = 0; i < invalidRates.Length; i++)
            {
                OutputDataRate rate = (OutputDataRate)invalidRates[i];
                Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetAccelerometerConfiguration(AccelerometerRange.Range4G, rate));
                Assert.ThrowsException(typeof(ArgumentOutOfRangeException), () => Qmi8658AccelerometerGyroscope.GetGyroscopeConfiguration(GyroscopeRange.Range512Dps, rate));
            }
        }

        [TestMethod]
        public void AddressesAndMeasurementRegistersMatchDatasheet()
        {
            Assert.AreEqual(0x6A, (int)Qmi8658AccelerometerGyroscope.DefaultI2cAddress);
            Assert.AreEqual(0x6B, (int)Qmi8658AccelerometerGyroscope.SecondaryI2cAddress);
            Assert.AreEqual(0x2E, (int)Register.Status0);
            Assert.AreEqual(0x33, (int)Register.TemperatureLow);
            Assert.AreEqual(0x35, (int)Register.AccelerometerXLow);
            Assert.AreEqual(0x3B, (int)Register.GyroscopeXLow);
            Assert.AreEqual(0x60, (int)Register.Reset);
        }
    }
}
