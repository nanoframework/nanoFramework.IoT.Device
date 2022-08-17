// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Model;
using System.Threading;
using nanoFramework.Device.OneWire;
using UnitsNet;

namespace Iot.Device.Ds18b20
{
    /// <summary>
    /// DS18B20 - Temperature sensor.
    /// </summary>
    [Interface("DS18B20 - Temperature sensor")]
    public class Ds18b20
    {
        private const float ErrorTemperature = -999.99F;

        private const byte ParasitePoweringModeValue = 0x00;

        private OneWireHost _oneWireHost = null;

        private bool _isTrackingChanges = false;

        private double _tempLowAlarm = 0;

        private double _tempHighAlarm = 0;

        private Thread _changeTracker = null;

        private float _temperatureInCelsius = 0;

        private bool _manyDevicesConnected = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds18b20"/> class.
        /// </summary>
        /// <param name="oneWireHost">One wire host (logical bus) to use.</param>
        /// <param name="deviceAddress">The device address (if null, then this device will search for one on the bus and latch on to the first one found).</param>
        /// <param name="manyDevicesConnected">True for more than one sensor connected to bus.</param>
        /// <param name="temperatureResolution">Sensor resolution.</param>
        /// <exception cref="ArgumentException">When device address length is not equal to 8 characters.</exception>
        /// <exception cref="ArgumentException">When device address is not Ds18b20 sensors family address.</exception>
        public Ds18b20(OneWireHost oneWireHost, byte[] deviceAddress = null, bool manyDevicesConnected = false, TemperatureResolution temperatureResolution = TemperatureResolution.VeryHigh)
        {
            _oneWireHost = oneWireHost;
            TemperatureResolution = temperatureResolution;
            _manyDevicesConnected = manyDevicesConnected;

            if (deviceAddress != null)
            {
                if (deviceAddress.Length != 8)
                {
                    throw new ArgumentException();
                }

                if (deviceAddress[0] != (byte)RomCommands.FamilyCode)
                {
                    throw new ArgumentException();
                }

                Address = deviceAddress;
            }

            _temperatureInCelsius = ErrorTemperature;
            TemperatureHighAlarm = Temperature.FromDegreesCelsius(30);
            TemperatureLowAlarm = Temperature.FromDegreesCelsius(20);
        }

        /// <summary>
        /// Gets or sets temperature resolution.
        /// </summary>
        [Property("TemperatureResolution")]
        public TemperatureResolution TemperatureResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether alarm search mode is enabled.
        /// </summary>
        [Property("IsAlarmSearchCommandEnabled")]
        public bool IsAlarmSearchCommandEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets high temperature alarm. Min -55C, Max 125C.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When high alarm temperature is not between -55 and 125 degrees.</exception>
        [Property("TemperatureHighAlarm")]
        public Temperature TemperatureHighAlarm
        {
            get
            {
                return Temperature.FromDegreesCelsius(_tempHighAlarm);
            }

            set
            {
                if (value.DegreesCelsius < -55 || value.DegreesCelsius > 125)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _tempHighAlarm = value.DegreesCelsius;
            }
        }

        /// <summary>
        /// Gets or sets low temperature alarm. Min -55C, Max 125C.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When low alarm temperature is not between -55 and 125 degrees.</exception>
        [Property("TemperatureLowAlarm")]
        public Temperature TemperatureLowAlarm
        {
            get
            {
                return Temperature.FromDegreesCelsius(_tempLowAlarm);
            }

            set
            {
                if (value.DegreesCelsius < -55 || value.DegreesCelsius > 125)
                {
                    throw new ArgumentOutOfRangeException();
                }

                _tempLowAlarm = value.DegreesCelsius;
            }
        }

