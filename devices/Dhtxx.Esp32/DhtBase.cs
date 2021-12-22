﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using nanoFramework.Hardware.Esp32.Rmt;
using System;
using System.Device;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Model;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

namespace Iot.Device.DHTxx.Esp32
{
    /// <summary>
    /// Temperature and Humidity Sensor DHTxx
    /// </summary>
    [Interface("Temperature and Humidity Sensor DHTxx")]
    public abstract class DhtBase : IDisposable
    {
        /// <summary>
        /// Read buffer
        /// </summary>
        protected byte[] _readBuff = new byte[5];

        private readonly CommunicationProtocol _protocol;

        /// <summary>
        /// GPIO pin
        /// </summary>
        protected readonly int _pin;

        /// <summary>
        /// True to dispose the Gpio Controller
        /// </summary>
        protected readonly bool _shouldDispose;

        /// <summary>
        /// I2C device used to communicate with the device
        /// </summary>
        protected I2cDevice? _i2cDevice;

        /// <summary>
        /// <see cref="GpioController"/> related with the <see cref="_pin"/>.
        /// </summary>
        protected GpioController? _controller;

        /// <summary>
        /// The receiver channel used to receive the signals.
        /// </summary>
        protected ReceiverChannel? _rxChannel;

        private DateTime _lastMeasurement = DateTime.UtcNow;

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
        public virtual Temperature Temperature
        {
            get
            {
                ReadData();
                return IsLastReadSuccessful ? GetTemperature(_readBuff) : default(Temperature);
            }
        }

        /// <summary>
        /// Get the last read of relative humidity in percentage
        /// </summary>
        /// <remarks>
        /// If last read was not successful, it returns <code>default(RelativeHumidity)</code>
        /// </remarks>
        [Telemetry]
        public virtual RelativeHumidity Humidity
        {
            get
            {
                ReadData();
                return IsLastReadSuccessful ? GetHumidity(_readBuff) : default(RelativeHumidity);
            }
        }

