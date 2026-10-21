// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.IO;
using System.Numerics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Qmi8658
{
    /// <summary>
    /// QMI8658 six-axis accelerometer and gyroscope.
    /// </summary>
    /// <remarks>Access is not thread-safe. Serialize configuration and measurement calls.</remarks>
    [Interface("QMI8658 accelerometer and gyroscope")]
    public class Qmi8658AccelerometerGyroscope : IDisposable
    {
        /// <summary>
        /// The default I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6A;

        /// <summary>
        /// The alternate I2C address, used on the Waveshare ESP32-S3-Touch-LCD-2.
        /// </summary>
        public const byte SecondaryI2cAddress = 0x6B;

        private readonly bool _shouldDispose;
        private readonly byte[] _registerBuffer = new byte[1];
        private readonly byte[] _writeBuffer = new byte[2];
        private readonly byte[] _byteBuffer = new byte[1];
        private readonly byte[] _dataBuffer = new byte[12];
        private I2cDevice _i2c;
        private AccelerometerRange _accelerometerRange;
        private GyroscopeRange _gyroscopeRange;
        private OutputDataRate _accelerometerOdr;
        private OutputDataRate _gyroscopeOdr;
        private bool _configured;

        /// <summary>
        /// Initializes a new instance of the <see cref="Qmi8658AccelerometerGyroscope"/> class.
        /// Resets the device and enables both sensors in normal, asynchronous register-read mode.
        /// </summary>
        /// <param name="i2cDevice">The I2C device.</param>
        /// <param name="accelerometerRange">The initial accelerometer range.</param>
        /// <param name="gyroscopeRange">The initial gyroscope range.</param>
        /// <param name="accelerometerOdr">The initial accelerometer output data rate.</param>
        /// <param name="gyroscopeOdr">The initial gyroscope output data rate.</param>
        /// <param name="shouldDispose">Whether to dispose the supplied I2C device when disposing this instance.</param>
        /// <exception cref="ArgumentNullException">The I2C device is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">A range or output data rate is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed or the device identifier is not 0x05.</exception>
        public Qmi8658AccelerometerGyroscope(
            I2cDevice i2cDevice,
            AccelerometerRange accelerometerRange = AccelerometerRange.Range4G,
            GyroscopeRange gyroscopeRange = GyroscopeRange.Range512Dps,
            OutputDataRate accelerometerOdr = OutputDataRate.Odr250Hz,
            OutputDataRate gyroscopeOdr = OutputDataRate.Odr250Hz,
            bool shouldDispose = true)
        {
            _i2c = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
            GetAccelerometerConfiguration(accelerometerRange, accelerometerOdr);
            GetGyroscopeConfiguration(gyroscopeRange, gyroscopeOdr);

            // WHO_AM_I identifies the QMI8658 as 0x05, independently of its I2C address.
            if (ReadByte(Register.WhoAmI) != 0x05)
            {
                throw new IOException();
            }

            // Writing 0xB0 to RESET requests a software reset; allow 20 ms before accessing I2C again.
            WriteByte(Register.Reset, 0xB0);
            Thread.Sleep(20);
            if (ReadByte(Register.WhoAmI) != 0x05)
            {
                throw new IOException();
            }

            // CTRL1 bit 6 enables address auto-increment for multi-byte reads; bit 5 is clear for little-endian SPI.
            WriteByte(Register.Control1, 0x40);
            Configure(accelerometerRange, gyroscopeRange, accelerometerOdr, gyroscopeOdr);
        }

        /// <summary>
        /// Gets or sets the accelerometer full-scale range.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The range is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Property]
        public AccelerometerRange AccelerometerScale
        {
            get => _accelerometerRange;
            set => Configure(value, _gyroscopeRange, _accelerometerOdr, _gyroscopeOdr);
        }

        /// <summary>
        /// Gets or sets the gyroscope full-scale range.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The range is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Property]
        public GyroscopeRange GyroscopeScale
        {
            get => _gyroscopeRange;
            set => Configure(_accelerometerRange, value, _accelerometerOdr, _gyroscopeOdr);
        }

        /// <summary>
        /// Gets or sets the accelerometer output data rate in normal mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The output data rate is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Property]
        public OutputDataRate AccelerometerDataRate
        {
            get => _accelerometerOdr;
            set => Configure(_accelerometerRange, _gyroscopeRange, value, _gyroscopeOdr);
        }

        /// <summary>
        /// Gets or sets the gyroscope output data rate in normal mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The output data rate is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Property]
        public OutputDataRate GyroscopeDataRate
        {
            get => _gyroscopeOdr;
            set => Configure(_accelerometerRange, _gyroscopeRange, _accelerometerOdr, value);
        }

        /// <summary>
        /// Gets a value indicating whether both sensors have new data available.
        /// </summary>
        /// <exception cref="InvalidOperationException">The last configuration attempt failed.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Telemetry]
        public bool IsDataReady
        {
            get
            {
                EnsureConfigured();

                // STATUS0 bits 0 (aDA) and 1 (gDA) indicate new accelerometer and gyroscope data.
                return (ReadByte(Register.Status0) & 0x03) == 0x03;
            }
        }

        /// <summary>
        /// Sets the ranges and output data rates while both sensors are disabled, then restarts them.
        /// </summary>
        /// <param name="accelerometerRange">The accelerometer range.</param>
        /// <param name="gyroscopeRange">The gyroscope range.</param>
        /// <param name="accelerometerOdr">The accelerometer output data rate.</param>
        /// <param name="gyroscopeOdr">The gyroscope output data rate.</param>
        /// <remarks>Allows 70 ms for power-down and 200 ms for startup and filter settling. If an I2C transfer fails, call this method successfully before reading measurements again.</remarks>
        /// <exception cref="ArgumentOutOfRangeException">A range or output data rate is unsupported.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Command]
        public void Configure(AccelerometerRange accelerometerRange, GyroscopeRange gyroscopeRange, OutputDataRate accelerometerOdr, OutputDataRate gyroscopeOdr)
        {
            byte accelerometerConfig = GetAccelerometerConfiguration(accelerometerRange, accelerometerOdr);
            byte gyroscopeConfig = GetGyroscopeConfiguration(gyroscopeRange, gyroscopeOdr);
            _configured = false;

            // Clear CTRL7 aEN/gEN and leave syncSmpl disabled before changing sensor settings.
            WriteByte(Register.Control7, 0x00);

            // Power-down takes up to two sample periods: 2 / 31.25 Hz = 64 ms, rounded up.
            Thread.Sleep(70);
            WriteByte(Register.Control2, accelerometerConfig);
            WriteByte(Register.Control3, gyroscopeConfig);

            // CTRL5: gLPF_EN (bit 4) and aLPF_EN (bit 0) enable both filters;
            // gLPF_MODE[6:5] = aLPF_MODE[2:1] = 10 selects bandwidths of 5.32% of ODR.
            WriteByte(Register.Control5, 0x55);

            // CTRL7 bits 1:0 enable the gyroscope and accelerometer; syncSmpl remains clear.
            WriteByte(Register.Control7, 0x03);

            // Allow 60 ms gyro wakeup + 3 / 31.25 Hz filter settling = 156 ms, with margin.
            Thread.Sleep(200);
            _accelerometerRange = accelerometerRange;
            _gyroscopeRange = gyroscopeRange;
            _accelerometerOdr = accelerometerOdr;
            _gyroscopeOdr = gyroscopeOdr;
            _configured = true;
        }

        /// <summary>
        /// Reads acceleration in g in the sensor's native axis orientation.
        /// </summary>
        /// <returns>The acceleration vector, including gravity.</returns>
        /// <exception cref="InvalidOperationException">The last configuration attempt failed.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Telemetry]
        public Vector3 GetAccelerometer()
        {
            EnsureConfigured();
            Read(Register.AccelerometerXLow, new SpanByte(_dataBuffer, 0, 6));
            return DecodeVector(new SpanByte(_dataBuffer, 0, 6), GetAccelerometerResolution(_accelerometerRange));
        }

        /// <summary>
        /// Reads angular rate in degrees per second in the sensor's native axis orientation.
        /// </summary>
        /// <returns>The angular rate vector.</returns>
        /// <exception cref="InvalidOperationException">The last configuration attempt failed.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Telemetry]
        public Vector3 GetGyroscope()
        {
            EnsureConfigured();
            Read(Register.GyroscopeXLow, new SpanByte(_dataBuffer, 0, 6));
            return DecodeVector(new SpanByte(_dataBuffer, 0, 6), GetGyroscopeResolution(_gyroscopeRange));
        }

        /// <summary>
        /// Reads both sensor vectors in one 12-byte burst without waiting for a new sample.
        /// </summary>
        /// <param name="acceleration">Acceleration in g, including gravity.</param>
        /// <param name="angularRate">Angular rate in degrees per second.</param>
        /// <remarks>The device uses asynchronous output updates; this is not a synchronized sampling API.</remarks>
        /// <exception cref="InvalidOperationException">The last configuration attempt failed.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        public void ReadAccelerationAndGyroscope(out Vector3 acceleration, out Vector3 angularRate)
        {
            EnsureConfigured();
            Read(Register.AccelerometerXLow, _dataBuffer);
            acceleration = DecodeVector(new SpanByte(_dataBuffer, 0, 6), GetAccelerometerResolution(_accelerometerRange));
            angularRate = DecodeVector(new SpanByte(_dataBuffer, 6, 6), GetGyroscopeResolution(_gyroscopeRange));
        }

        /// <summary>
        /// Reads the internal die temperature, not ambient temperature.
        /// </summary>
        /// <returns>The temperature of the sensor die.</returns>
        /// <exception cref="InvalidOperationException">The last configuration attempt failed.</exception>
        /// <exception cref="IOException">An I2C transfer failed.</exception>
        /// <exception cref="ObjectDisposedException">The device has been disposed.</exception>
        [Telemetry]
        public Temperature GetInternalTemperature()
        {
            EnsureConfigured();
            Read(Register.TemperatureLow, new SpanByte(_dataBuffer, 0, 2));
            return DecodeTemperature(new SpanByte(_dataBuffer, 0, 2));
        }

        /// <summary>
        /// Releases the resources used by this device.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2c?.Dispose();
            }

            _i2c = null;
        }

        internal static byte GetAccelerometerConfiguration(AccelerometerRange range, OutputDataRate odr)
        {
            // CTRL2 aFS[6:4] supports codes 0..3; normal-mode aODR[3:0] supports 0..8.
            if ((byte)range > 3 || (byte)odr > 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (byte)(((byte)range << 4) | (byte)odr);
        }

        internal static byte GetGyroscopeConfiguration(GyroscopeRange range, OutputDataRate odr)
        {
            // CTRL3 gFS[6:4] supports codes 0..7; normal-mode gODR[3:0] supports 0..8.
            if ((byte)range > 7 || (byte)odr > 8)
            {
                throw new ArgumentOutOfRangeException();
            }

            return (byte)(((byte)range << 4) | (byte)odr);
        }

        internal static double GetAccelerometerResolution(AccelerometerRange range)
        {
            GetAccelerometerConfiguration(range, OutputDataRate.Odr250Hz);

            // Full-scale starts at 2 g and doubles per range code; signed 16-bit output spans -32768..32767.
            return (2 << (int)range) / 32768.0;
        }

        internal static double GetGyroscopeResolution(GyroscopeRange range)
        {
            GetGyroscopeConfiguration(range, OutputDataRate.Odr250Hz);

            // Full-scale starts at 16 dps and doubles per range code; signed 16-bit output spans -32768..32767.
            return (16 << (int)range) / 32768.0;
        }

        internal static Vector3 DecodeVector(SpanByte data, double resolution)
        {
            return new Vector3(
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(0, 2)) * resolution,
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(2, 2)) * resolution,
                BinaryPrimitives.ReadInt16LittleEndian(data.Slice(4, 2)) * resolution);
        }

        internal static Temperature DecodeTemperature(SpanByte data)
        {
            // TEMP is signed fixed-point Celsius at 256 LSB per degree, with no offset.
            return Temperature.FromDegreesCelsius(BinaryPrimitives.ReadInt16LittleEndian(data) / 256.0);
        }

        private void EnsureConfigured()
        {
            ThrowIfDisposed();
            if (!_configured)
            {
                throw new InvalidOperationException();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_i2c == null)
            {
                throw new ObjectDisposedException();
            }
        }

        private byte ReadByte(Register register)
        {
            Read(register, _byteBuffer);
            return _byteBuffer[0];
        }

        private void Read(Register register, SpanByte data)
        {
            ThrowIfDisposed();
            _registerBuffer[0] = (byte)register;
            if (_i2c.WriteRead(_registerBuffer, data).Status != I2cTransferStatus.FullTransfer)
            {
                throw new IOException();
            }
        }

        private void WriteByte(Register register, byte value)
        {
            ThrowIfDisposed();
            _writeBuffer[0] = (byte)register;
            _writeBuffer[1] = value;
            if (_i2c.Write(_writeBuffer).Status != I2cTransferStatus.FullTransfer)
            {
                throw new IOException();
            }
        }
    }
}
