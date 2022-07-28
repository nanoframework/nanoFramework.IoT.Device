// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Device.Model;
using System.Threading;
using Iot.Device.Ds18b201;
using nanoFramework.Device.OneWire;
using UnitsNet;

namespace Iot.Device.Ds18b20
{
    /// <summary>
    /// DS18B20 - Temperature sensor 
    /// </summary>
    [Interface("DS18B20 - Temperature sensor")]
    public class Ds18b20
    {
        private const float _errorTemperature = -999.99F;

        private const byte _parasitePoweringModeValue = 0x00;

        private OneWireHost _oneWireHost = null;

        private bool _isTrackingChanges = false;

        private double _tempLowAlarm = 0;

        private double _tempHighAlarm = 0;

        private Thread _changeTracker = null;

        private float _temperatureInCelsius = 0;

        private bool _manyDevicesConnected = false;

        /// <summary>
        /// Constructs Ds18b20 instance
        /// </summary>
        /// <param name="oneWireHost">One wire host (logical bus) to use</param>
        /// <param name="deviceAddress">The device address (if null, then this device will search for one on the bus and latch on to the first one found)</param>
        /// <param name="manyDevicesConnected">True for more than one sensor connected to bus</param>
        /// <param name="temperatureResolution">Sensor resolution</param>
        public Ds18b20(OneWireHost oneWireHost, byte[] deviceAddress = null, bool manyDevicesConnected = false,
            TemperatureResolution temperatureResolution = TemperatureResolution.VeryHigh)
        {
            _oneWireHost = oneWireHost;
            TemperatureResolution = temperatureResolution;
            _manyDevicesConnected = manyDevicesConnected;

            if (deviceAddress != null)
            {
                if (deviceAddress.Length != 8) throw new ArgumentException(); //must be 8 bytes
                if (deviceAddress[0] != (byte)RomCommands.FamilyCode)
                {
                    throw new ArgumentException("Device address does not belong to Ds18b20 sensors family.");
                }

                Address = deviceAddress;
            }

            _temperatureInCelsius = _errorTemperature;
            TemperatureHighAlarm = Temperature.FromDegreesCelsius(30);
            TemperatureLowAlarm = Temperature.FromDegreesCelsius(20);
        }

        /// <summary>
        /// Sets or gets temperature resolution
        /// </summary>
        [Property("TemperatureResolution")]
        public TemperatureResolution TemperatureResolution
        {
            get;
            set;
        }

        /// <summary>
        /// Sets or gets value indicating if alarm search mode is enabled. Temperature
        /// </summary>
        [Property("IsAlarmSearchCommandEnabled")]
        public bool IsAlarmSearchCommandEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Sets or gets high temperature alarm. Min -55C, Max 125C
        /// </summary>
        [Property("TemperatureHighAlarm")]
        public Temperature TemperatureHighAlarm
        {
            get
            {
                return Temperature.FromDegreesCelsius(_tempHighAlarm);
            }
            set
            {
                if (value.DegreesCelsius is < -55 or > 125)
                {
                    throw new ArgumentOutOfRangeException(nameof(TemperatureHighAlarm),
                        "High alarm temperature has to be between -55 and 125 degrees");
                }

                _tempHighAlarm = value.DegreesCelsius;
            }
        }

        /// <summary>
        /// Sets or gets low temperature alarm. Min -55C, Max 125C
        /// </summary>
        [Property("TemperatureLowAlarm")]
        public Temperature TemperatureLowAlarm
        {
            get { return Temperature.FromDegreesCelsius(_tempLowAlarm); }
            set
            {
                if (value.DegreesCelsius is < -55 or > 125)
                {
                    throw new ArgumentOutOfRangeException(nameof(TemperatureLowAlarm),
                        "Low alarm temperature has to be between -55 and 125 degrees");
                }

                _tempLowAlarm = value.DegreesCelsius;
            }
        }

        /// <summary>
        /// Returns information if it's powered as parasite of the board.
        /// </summary>
        [Property("IsParasitePowered")]
        public bool IsParasitePowered
        {
            get
            {
                SelectDevice();

                // Now read power supply external | parasite
                var verify = _oneWireHost.WriteByte((byte)FunctionCommands.ReadPowerSupply);

                return _oneWireHost.ReadByte() == _parasitePoweringModeValue;
            }
        }

        /// <summary>
        /// Reads temperature, check data sheet, page 14, 8.6.1 section
        /// </summary>
        /// <returns>
        /// Temperature
        /// </returns>
        [Telemetry("Temperature")]
        public Temperature Temperature => Temperature.FromDegreesCelsius(_temperatureInCelsius);

        /// <summary>
        /// Delegate that defines method signature that will be called
        /// when sensor value change event happens
        /// </summary>
        public delegate void OnSensorChanged();

        /// <summary>
        /// Event that is called when the sensor value changes
        /// </summary>
        public event OnSensorChanged SensorValueChanged;

        /// <summary>
        /// The 8-byte address of selected device 
        /// (since there could be more than one such devices on the bus)
        /// </summary>
        public byte[] Address { get; set; }

