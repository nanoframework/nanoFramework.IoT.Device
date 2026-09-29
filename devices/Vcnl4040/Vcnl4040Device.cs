// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Vcnl4040.Definitions;
using Iot.Device.Vcnl4040.Internal;

namespace Iot.Device.Vcnl4040
{
    /// <summary>
    /// Represents a VCNL4040 device.
    /// </summary>
    public class Vcnl4040Device : IDisposable
    {
        /// <summary>
        /// I2C bus address.
        /// </summary>
        public const int DefaultI2cAddress = 0x60;

        private const int CompatibleDeviceId = 0x0186;
        private readonly InterruptFlagRegister _interruptFlagRegister;
        private readonly IdRegister _idRegister;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Gets the ambient light sensor of the VCNL4040 device.
        /// </summary>
        public AmbientLightSensor AmbientLightSensor { get; }

        /// <summary>
        /// Gets the proximity sensor of the VCNL4040 device.
        /// </summary>
        public ProximitySensor ProximitySensor { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vcnl4040Device"/> class.
        /// It checks communication and compatibility basing on the device identifier.
        /// After that it resets the device by setting all registers to the default values.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <see langword="null"/>.</exception>
        public Vcnl4040Device(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();

            AmbientLightSensor = new AmbientLightSensor(_i2cDevice);
            ProximitySensor = new ProximitySensor(_i2cDevice);

            _interruptFlagRegister = new InterruptFlagRegister(_i2cDevice);
            _idRegister = new IdRegister(_i2cDevice);

            VerifyDevice();
            Reset();
        }

        /// <summary>
        /// Resets the device to defaults.
        /// </summary>
        public void Reset()
        {
            WriteRegister(CommandCode.ALS_THDL, 0x00, 0x00);
            WriteRegister(CommandCode.ALS_THDH, 0x00, 0x00);
            WriteRegister(CommandCode.PS_THDL, 0x00, 0x00);
            WriteRegister(CommandCode.PS_THDH, 0x00, 0x00);
            WriteRegister(CommandCode.PS_CANC, 0x00, 0x00);
            WriteRegister(CommandCode.PS_CONF_3_MS, 0x00, 0x00);
            WriteRegister(CommandCode.ALS_CONF, 0x01, 0x00);
            WriteRegister(CommandCode.PS_CONF_1_2, 0x01, 0x00);

            // Clear interrupt flags by reading.
            _interruptFlagRegister.Read();
        }

        /// <summary>
        /// Verifies whether a functional I2C connection to the device exists and checks the identification for
        /// device recognition. An exception is raised, if the communication doesn't work or the identification is incorrect.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when I2C communication fails.</exception>
        /// <exception cref="NotSupportedException">Incompatible device detected.</exception>
        public void VerifyDevice()
        {
            _idRegister.Read();
            if (_idRegister.Id != CompatibleDeviceId)
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_i2cDevice != null)
            {
                _i2cDevice.Dispose();
                _i2cDevice = null;
            }
        }

        /// <summary>
        /// Gets the device identifier.
        /// </summary>
        public int DeviceId
        {
            get
            {
                _idRegister.Read();
                return _idRegister.Id;
            }
        }

        /// <summary>
        /// Gets and clears (by reading) the interrupt flags.
        /// </summary>
        /// <returns>The interrupt flags.</returns>
        public InterruptFlags GetAndClearInterruptFlags()
        {
            _interruptFlagRegister.Read();
            InterruptFlags flags = new InterruptFlags(
                _interruptFlagRegister.PsSpFlag,
                _interruptFlagRegister.AlsIfL,
                _interruptFlagRegister.AlsIfH,
                _interruptFlagRegister.PsIfClose,
                _interruptFlagRegister.PsIfAway);
            return flags;
        }

        private void WriteRegister(CommandCode commandCode, byte dataLow, byte dataHigh)
        {
            SpanByte data = new byte[] { (byte)commandCode, dataLow, dataHigh };
            I2cTransferResult result = _i2cDevice.Write(data);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
