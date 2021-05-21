// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Iot.Device.Magnetometer;
using UnitsNet;

namespace Iot.Device.Imu
{
    /// <summary>
    /// MPU9250 - gyroscope, accelerometer, temperature and magnetometer (thru an embedded AK8963).
    /// </summary>
    [Interface("MPU9250 - gyroscope, accelerometer, temperature and magnetometer (thru an embedded AK8963)")]
    public class Mpu9250 : Mpu6500
    {
        private Ak8963 _ak8963;
        private bool _shouldDispose;
        // Use for the first magnetometer read when switch to continuous 100 Hz
        private bool _firstContinuousRead = true;

        #region Magnetometer

        /// <summary>
        /// Get the magnetometer bias
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///    +Z   +Y
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///         +X
        /// </remarks>
        [Property]
        public Vector3 MagnometerBias => new Vector3(_ak8963.MagnetometerBias.Y, _ak8963.MagnetometerBias.X, -_ak8963.MagnetometerBias.Z);

        /// <summary>
        /// Calibrate the magnetometer. Make sure your sensor is as far as possible of magnet.
        /// Move your sensor in all direction to make sure it will get enough data in all points of space
        /// Calculate as well the magnetometer bias
        /// </summary>
        /// <param name="calibrationCounts">number of points to read during calibration, default is 1000</param>
        /// <returns>Returns the factory calibration data</returns>
        [Command]
        public Vector3 CalibrateMagnetometer(int calibrationCounts = 1000)
        {
            if (_wakeOnMotion)
            {
                return Vector3.Zero;
            }

            // Run the calibration
            var calib = _ak8963.CalibrateMagnetometer(calibrationCounts);
            // Invert X and Y, don't change Z, this is a multiplication factor only
            // it should stay positive
            return new Vector3(calib.Y, calib.X, calib.Z);
        }

        /// <summary>
        /// True if there is a data to read
        /// </summary>
        public bool HasDataToRead => !(_wakeOnMotion && _ak8963.HasDataToRead);

        /// <summary>
        /// Check if the magnetometer version is the correct one (0x48)
        /// </summary>
        /// <returns>Returns the Magnetometer version number</returns>
        /// <remarks>When the wake on motion is on, you can't read the magnetometer, so this function returns 0</remarks>
        [Property("MagnetometerVersion")]
        public byte GetMagnetometerVersion() => _wakeOnMotion ? (byte)0 : _ak8963.GetDeviceInfo();

        /// <summary>
        /// Read the magnetometer without bias correction and can wait for new data to be present
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///    +Z   +Y
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///         +X
        /// </remarks>
        /// <param name="waitForData">true to wait for new data</param>
        /// <returns>The data from the magnetometer</returns>
        public Vector3 ReadMagnetometerWithoutCorrection(bool waitForData = true)
        {
            var readMag = _ak8963.ReadMagnetometerWithoutCorrection(waitForData, GetTimeout());
            _firstContinuousRead = false;
            return _wakeOnMotion ? Vector3.Zero : new Vector3(readMag.Y, readMag.X, -readMag.Z);
        }

        /// <summary>
        /// Read the magnetometer with bias correction and can wait for new data to be present
        /// </summary>
        /// <remarks>
        /// Vector axes are the following:
        ///    +Z   +Y
        ///  \  |  /
        ///   \ | /
        ///    \|/
        ///    /|\
        ///   / | \
        ///  /  |  \
        ///         +X
        /// </remarks>
        /// <param name="waitForData">true to wait for new data</param>
        /// <returns>The data from the magnetometer</returns>
        [Telemetry("MagneticInduction")]
        public Vector3 ReadMagnetometer(bool waitForData = true)
        {
            Vector3 magn = _ak8963.ReadMagnetometer(waitForData, GetTimeout());
            return new Vector3(magn.Y, magn.X, -magn.Z);
        }

        // TODO: find what is the value in the documentation, it should be pretty fast
        // But taking the same value as for the slowest one so th 8Hz one
        private TimeSpan GetTimeout() => _ak8963.MeasurementMode switch
        {
            // 8Hz measurement period plus 2 milliseconds
            MeasurementMode.SingleMeasurement or MeasurementMode.ExternalTriggedMeasurement or MeasurementMode.SelfTest or MeasurementMode.ContinuousMeasurement8Hz
                => TimeSpan.FromMilliseconds(127),
            // 100Hz measurement period plus 2 milliseconds
            // When switching to this mode, the first read can be longer than 10 ms. Tests shows up to 100 ms
            MeasurementMode.ContinuousMeasurement100Hz => _firstContinuousRead ? TimeSpan.FromMilliseconds(100) : TimeSpan.FromMilliseconds(12),
            _ => TimeSpan.Zero,
        };

        /// <summary>
        /// Select the magnetometer measurement mode
        /// </summary>
        public MeasurementMode MagnetometerMeasurementMode
        {
            get => _ak8963.MeasurementMode;
            set
            {
                _ak8963.MeasurementMode = value;
                if (value == MeasurementMode.ContinuousMeasurement100Hz)
                {
                    _firstContinuousRead = true;
                }
            }
        }