        /// <summary>
        /// Gets a value indicating whether it's powered as parasite of the board.
        /// </summary>
        [Property("IsParasitePowered")]
        public bool IsParasitePowered
        {
            get
            {
                SelectDevice();

                var verify = _oneWireHost.WriteByte((byte)FunctionCommands.ReadPowerSupply);

                return _oneWireHost.ReadByte() == ParasitePoweringModeValue;
            }
        }

        /// <summary>
        /// Delegate that defines method signature that will be called
        /// when sensor value change event happens.
        /// </summary>
        /// <param name="temperature">
        /// Contains the measured temperature if the sensor reacted on request.
        /// Contains -999.99 otherwise.
        /// </param>
        public delegate void OnSensorChanged(Temperature temperature);

        /// <summary>
        /// Event that is called when the sensor value changes.
        /// </summary>
        public event OnSensorChanged SensorValueChanged;

        /// <summary>
        /// Gets or sets the 8-byte address of selected device 
        /// (since there could be more than one such devices on the bus).
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Gets an array of addresses of all Ds18B20 devices on network or only
        /// devices in alarm if alarm mode is set on.
        /// </summary>
        public byte[][] AddressNet { get; private set; }

        /// <summary>
        /// Initializes the sensor. This step will perform a reset of the 1-wire bus.
        /// It will check for existence of a 1-wire device. If no address was provided, then the
        /// 1-wire bus will be searched and the first device that matches the family code will be latched on to.
        /// Developer should check for successful initialization by checking the value returned. 
        /// If ManyDevicesConnected is set it will keep searching until find last device, saving all in AddressNet array.
        /// </summary>
        /// <returns><b>true</b> on success, else <b>false</b></returns>
        public bool Initialize()
        {
            int foundDevices = 0;
            ArrayList allDevices = new ArrayList();

            _oneWireHost.TouchReset();

            if (Address == null)
            {
                if (_manyDevicesConnected)
                {
                    if (_oneWireHost.FindFirstDevice(false, IsAlarmSearchCommandEnabled))
                    {
                        do
                        {
                            if (_oneWireHost.SerialNumber[0] == (byte)RomCommands.FamilyCode)
                            {
                                _oneWireHost.TouchReset();
                                Address = new byte[_oneWireHost.SerialNumber.Length];
                                Array.Copy(_oneWireHost.SerialNumber, Address, _oneWireHost.SerialNumber.Length);
                                foundDevices++;
                                allDevices.Add(Address);
                            }
                        }
                        while (_oneWireHost.FindNextDevice(false, IsAlarmSearchCommandEnabled));
                    }

                    if (foundDevices > 0)
                    {
                        AddressNet = new byte[foundDevices][];
                        int i = 0;
                        foreach (byte[] device in allDevices)
                        {
                            AddressNet[i] = new byte[device.Length];
                            Array.Copy(device, AddressNet[i], device.Length);
                            i++;
                        }
                    }
                }
                else
                {
                    if (_oneWireHost.FindFirstDevice(true, IsAlarmSearchCommandEnabled))
                    {
                        do
                        {
                            if (_oneWireHost.SerialNumber[0] == (byte)RomCommands.FamilyCode)
                            {
                                Address = new byte[_oneWireHost.SerialNumber.Length];
                                Array.Copy(_oneWireHost.SerialNumber, Address, _oneWireHost.SerialNumber.Length);
                                foundDevices = 1;
                                break;
                            }
                        }
                        while (_oneWireHost.FindNextDevice(true, IsAlarmSearchCommandEnabled));
                    }
                }
            }

            return foundDevices > 0;
        }

        /// <summary>
        /// Reads the temperature. A return value indicates whether the reading succeeded.
        /// </summary>
        /// <param name="temperature">
        /// Contains the measured temperature if the sensor reacted on request.
        /// Contains -999.99 otherwise.
        /// </param>
        /// <returns><code>true</code> if measurement was not skipped, otherwise <code>false</code>.</returns>
        [Telemetry("Temperature")]
        public bool TryReadTemperature(out Temperature temperature) => Read(out temperature);