        /// <summary>
        /// Create a DHT sensor
        /// </summary>
        /// <param name="pinEcho">The pin number which is used as echo (GPIO number)</param>
        /// <param name="pinTrigger">The pin number which is used as trigger (GPIO number)</param>
        /// <param name="pinNumberingScheme">The GPIO pin numbering scheme</param>
        /// <param name="gpioController"><see cref="GpioController"/> related with operations on pins</param>
        /// <param name="shouldDispose"><see langword="true"/> to dispose the <see cref="GpioController"/></param>
        public DhtBase(int pinEcho, int pinTrigger, PinNumberingScheme pinNumberingScheme = PinNumberingScheme.Logical, GpioController? gpioController = null, bool shouldDispose = true)
        {
            _protocol = CommunicationProtocol.OneWire;
            _shouldDispose = shouldDispose || gpioController is null;
            _controller = gpioController ?? new GpioController(pinNumberingScheme);
            _pin = pinTrigger;
            _rxChannel = new ReceiverChannel(pinEcho);
            // 1us clock ( 80Mhz / 80 ) = 1Mhz
            _rxChannel.ClockDivider = 80;
            // no filter
            _rxChannel.EnableFilter(false, 5);
            // max time 1us clock
            _rxChannel.SetIdleThresold(ushort.MaxValue);
            // timeout after 1 second
            _rxChannel.ReceiveTimeout = TimeSpan.FromSeconds(1);

            _controller.OpenPin(_pin, PinMode.Output);
            // delay 1s to make sure DHT stable
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Create a DHT sensor through I2C (Only DHT12)
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public DhtBase(I2cDevice i2cDevice)
        {
            _protocol = CommunicationProtocol.I2C;
            _i2cDevice = i2cDevice;
        }

        /// <summary>
        /// Start a reading
        /// </summary>
        internal virtual void ReadData()
        {
            // The time of two measurements should be more than 1s.
            if (DateTime.UtcNow - _lastMeasurement < TimeSpan.FromSeconds(1))
            {
                return;
            }

            if (_protocol == CommunicationProtocol.OneWire)
            {
                ReadThroughOneWire();
            }
            else
            {
                ReadThroughI2c();
            }
        }

        /// <summary>
        /// Read through One-Wire
        /// </summary>
        internal virtual void ReadThroughOneWire()
        {
            if ((_controller is null) || (_rxChannel is null))
            {
                throw new Exception("GPIO controller or RMT receiver is not configured.");
            }

            RmtCommand[] response;
            byte readVal = 0;

            // keep data line HIGH
            _controller.SetPinMode(_pin, PinMode.Output);
            _rxChannel.Start(true);
            _controller.Write(_pin, PinValue.High);
            DelayHelper.DelayMilliseconds(20, true);

            // send trigger signal
            _controller.Write(_pin, PinValue.Low);
            // wait at least 18 milliseconds
            // here wait for 18 milliseconds will cause sensor initialization to fail
            DelayHelper.DelayMilliseconds(20, true);

            // pull up data line
            // wait 20 - 40 microseconds
            _controller.SetPinMode(_pin, PinMode.InputPullUp);

            // Receive everything
            response = _rxChannel.GetAllItems();
            _rxChannel.Stop();
            // Set back to pull up
            _controller.SetPinMode(_pin, PinMode.Output);

            // We will read 43 elements. The first 1 is the large pulse
            // The second one the small puls and the fisrt 80 micro second one
            // The thrid one the second micro second element
            if ((response != null) && (response.Length >= 43))
            {
                // the read data contains 40 bits
                for (int i = 0; i < 40; i++)
                {
                    readVal <<= 1;
                    // It's a 1 if the timing if 70 micro seconds, 0 if between 26 and 28
                    if (response[i + 3].Duration0 > 50)
                    {
                        readVal |= 1;
                    }

                    if (((i + 1) % 8) == 0)
                    {
                        _readBuff[i / 8] = readVal;
                    }
                }

                _lastMeasurement = DateTime.UtcNow;

                if ((_readBuff[4] == ((_readBuff[0] + _readBuff[1] + _readBuff[2] + _readBuff[3]) & 0xFF)))
                {
                    IsLastReadSuccessful = (_readBuff[0] != 0) || (_readBuff[2] != 0);
                }
                else
                {
                    IsLastReadSuccessful = false;
                }
            }
        }

        /// <summary>
        /// Read through I2C
        /// </summary>
        internal virtual void ReadThroughI2c()
        {
            if (_i2cDevice is null)
            {
                throw new Exception("I2C device is not configured");
            }

            // DHT12 Humidity Register
            _i2cDevice.WriteByte(0x00);
            // humidity int, humidity decimal, temperature int, temperature decimal, checksum
            _i2cDevice.Read(_readBuff);

            _lastMeasurement = DateTime.UtcNow;

            if ((_readBuff[4] == ((_readBuff[0] + _readBuff[1] + _readBuff[2] + _readBuff[3]) & 0xFF)))
            {
                IsLastReadSuccessful = (_readBuff[0] != 0) || (_readBuff[2] != 0);
            }
            else
            {
                IsLastReadSuccessful = false;
            }
        }

        /// <summary>
        /// Converting data to humidity
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Humidity</returns>
        internal abstract RelativeHumidity GetHumidity(byte[] readBuff);

        /// <summary>
        /// Converting data to Temperature
        /// </summary>
        /// <param name="readBuff">Data</param>
        /// <returns>Temperature</returns>
        internal abstract Temperature GetTemperature(byte[] readBuff);

        /// <inheritdoc/>
        public void Dispose()
        {
            _rxChannel?.Dispose();
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
            else if (_controller != null && _controller.IsPinOpen(_pin))
            {
                _controller.ClosePin(_pin);
            }

            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
