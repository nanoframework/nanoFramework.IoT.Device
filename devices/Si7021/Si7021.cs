// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Si7021
{
    /// <summary>
    /// Temperature and Humidity Sensor Si7021.
    /// </summary>
    [Interface("Temperature and Humidity Sensor Si7021")]
    public class Si7021 : IDisposable
    {
        private const byte SerialNumberLenght = 8;
        private const byte FwRevisionV20 = 0x20;
        private const byte FwRevisionV10 = 0xFF;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Si7021 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x40;

        /// <summary>
        /// Si7021 Temperature [°C].
        /// </summary>
        [Telemetry]
        public Temperature Temperature => Temperature.FromDegreesCelsius(GetTemperature());

        /// <summary>
        /// Relative Humidity.
        /// </summary>
        [Telemetry]
        public RelativeHumidity Humidity => GetHumidity();

        /// <summary>
        /// Si7021 Firmware Revision.
        /// </summary>
        [Property]
        public Version Revision => GetRevision();

        /// <summary>
        /// Gets or sets measurement resolution.
        /// </summary>
        [Property]
        public Resolution Resolution { get => GetResolution(); set => SetResolution(value); }

        private bool _heater;

        /// <summary>
        /// Gets or sets a value indicating whether heater is on.
        /// </summary>
        [Property]
        public bool Heater
        {
            get => _heater;
            set
            {
                SetHeater(value);
                _heater = value;
            }
        }

        /// <summary>
        /// Gets individualized serial number of the Si7021.
        /// </summary>
        public byte[] SerialNumber { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Si7021" /> class.
        /// </summary>
        /// <param name="i2cDevice"><see cref="I2cDevice"/> to communicate with Si7021 device.</param>
        /// <param name="resolution">Si7021 Read Resolution.</param>
        public Si7021(I2cDevice i2cDevice, Resolution resolution = Resolution.Resolution1)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

            // read electronic serial number
            ReadElectronicSerialNumber();

            SetResolution(resolution);
        }

        /// <summary>
        /// Read electronic serial number.
        /// </summary>
        private void ReadElectronicSerialNumber()
        {
            SerialNumber = new byte[SerialNumberLenght];

            // setup reading of 1st byte
            Span<byte> writeBuff = new byte[2]
            {
                (byte)Command.SI_READ_Electronic_ID_1_1, (byte)Command.SI_READ_Electronic_ID_1_2
            };

            _i2cDevice.Write(writeBuff);

            // read 1st half and store in the initial half of the array
            _ = _i2cDevice.Read(new Span<byte>(SerialNumber, 0, 4));

            writeBuff = new byte[2]
            {
                (byte)Command.SI_READ_Electronic_ID_2_1, (byte)Command.SI_READ_Electronic_ID_2_2
            };

            _i2cDevice.Write(writeBuff);

            // read 2nd half and store in the respective half of the array
            _ = _i2cDevice.Read(new Span<byte>(SerialNumber, 3, 4));
        }

        /// <summary>
        /// Get Si7021 Temperature [°C].
        /// </summary>
        /// <returns>Temperature [°C].</returns>
        private double GetTemperature()
        {
            Span<byte> readbuff = new byte[2];

            // Send temperature command, read back two bytes
            _ = _i2cDevice.WriteByte((byte)Command.SI_TEMP);

            // wait for conversion to complete: tCONV(T) max 11ms)
            Thread.Sleep(10);

            _ = _i2cDevice.Read(readbuff);

            // Calculate temperature
            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readbuff);
            double temp = ((175.72 * raw) / 65536.0) - 46.85;

            return Math.Round(temp * 10) / 10.0;
        }

        /// <summary>
        /// Get Si7021 Relative Humidity (%).
        /// </summary>
        /// <returns>Relative Humidity (%).</returns>
        private RelativeHumidity GetHumidity()
        {
            Span<byte> readbuff = new byte[2];

            // Send humidity read command, read back two bytes
            _ = _i2cDevice.WriteByte((byte)Command.SI_HUMI);

            // wait for conversion to complete: tCONV(RH) + tCONV(T) max 20ms
            Thread.Sleep(20);

            _ = _i2cDevice.Read(readbuff);

            // Calculate humidity
            ushort raw = BinaryPrimitives.ReadUInt16BigEndian(readbuff);
            double humidity = ((125 * raw) / 65536.0) - 6;

            return RelativeHumidity.FromPercent(humidity);
        }

        /// <summary>
        /// Get Si7021 firmware revision.
        /// </summary>
        /// <returns>The FirmwareRevision.</returns>
        private Version GetRevision()
        {
            Span<byte> writeBuff = new byte[2]
            {
                (byte)Command.SI_REVISION_MSB, (byte)Command.SI_REVISION_LSB
            };

            _i2cDevice.Write(writeBuff);

            var fwRevision = _i2cDevice.ReadByte();

            if (fwRevision == FwRevisionV20)
            {
                return new Version(2, 0);
            }
            else if (fwRevision == FwRevisionV10)
            {
                return new Version(1, 0);
            }

            return new Version(0, 0);
        }

        /// <summary>
        /// Set Si7021 Measurement Resolution.
        /// </summary>
        /// <param name="resolution">Measurement Resolution.</param>
        private void SetResolution(Resolution resolution)
        {
            byte reg1 = GetUserRegister1();

            reg1 &= 0b0111_1110;

            // Details in the Datasheet P25
            reg1 = (byte)(reg1 | ((byte)resolution & 0b01) | (((byte)resolution & 0b10) >> 1 << 7));

            Span<byte> writeBuff = new byte[2]
            {
                (byte)Command.SI_USER_REG1_WRITE, reg1
            };

            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Get Si7021 Measurement Resolution.
        /// </summary>
        /// <returns>Measurement Resolution.</returns>
        private Resolution GetResolution()
        {
            byte reg1 = GetUserRegister1();

            byte bit0 = (byte)(reg1 & 0b0000_0001);
            byte bit1 = (byte)((reg1 & 0b1000_0000) >> 7);

            return (Resolution)(bit1 << 1 | bit0);
        }

        /// <summary>
        /// Set Si7021 Heater.
        /// </summary>
        /// <param name="isOn">Heater on when value is true.</param>
        private void SetHeater(bool isOn)
        {
            byte reg1 = GetUserRegister1();

            if (isOn)
            {
                reg1 |= 0b0100;
            }
            else
            {
                reg1 &= 0b1111_1011;
            }

            Span<byte> writeBuff = new byte[2]
            {
                (byte)Command.SI_USER_REG1_WRITE, reg1
            };

            _i2cDevice.Write(writeBuff);
        }

        /// <summary>
        /// Get User Register 1.
        /// </summary>
        /// <returns>Content of User Register 1.</returns>
        private byte GetUserRegister1()
        {
            _i2cDevice.WriteByte((byte)Command.SI_USER_REG1_READ);

            return _i2cDevice.ReadByte();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
