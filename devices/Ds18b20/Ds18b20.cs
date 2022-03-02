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
    /// DS18B20 - Temperature sensor 
    /// </summary>
    [Interface("DS18B20 - Temperature sensor")]
    public class Ds18b20
    {
        #region Implementation
        /// <summary>
        /// The underlying One Wire device
        /// </summary>
        private OneWireHost _oneWireHost = null;

        /// <summary>
        /// Is this sensor tracking changes
        /// </summary>
        protected bool _isTrackingChanges = false;
        /// <summary>
        /// The thread that keeps a track of sensor value change
        /// </summary>
        protected Thread _changeTracker = null;
        #endregion

        #region Events/Delegates
        /// <summary>
        /// Delegate that defines method signature that will be called
        /// when sensor value change event happens
        /// </summary>
        public delegate void OnSensorChanged();
        /// <summary>
        /// Event that is called when the sensor value changes
        /// </summary>
        public event OnSensorChanged SensorValueChanged;
        #endregion

        #region Constants
        /// <summary>
        /// Command to soft reset the HTU21D sensor
        /// </summary>
        public static readonly byte FAMILY_CODE = 0x28;
        /// <summary>
        /// Command to address specific device on network
        /// </summary>
        public static readonly byte MATCH_ROM = 0x55;
        /// <summary>
        /// Command to address all devices on the bus simultaneously
        /// </summary>
        public static readonly byte SKIP_ROM = 0xCC;
        /// <summary>
        /// Set search mode to normal
        /// </summary>
        public const bool NORMAL = false;
        /// <summary>
        /// Set search mode to search alarm
        /// </summary>
        public const bool SEARCH_ALARM = true;
        /// <summary>
        /// Command to trigger a temperature conversion
        /// </summary>
        private readonly byte CONVERT_TEMPERATURE = 0x44;
        /// <summary>
        /// Command copy scratchpad registers to EEPROM
        /// </summary>
        private readonly byte COPY_SCRATCHPAD = 0x48;
        /// <summary>
        /// Recalls the alarm trigger values and configuration
        /// from EEPROM to scratchpad registers
        /// </summary>
        private readonly byte RECALL_E2 = 0xB8;
        /// <summary>
        /// Command to write to scratchpad registers
        /// </summary>
        private readonly byte WRITE_SCRATCHPAD = 0x4E;
        /// <summary>
        /// Command to read scratchpad registers
        /// </summary>
        private readonly byte READ_SCRATCHPAD = 0xBE;
        /// <summary>
        /// Check if any DS18B20s on the bus are using parasite power
        /// Return false for parasite power, true for external power
        /// </summary>
        private readonly byte READ_POWER_SUPPLY = 0xB8;
        /// <summary>
        /// Error value of temperature
        /// </summary>
        private const float ERROR_TEMPERATURE = -999.99F;
        #endregion

        #region Properties
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
        /// Set to true if more than one device connected ie network.
        /// </summary>
        public bool Multidrop { get; set; }
        /// <summary>
        /// Total number of 18B20 devices on network.
        /// </summary>
        public int Found;

        private float TemperatureInCelcius { get; set; }
        
        /// <summary>
        /// Accessor/Mutator for Alarm Hi register in celcius
        /// Min -55, Max 125
        /// </summary>
        private sbyte tempHiAlarm;
        public sbyte TempHiAlarm
        {
            get { return tempHiAlarm; }
            set
            {
                tempHiAlarm = value;
                if (value < -55) { tempHiAlarm = -55; }
                if (value > 125) { tempHiAlarm = 125; }
            }
        }
        /// <summary>
        /// Accessor/Mutator for Alarm Lo register in celcius
        /// Min -55, Max 125
        /// </summary>
        private sbyte tempLoAlarm;
        public sbyte TempLoAlarm
        {
            get { return tempLoAlarm; }
            set
            {
                tempLoAlarm = value;
                if (value < -55) { tempLoAlarm = -55; }
                if (value > 125) { tempLoAlarm = 125; }
            }
        }
        /// <summary>
        /// Set search mode to normal
        /// or only devices in alarm
        /// </summary>
        private bool searchMode;
        public bool SetSearchMode
        {
            set { searchMode = value; }
        }
        #endregion

        /// <summary>
        /// Constructs Ds18b20 instance
        /// </summary>
        /// <param name="oneWireHost">One wire host (logical bus) to use</param>
        /// <param name="deviceAddr">The device address (if null, then this device will search for one on the bus and latch on to the first one found)</param>
        /// <param name="multidrop"> True for more than one sensor</param>
        /// <param name="temperatureResolution">Sensor resolution</param>
        public Ds18b20(OneWireHost oneWireHost, byte[] deviceAddr = null, bool multidrop = false, TemperatureResolution temperatureResolution = TemperatureResolution.VeryHigh)
        {
            _oneWireHost = oneWireHost;
            TemperatureResolution = temperatureResolution;
            Multidrop = multidrop;

            if (deviceAddr != null)
            {
                if (deviceAddr.Length != 8) throw new ArgumentException();//must be 8 bytes
                if (deviceAddr[0] != FAMILY_CODE) throw new ArgumentException();//invalid family code
                Address = deviceAddr;
            }
            TemperatureInCelcius = ERROR_TEMPERATURE;
            TempHiAlarm = 30; // Set default alarm values
            TempLoAlarm = 20;
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
        /// Reads temperature, check data sheet, page 14, 8.6.1 section
        /// </summary>
        /// <returns>
        /// Temperature
        /// </returns>
        [Telemetry("Temperature")]
        public Temperature Temperature => Temperature.FromDegreesCelsius(TemperatureInCelcius);

        #region IDisposable Support

        /// <summary>
        /// Dispose this object
        /// </summary>
        void DisposeSensor()
        {
            Address = null;
        }
        #endregion

        #region Core Methods
        /// <summary>
        /// Initialize the sensor. This step will perform a reset of the 1-wire bus.
        /// It will check for existence of a 1-wire device. If no address was provided, then the
        /// 1-wire bus will be searched and the first device that matches the family code will be latched on to.
        /// Developer should check for successful initialization by checking the value returned. 
        /// It must be bigger than 0.
        /// If in Multidrop mode will keep seaching until find last device, saving all in AddressNet array.
        /// </summary>
        public bool Initialize()
        {
            Found = 0;
            //ArrayList allDevices;
            ArrayList allDevices = new ArrayList();

            _oneWireHost.TouchReset();

            if (Address == null) //search for a device with the required family code
            {
                //found the device
                if (Multidrop)
                {
                    if (_oneWireHost.FindFirstDevice(false, searchMode))
                    {
                        do
                        {
                            if (_oneWireHost.SerialNumber[0] == FAMILY_CODE)
                            {
                                _oneWireHost.TouchReset();
                                Address = new byte[_oneWireHost.SerialNumber.Length];
                                Array.Copy(_oneWireHost.SerialNumber, Address, _oneWireHost.SerialNumber.Length);
                                Found++;
                                allDevices.Add(Address);
                                //if (Found == 6) { break; } //Temp fix during test endless loop
                            }
                        } while (_oneWireHost.FindNextDevice(false, searchMode));//keep searching until we get one
                    }

                    if (Found > 0)
                    {
                        AddressNet = new byte[Found][];
                        int i = 0;
                        foreach (byte[] device in allDevices)
                        {
                            AddressNet[i] = new byte[device.Length];
                            Array.Copy(device, AddressNet[i], device.Length);
                            i++;
                        }
                        allDevices = null;
                    }
                }
                else
                {
                    if (_oneWireHost.FindFirstDevice(true, searchMode))
                    {
                        do
                        {
                            if (_oneWireHost.SerialNumber[0] == FAMILY_CODE)
                            {
                                Address = new byte[_oneWireHost.SerialNumber.Length];
                                Array.Copy(_oneWireHost.SerialNumber, Address, _oneWireHost.SerialNumber.Length);
                                Found = 1;
                                break;
                            }
                        } while (_oneWireHost.FindNextDevice(true, searchMode));//keep searching until we get one
                    }
                }
            }
            if (Found > 0) { return true; };
            return false;
        }

        private void SelectDevice()
        {
            if (Address != null && Address.Length == 8 && Address[0] == FAMILY_CODE)
            {
                //now write command and ROM at once
                byte[] cmdAndData = new byte[9] {
                   MATCH_ROM, //Address specific device command
                   Address[0],Address[1],Address[2],Address[3],Address[4],Address[5],Address[6],Address[7] //do not convert to a for..loop
               };

                _oneWireHost.TouchReset();
                foreach (var b in cmdAndData) _oneWireHost.WriteByte(b);
            }
        }

        private void Convert_T()
        {
            _oneWireHost.TouchReset();
            //first address all devices
            _oneWireHost.WriteByte(SKIP_ROM);//Skip ROM command
            _oneWireHost.WriteByte(CONVERT_TEMPERATURE);//convert temperature
                                                        // According datasheet. Less resolution needs less time to complete.
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

        /// <summary>
        /// Prepare sensor to read the data
        /// </summary>
        public void PrepareToRead()
        {
            if ((Address != null || Found != 0) && Address.Length == 8 && Address[0] == FAMILY_CODE)
            {
                Convert_T();
            }
        }

        /// <summary>
        /// Read sensor data
        /// </summary>
        /// <returns>true on success, else false</returns>
        public bool Read()
        {
            SelectDevice();

            //now read the scratchpad
            var verify = _oneWireHost.WriteByte(READ_SCRATCHPAD);

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

                TemperatureInCelcius = ((float)temp) / 16;
                return true;
            }
            else
            {
                TemperatureInCelcius = ERROR_TEMPERATURE;
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
            TemperatureInCelcius = ERROR_TEMPERATURE;
        }
        #endregion

        /// <summary>
        /// Search for alarm condition.
        /// Save in AddressNet the list of devices
        /// under alarm condition.
        /// </summary>
        /// <returns>bool</returns>
        public bool SearchForAlarmCondition()
        {
            Address = null;

            Convert_T();
            if (Initialize()) { return true; }
            return false;
        }

        #region Configuration Methods
        /// <summary>
        /// Read sensor Configuration and
        /// Write on Resolution, TempHiAlarm and TempLoAlarm properties.
        /// Returns false if error during reading sensor.
        /// Write 0xEE (238) to a property if
        /// error during property handle.
        /// </summary>
        public bool ConfigurationRead(bool recall = false)
        {
            var verify = 0;

            // Restore Register from EEPROM
            if (recall == true)
            {
                SelectDevice();
                verify = _oneWireHost.WriteByte(RECALL_E2);
                while (_oneWireHost.ReadByte() == 0) { Thread.Sleep(10); }
            }

            // Now read the scratchpad
            SelectDevice();
            verify = _oneWireHost.WriteByte(READ_SCRATCHPAD);

            // Discard temperature bytes
            _oneWireHost.ReadByte();
            _oneWireHost.ReadByte();

            TempHiAlarm = (sbyte)_oneWireHost.ReadByte();
            TempLoAlarm = (sbyte)_oneWireHost.ReadByte();
            int configReg = _oneWireHost.ReadByte();

            if (_oneWireHost.TouchReset())
            {
                TemperatureResolution = (TemperatureResolution) (configReg >> 5);
            }
            else
            {
                TemperatureResolution = TemperatureResolution.VeryHigh;
                return false;
            };

            return true;
        }

        /// <summary>
        /// Write sensor Configuration
        /// from tempHiAlarm, tempLoAlarm and
        /// resolution.
        /// The unchanged registers will be overwritten.
        /// </summary>
        public bool ConfigurationWrite(bool save = false)
        {

            SelectDevice();

            //now write the scratchpad
            var verify = _oneWireHost.WriteByte(WRITE_SCRATCHPAD);

            _oneWireHost.WriteByte((byte)tempHiAlarm);
            _oneWireHost.WriteByte((byte)tempLoAlarm);
            _oneWireHost.WriteByte((byte)((byte)TemperatureResolution << 5));

            // Save confuguration on device's EEPROM
            if (save)
            {
                SelectDevice();
                verify = _oneWireHost.WriteByte(COPY_SCRATCHPAD);
                Thread.Sleep(10);
            };
            return true;
        }

        /// <summary>
        /// Returns information if it's powered as parasite of the board
        /// </summary>
        /// <returns>bool</returns>
        public bool IsParasitePowered()
        {
            SelectDevice();

            // Now read power supply external | parasite
            var verify = _oneWireHost.WriteByte(READ_POWER_SUPPLY);

            return _oneWireHost.ReadByte() == 0x00;
        }
        #endregion

        #region Change tracking
        /// <summary>
        /// This sensor suports change tracking
        /// </summary>
        /// <returns>bool</returns>
        public bool CanTrackChanges()
        {
            return true;
        }

        /// <summary>
        /// Let the world know whether the sensor value has changed or not
        /// </summary>
        /// <returns>bool</returns>
        bool HasSensorValueChanged()
        {
            float previousTemperature = TemperatureInCelcius;

            PrepareToRead();
            Read();

            float currentTemperature = TemperatureInCelcius;

            bool valuesChanged = (previousTemperature != currentTemperature);

            return valuesChanged;
        }

        /// <summary>
        /// Start to track the changes
        /// </summary>
        /// <param name="ms">Interval in milliseconds to track the changes to sensor values</param>
        public virtual void BeginTrackChanges(int ms)
        {
            if (_isTrackingChanges) throw new InvalidOperationException("Already tracking changes");
            if (ms < 50) throw new ArgumentOutOfRangeException("ms", "Minimum interval to track sensor changes is 50 milliseconds");
            if (SensorValueChanged == null) throw new NotSupportedException("Tracking not supported if SensorValueChanged event is not defined");

            _changeTracker = new Thread(() =>
            {
                int divs = (int)(ms / 1000);

                while (_isTrackingChanges)
                {
                    if (ms > 1000)
                    {
                        divs = (int)(ms / 1000);
                        while (_isTrackingChanges && divs > 0)
                        {
                            Thread.Sleep(1000);
                            divs--;
                        }
                    }
                    else
                        Thread.Sleep(ms);
                    //now check for change
                    if (HasSensorValueChanged() && SensorValueChanged != null)
                    {
                        try { SensorValueChanged(); } catch {; ; /*do nothing..upto event handler to decide what to do*/ }
                    }
                }

            });
            _isTrackingChanges = true;
            _changeTracker.Start();
        }

        /// <summary>
        /// Stop tracking changes
        /// </summary>
        public virtual void EndTrackChanges()
        {
            _isTrackingChanges = false;
            Thread.Sleep(3000);//see BeginChangeTracker to know why 3000 is chosen...3x of lowest wait time
            if (_changeTracker.IsAlive)
            {
                //force kill
                try { _changeTracker.Abort(); } finally { _changeTracker = null; }
            }
        }

        #endregion
    }

    /// <summary>
    /// Temperature sampling resolution, check data sheet, page 11, 8.5.1.3 section
    /// </summary>
    public enum TemperatureResolution : byte
    {
        /// <summary>
        /// 9 bit
        /// </summary>
        VeryLow = 0x00,

        /// <summary>
        /// 10 bit
        /// </summary>
        Low = 0x01,

        /// <summary>
        ///11 bit
        /// </summary>
        High = 0x02,

        /// <summary>
        /// 12 bit
        /// </summary>
        VeryHigh = 0x03,
    }
}
