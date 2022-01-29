﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Device.Model;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Am2320
{
    /// <summary>
    /// AM2320 - Temperature and Humidity sensor
    /// </summary>
    [Interface("AM2320 - Temperature and Humidity sensor")]
    public class Am2320 : IDisposable
    {
        private readonly I2cDevice _i2c;
        private readonly byte[] _readBuff = new byte[8];

        /// <summary>
        /// AM3220 default I2C address
        /// </summary>
        public const int DefaultI2cAddress = 0x5C;

        /// <summary>
        /// The minimum read period is 1.5 seconds. Do not read the sensor more often.
        /// </summary>
        public static readonly TimeSpan MinimumReadPeriod = new TimeSpan(0, 0, 0, 1, 500);

        private DateTime _lastMeasurement = DateTime.UtcNow.Subtract(MinimumReadPeriod);

        /// <summary>
        /// How last read went, <c>true</c> for success, <c>false</c> for failure
        /// </summary>
        public bool IsLastReadSuccessful { get; internal set; }

        /// <summary>
        /// Get the last read temperature
        /// </summary>
        /// <remarks>
        /// If last read was not successful, it returns <code>default(Temperature)</code>
        /// </remarks>
        [Telemetry]
        public Temperature Temperature
        {
            get
            {
                ReadData();
                return IsLastReadSuccessful ? GetTemperature() : default(Temperature);
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successful, it returns <code>default(RelativeHumidity)</code>
        /// </remarks>
        [Telemetry]
        public RelativeHumidity Humidity
        {
            get
            {
                ReadData();
                return IsLastReadSuccessful ? GetHumidity() : default(RelativeHumidity);
            }
        }

        /// <summary>
        /// Gets the device information.
        /// </summary>
        [Property]
        public DeviceInformation DeviceInformation
        {
            get
            {
                SpanByte buff = new byte[11];
                // 32 bit device ID
                // Forces the sensor to wake up and wait 10 ms according to documentation
                _i2c.WriteByte(0x00);
                Thread.Sleep(10);
                // Sending functin code 0x03, start regster 0x00 and 4 registers
                _i2c.Write(new byte[] { 0x03, 0x08, 0x07 });
                // Wait at least 30 micro seconds
                Thread.Sleep(1);
                _i2c.Read(buff);
                // Check if it is valid
                if (!IsValidReadBuffer(0x04))
                {                    
                    return null;
                }

                if (!IsCrcValid(buff.Slice(0, 9)))
                {                    
                    return null;
                }

                return new DeviceInformation()
                {
                    Model = BinaryPrimitives.ReadUInt16BigEndian(buff.Slice(2)),
                    Version = buff[4],
                    DeviceId = BinaryPrimitives.ReadUInt32BigEndian(buff.Slice(5)),
                };
            }
        }

        /// <summary>
        /// Creates an AM2320.
        /// </summary>
        /// <param name="i2c">The <see cref="I2cDevice"/>.</param>
        /// <remarks>This sensor only works on <see cref="I2cBusSpeed.StandardMode"/> speed.</remarks>
        public Am2320(I2cDevice i2c)
        {
            _i2c = i2c ?? throw new ArgumentNullException(nameof(i2c));
        }

        private void ReadData()
        {
            // Doc says, we can't read more often than 1.5 seconds
            if (_lastMeasurement.Add(MinimumReadPeriod) < DateTime.UtcNow)
            {
                IsLastReadSuccessful = false;
                return;
            }

            // Forces the sensor to wake up and wait 10 ms according to documentation
            _i2c.WriteByte(0x00);
            Thread.Sleep(10);
            // Sending functin code 0x03, start regster 0x00 and 4 registers
            _i2c.Write(new byte[] { 0x03, 0x00, 0x04 });
            // Wait at least 30 micro seconds
            Thread.Sleep(1);
            _i2c.Read(_readBuff);
            _lastMeasurement = DateTime.UtcNow;
            // Check if sucessfull read
            if (!IsValidReadBuffer(0x04))
            {
                IsLastReadSuccessful = false;
                return;
            }

            if (!IsCrcValid((new SpanByte(_readBuff)).Slice(0, 6)))
            {
                IsLastReadSuccessful = false;
                return;
            }

            IsLastReadSuccessful = true;
        }

        private bool IsValidReadBuffer(byte expected) => (_readBuff[0] == 0x03) && (_readBuff[1] == expected);

        private bool IsCrcValid(SpanByte buff)
        {
            var crc = Crc16(buff);
            return (crc >> 8 == _readBuff[6]) && ((crc & 0xFF) == _readBuff[7]);
        }

        private Temperature GetTemperature()
        {
            short temp = BinaryPrimitives.ReadInt16BigEndian((new SpanByte(_readBuff)).Slice(2));
            return Temperature.FromDegreesCelsius(temp / 10.0);
        }

        private RelativeHumidity GetHumidity()
        {
            short hum = BinaryPrimitives.ReadInt16BigEndian((new SpanByte(_readBuff)).Slice(4));
            return RelativeHumidity.FromPercent(hum / 10.0);
        }

        private ushort Crc16(SpanByte ptr)
        {
            ushort crc = 0xFFFF;
            byte i;
            byte inc = 0;
            while (inc < ptr.Length)
            {
                crc = ptr[inc++];
                for (i = 0; i < 8; i++)
                {
                    if ((crc & 0x01) == 0x01)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }

            return crc;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2c?.Dispose();
        }
    }
}