        private bool Read(out Temperature temperature)
        {
            PrepareToRead();

            SelectDevice();

            var verify = _oneWireHost.WriteByte((byte)FunctionCommands.ReadScratchpad);
            if (verify == 0)
            {
                temperature = Temperature.FromDegreesCelsius(ErrorTemperature);
                return false;
            }

            var tempLo = _oneWireHost.ReadByte();
            var tempHi = _oneWireHost.ReadByte();

            if (_oneWireHost.TouchReset())
            {
                var temp = (tempHi << 8) | tempLo;

                // Bits manipulation to represent negative values correctly.
                if ((tempHi >> 7) == 1)
                {
                    temp = temp | unchecked((int)0xffff0000);
                }

                _temperatureInCelsius = ((float)temp) / 16;
                temperature = Temperature.FromDegreesCelsius(_temperatureInCelsius);
                return true;
            }
            else
            {
                _temperatureInCelsius = ErrorTemperature;
                temperature = Temperature.FromDegreesCelsius(_temperatureInCelsius);
                return false;
            }
        }

        /// <summary>
        /// Reset the sensor.
        /// </summary>
        public void Reset()
        {
            _oneWireHost.TouchReset();
            _temperatureInCelsius = ErrorTemperature;
        }

        /// <summary>
        /// Search for alarm condition.
        /// </summary>
        /// <returns><b>true</b> on success, else <b>false</b></returns>
        public bool SearchForAlarmCondition()
        {
            Address = null;

            ConvertTemperature();
            return Initialize();
        }

