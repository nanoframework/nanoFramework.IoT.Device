// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Sht3x
{
    /// <summary>
    /// Humidity and Temperature Sensor SHT3x.
    /// </summary>
    [Interface("Humidity and Temperature Sensor SHT3x")]
    public class Sht3x : IDisposable
    {
        // CRC const
        private const byte CrcPolynomial = 0x31;
        private const byte CrcInit = 0xFF;

        private I2cDevice _i2cDevice;

        #region prop

        /// <summary>
        /// Gets or setsSHT3x Resolution.
        /// </summary>
        [Property]
        public Resolution Resolution { get; set; }

        private double _temperature;

        /// <summary>
        /// Gets SHT3x Temperature.
        /// </summary>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                ReadTempAndHumidity();
                return Temperature.FromDegreesCelsius(_temperature);
            }
        }

        private double _humidity;

        /// <summary>
        /// Gets SHT3x Relative Humidity (%).
        /// </summary>
        [Telemetry]
        public RelativeHumidity Humidity
        {
            get
            {
                ReadTempAndHumidity();
                return RelativeHumidity.FromPercent(_humidity);
            }
        }

        private bool _heater;

        /// <summary>
        /// Gets or sets a value indicating whether SHT3x Heater is on.
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

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Sht3x" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="resolution">SHT3x Read Resolution.</param>
        public Sht3x(I2cDevice i2cDevice, Resolution resolution = Resolution.High)
        {
            _i2cDevice = i2cDevice;

            Resolution = resolution;

            Reset();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }

        /// <summary>
        /// SHT3x Soft Reset.
        /// </summary>
        public void Reset() =>
            Write(Register.SHT_RESET);

        /// <summary>
        /// Set SHT3x Heater.
        /// </summary>
        /// <param name="isOn">Heater on when value is true.</param>
        private void SetHeater(bool isOn)
        {
            if (isOn)
            {
                Write(Register.SHT_HEATER_ENABLE);
            }
            else
            {
                Write(Register.SHT_HEATER_DISABLE);
            }
        }

        /// <summary>
        /// Read Temperature and Humidity.
        /// </summary>
        private void ReadTempAndHumidity()
        {
            SpanByte writeBuff = new byte[]
            {
                (byte)Register.SHT_MEAS, (byte)Resolution
            };
            SpanByte readBuff = new byte[6];

            _i2cDevice.Write(writeBuff);

            // wait SCL free
            Thread.Sleep(20);
            _i2cDevice.Read(readBuff);

            // Details in the Datasheet P13
            int st = (readBuff[0] << 8) | readBuff[1];      // Temp
            int srh = (readBuff[3] << 8) | readBuff[4];     // Humi

            // check 8-bit crc
            bool tCrc = CheckCrc8(readBuff.Slice(0, 2), readBuff[2]);
            bool rhCrc = CheckCrc8(readBuff.Slice(3, 2), readBuff[5]);
            if (tCrc == false || rhCrc == false)
            {
                return;
            }

            // Details in the Datasheet P13
            _temperature = Math.Round(((st * 175 / 65535.0) - 45) * 10) / 10.0;
            _humidity = Math.Round((srh * 100 / 65535.0) * 10) / 10.0;
        }

        /// <summary>
        /// 8-bit CRC Checksum Calculation.
        /// </summary>
        /// <param name="data">Raw Data.</param>
        /// <param name="crc8">Raw CRC8.</param>
        /// <returns>Checksum is true or false.</returns>
        private bool CheckCrc8(SpanByte data, byte crc8)
        {
            // Details in the Datasheet P13
            byte crc = CrcInit;
            for (int i = 0; i < 2; i++)
            {
                crc ^= data[i];

                for (int j = 8; j > 0; j--)
                {
                    if ((crc & 0x80) != 0)
                    {
                        crc = (byte)((crc << 1) ^ CrcPolynomial);
                    }
                    else
                    {
                        crc = (byte)(crc << 1);
                    }
                }
            }

            return crc == crc8;
        }

        private void Write(Register register)
        {
            byte msb = (byte)((short)register >> 8);
            byte lsb = (byte)((short)register & 0xFF);

            SpanByte writeBuff = new byte[]
            {
                msb, lsb
            };

            _i2cDevice.Write(writeBuff);

            // wait SCL free
            Thread.Sleep(20);
        }
    }
}