        /// <summary>
        /// Contains an array of address of all 18B20 devices on network or only
        /// devices in alarm if mode is set to SEARCH_ALARM 
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
            //ArrayList allDevices;
            ArrayList allDevices = new ArrayList();

            _oneWireHost.TouchReset();

            if (Address == null) //search for a device with the required family code
            {
                //found the device
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
                        while (_oneWireHost.FindNextDevice(false,
                                     IsAlarmSearchCommandEnabled)); //keep searching until we get one
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
                        while (_oneWireHost.FindNextDevice(true,
                                     IsAlarmSearchCommandEnabled)); //keep searching until we get one
                    }
                }
            }

            return foundDevices > 0;
        }



        /// <summary>
        /// Read sensor data
        /// </summary>
        /// <returns><b>true</b> on success, else <b>false</b></returns>
        public bool Read()
        {
            PrepareToRead();

            SelectDevice();

            //now read the scratchpad
            var verify = _oneWireHost.WriteByte((byte)FunctionCommands.ReadScratchpad);
            if (verify == 0)
            {
                return false;
            }

            //Now read the temperature
            var tempLo = _oneWireHost.ReadByte();
            var tempHi = _oneWireHost.ReadByte();

            if (_oneWireHost.TouchReset())
            {
                var temp = ((tempHi << 8) | tempLo);

                // Bits manipulation to represent negative values correctly.
                if ((tempHi >> 7) == 1)
                {
                    temp = (temp | unchecked((int)0xffff0000));
                }

                _temperatureInCelsius = ((float)temp) / 16;
                return true;
            }
            else
            {
                _temperatureInCelsius = _errorTemperature;
                return false;
            }
        }

        /// <summary>
        /// Reset the sensor...this performs a soft reset. To perform a hard reset, the system must be 
        /// power cycled
        /// </summary>
        public void Reset()
        {
            _oneWireHost.TouchReset();
            _temperatureInCelsius = _errorTemperature;
        }

        /// <summary>
        /// Search for alarm condition.
        /// </summary>
        /// <returns>bool</returns>
        public bool SearchForAlarmCondition()
        {
            Address = null;

            ConvertTemperature();
            return Initialize();
        }

        /// <summary>
        /// Read sensor Configuration and
        /// Write on Resolution, TempHiAlarm and TempLoAlarm properties.
        /// Returns false if error during reading sensor.
        /// Write 0xEE (238) to a property if
        /// error during property handle.
        /// </summary>
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

            // Now read the scratchpad
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
        /// Write sensor Configuration
        /// from tempHiAlarm, tempLoAlarm and
        /// resolution.
        /// The unchanged registers will be overwritten.
        /// </summary>
        /// <param name="writeToEEPROM">Flag indicating if configuration should be written also to EEPROM</param>
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
        /// Start to track the changes
        /// </summary>
        /// <param name="trackingInterval">Interval to track the changes to sensor values</param>
        public void BeginTrackChanges(TimeSpan trackingInterval)
        {
            if (_isTrackingChanges)
            {
                throw new InvalidOperationException("Already tracking changes");
            }

            if (trackingInterval.Milliseconds < 50)
            {
                throw new ArgumentOutOfRangeException(nameof(trackingInterval), "Minimum interval to track sensor changes is 50 milliseconds");
            }

            if (SensorValueChanged == null)
            {
                throw new NotSupportedException("Tracking not supported if SensorValueChanged event is not defined");
            }

            _changeTracker = new Thread(() =>
            {
                int divs = (int)(trackingInterval.Milliseconds / 1000);

                while (_isTrackingChanges)
                {
                    if (trackingInterval.Milliseconds > 1000)
                    {
                        divs = (int)(trackingInterval.Milliseconds / 1000);
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
                        SensorValueChanged();
                    }
                }

            });
            _isTrackingChanges = true;
            _changeTracker.Start();
        }

        /// <summary>
        /// Stop tracking changes
        /// </summary>
        public void EndTrackChanges()
        {
            _isTrackingChanges = false;
            Thread.Sleep(3000);//see BeginChangeTracker to know why 3000 is chosen...3x of lowest wait time
            if (_changeTracker.IsAlive)
            {
                //force kill
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
                //now write command and ROM at once
                byte[] cmdAndData = new byte[9]
                {
                    (byte)RomCommands.Match, //Address specific device command
                    Address[0], Address[1], Address[2], Address[3], Address[4], Address[5], Address[6],
                    Address[7] //do not convert to a for..loop
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
            //first address all devices
            _oneWireHost.WriteByte((byte)RomCommands.Skip);
            _oneWireHost.WriteByte((byte)FunctionCommands.ConvertTemperature); //convert temperature
            // According data sheet. Less resolution needs less time to complete.
            int waitConversion = TemperatureResolution switch
            {
                TemperatureResolution.VeryLow => 125,
                TemperatureResolution.Low => 250,
                TemperatureResolution.High => 500,
                TemperatureResolution.VeryHigh => 1000,
                _ => 1000
            };
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

            Read();

            float currentTemperature = _temperatureInCelsius;

            bool valuesChanged = previousTemperature != currentTemperature;

            return valuesChanged;
        }
    }
}