        /// <summary>
        /// Reads sensor configuration.
        /// </summary>
        /// <param name="restoreRegisterFromEEPROM">Flag indicating if configuration should be restored from EEPROM.</param>
        /// <returns><b>true</b> on success, else <b>false</b></returns>
        public bool ConfigurationRead(bool restoreRegisterFromEEPROM = false)
        {
            var verificationValue = 0;

            if (restoreRegisterFromEEPROM)
            {
                SelectDevice();
                verificationValue = _oneWireHost.WriteByte((byte)FunctionCommands.RecallAlarmTriggerValues);

                if (verificationValue == 0)
                {
                    return false;
                }

                while (_oneWireHost.ReadByte() == 0)
                {
                    Thread.Sleep(10);
                }
            }

            SelectDevice();
            verificationValue = _oneWireHost.WriteByte((byte)FunctionCommands.ReadScratchpad);

            if (verificationValue == 0)
            {
                return false;
            }

            // Discard temperature bytes
            _oneWireHost.ReadByte();
            _oneWireHost.ReadByte();

            _tempHighAlarm = (sbyte)_oneWireHost.ReadByte();
            TemperatureHighAlarm = Temperature.FromDegreesCelsius(_tempHighAlarm);
            _tempLowAlarm = (sbyte)_oneWireHost.ReadByte();
            TemperatureLowAlarm = Temperature.FromDegreesCelsius(_tempLowAlarm);
            int configReg = _oneWireHost.ReadByte();

            if (_oneWireHost.TouchReset())
            {
                TemperatureResolution = (TemperatureResolution)(configReg >> 5);
            }
            else
            {
                TemperatureResolution = TemperatureResolution.VeryHigh;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes sensor configuration. The unchanged registers will be overwritten.
        /// </summary>
        /// <param name="writeToEEPROM">Flag indicating if configuration should be written also to EEPROM.</param>
        public void ConfigurationWrite(bool writeToEEPROM = false)
        {
            SelectDevice();

            _oneWireHost.WriteByte((byte)FunctionCommands.WriteScratchpad);

            _oneWireHost.WriteByte((byte)_tempHighAlarm);
            _oneWireHost.WriteByte((byte)_tempLowAlarm);
            _oneWireHost.WriteByte((byte)((byte)TemperatureResolution << 5));

            if (writeToEEPROM)
            {
                SelectDevice();
                _oneWireHost.WriteByte((byte)FunctionCommands.CopyScratchpad);
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Starts tracking the changes of temperature.
        /// </summary>
        /// <exception cref="InvalidOperationException">When tracking is already enabled.</exception>
        /// <exception cref="ArgumentOutOfRangeException">When tracking interval is shorter than 50 milliseconds.</exception>
        /// <exception cref="NotSupportedException">When event handler is not assigned.</exception>
        /// <param name="trackingInterval">Interval to track the changes to sensor values.</param>
        public void BeginTrackChanges(TimeSpan trackingInterval)
        {
            if (_isTrackingChanges)
            {
                throw new InvalidOperationException();
            }

            if (trackingInterval.TotalMilliseconds < 50)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (SensorValueChanged == null)
            {
                throw new NotSupportedException();
            }

            _changeTracker = new Thread(() =>
            {
                int divs = (int)(trackingInterval.TotalMilliseconds / 1000);

                while (_isTrackingChanges)
                {
                    if (trackingInterval.TotalMilliseconds > 1000)
                    {
                        divs = (int)(trackingInterval.TotalMilliseconds / 1000);
                        while (_isTrackingChanges && divs > 0)
                        {
                            Thread.Sleep(1000);
                            divs--;
                        }
                    }
                    else
                    {
                        Thread.Sleep(trackingInterval);
                    }

                    if (HasSensorValueChanged() && SensorValueChanged != null)
                    {
                        SensorValueChanged(Temperature.FromDegreesCelsius(_temperatureInCelsius));
                    }
                }
            });
            _isTrackingChanges = true;
            _changeTracker.Start();
        }

        /// <summary>
        /// Stops tracking changes.
        /// </summary>
        public void EndTrackChanges()
        {
            _isTrackingChanges = false;

            // see BeginChangeTracker to know why 3000 is chosen - 3x of lowest wait time
            Thread.Sleep(3000);
            if (_changeTracker.IsAlive)
            {
                try
                {
                    _changeTracker.Abort();
                }
                finally
                {
                    _changeTracker = null;
                }
            }
        }

        private void SelectDevice()
        {
            if (Address != null && Address.Length == 8 && Address[0] == (byte)RomCommands.FamilyCode)
            {
                // do not convert to a for..loop
                byte[] cmdAndData = new byte[9]
                {
                    (byte)RomCommands.Match,
                    Address[0], Address[1], Address[2], Address[3], Address[4], Address[5], Address[6],
                    Address[7]
                };

                _oneWireHost.TouchReset();
                foreach (var b in cmdAndData)
                {
                    _oneWireHost.WriteByte(b);
                }
            }
        }

        private void ConvertTemperature()
        {
            _oneWireHost.TouchReset();
            _oneWireHost.WriteByte((byte)RomCommands.Skip);
            _oneWireHost.WriteByte((byte)FunctionCommands.ConvertTemperature);
            int waitConversion;
            switch (TemperatureResolution)
            {
                case TemperatureResolution.VeryLow:
                    waitConversion = 125;
                    break;
                case TemperatureResolution.Low:
                    waitConversion = 250;
                    break;
                case TemperatureResolution.High:
                    waitConversion = 500;
                    break;
                case TemperatureResolution.VeryHigh:
                default:
                    waitConversion = 1000;
                    break;
            }

            Thread.Sleep(waitConversion);
        }

        private void PrepareToRead()
        {
            if (Address != null && Address.Length == 8 && Address[0] == (byte)RomCommands.FamilyCode)
            {
                ConvertTemperature();
            }
        }

        private bool HasSensorValueChanged()
        {
            float previousTemperature = _temperatureInCelsius;

            var readStatus = Read(out _);
            if (!readStatus)
            {
                return false;
            }

            float currentTemperature = _temperatureInCelsius;

            bool valuesChanged = previousTemperature != currentTemperature;

            return valuesChanged;
        }
    }
}