        /// <summary>
        /// Select the magnetometer output bit rate
        /// </summary>
        [Property]
        public OutputBitMode MagnetometerOutputBitMode
        {
            get => _ak8963.OutputBitMode;
            set => _ak8963.OutputBitMode = value;
        }

        /// <summary>
        /// Get the magnetometer hardware adjustment bias
        /// </summary>
        [Property]
        public Vector3 MagnetometerAdjustment => _ak8963.MagnetometerAdjustment;

        #endregion

        /// <summary>
        /// Initialize the MPU9250
        /// </summary>
        /// <param name="i2cDevice">The I2C device</param>
        /// <param name="shouldDispose">Will automatically dispose the I2C device if true</param>
        /// <param name="i2CDeviceAk8963">An I2C Device for the AK8963 when exposed and not behind the MPU9250</param>
        public Mpu9250(I2cDevice i2cDevice, bool shouldDispose = true, I2cDevice? i2CDeviceAk8963 = null)
            : base(i2cDevice, true)
        {
            Reset();
            PowerOn();
            if (!CheckVersion())
            {
                throw new IOException($"This device does not contain the correct signature 0x71 for a MPU9250");
            }

            GyroscopeBandwidth = GyroscopeBandwidth.Bandwidth0250Hz;
            GyroscopeRange = GyroscopeRange.Range0250Dps;
            AccelerometerBandwidth = AccelerometerBandwidth.Bandwidth1130Hz;
            AccelerometerRange = AccelerometerRange.Range02G;
            // Setup I2C for the AK8963
            WriteRegister(Register.USER_CTRL, (byte)UserControls.I2C_MST_EN);
            // Speed of 400 kHz
            WriteRegister(Register.I2C_MST_CTRL, (byte)I2cBusFrequency.Frequency400kHz);
            _shouldDispose = shouldDispose;
            // There are 2 options to setup the Ak8963. Either the I2C address is exposed, either not.
            // Trying both and pick one of them
            if (i2CDeviceAk8963 == null)
            {
                try
                {
                    _ak8963 = new Ak8963(i2cDevice, new Ak8963Attached(), false);
                }
                catch (IOException ex)
                {
                    throw new IOException($"Please try to create an I2cDevice for the AK8963, it may be exposed", ex);
                }
            }
            else
            {
                _ak8963 = new Ak8963(i2CDeviceAk8963);
            }

            if (!_ak8963.IsVersionCorrect())
            {
                // Try to reset the device first
                _ak8963.Reset();
                // Wait a bit
                if (!_ak8963.IsVersionCorrect())
                {
                    // Try to reset the I2C Bus
                    WriteRegister(Register.USER_CTRL, (byte)UserControls.I2C_MST_RST);
                    Thread.Sleep(100);
                    // Resetup again
                    WriteRegister(Register.USER_CTRL, (byte)UserControls.I2C_MST_EN);
                    WriteRegister(Register.I2C_MST_CTRL, (byte)I2cBusFrequency.Frequency400kHz);
                    // Poorly documented time to wait after the I2C bus reset
                    // Found out that waiting a little bit is needed. Exact time may be lower
                    Thread.Sleep(100);
                    // Try one more time
                    if (!_ak8963.IsVersionCorrect())
                    {
                        throw new IOException($"This device does not contain the correct signature 0x48 for a AK8963 embedded into the MPU9250");
                    }
                }
            }

            _ak8963.MeasurementMode = MeasurementMode.SingleMeasurement;
        }

        /// <summary>
        /// Return true if the version of MPU9250 is the correct one
        /// </summary>
        /// <returns>True if success</returns>
        internal new bool CheckVersion()
        {
            // Check if the version is thee correct one
            return ReadByte(Register.WHO_AM_I) == 0x71;
        }

        /// <summary>
        /// Setup the Wake On Motion. This mode generate a rising signal on pin INT
        /// You can catch it with a normal GPIO and place an interruption on it if supported
        /// Reading the sensor won't give any value until it wakes up periodically
        /// Only Accelerator data is available in this mode
        /// </summary>
        /// <param name="accelerometerThreshold">Threshold of magnetometer x/y/z axes. LSB = 4mg. Range is 0mg to 1020mg</param>
        /// <param name="acceleratorLowPower">Frequency used to measure data for the low power consumption mode</param>
        public new void SetWakeOnMotion(uint accelerometerThreshold, AccelerometerLowPowerFrequency acceleratorLowPower)
        {
            // We can't use the magnetometer, only Accelerometer will be measured
            _ak8963.MeasurementMode = MeasurementMode.PowerDown;
            base.SetWakeOnMotion(accelerometerThreshold, acceleratorLowPower);
        }

        /// <summary>
        /// Cleanup everything
        /// </summary>
        public new void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
                _i2cDevice = null!;
            }
        }

    }
}
