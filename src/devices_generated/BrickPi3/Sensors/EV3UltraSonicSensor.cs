// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Iot.Device.BrickPi3.Extensions;
using Iot.Device.BrickPi3.Models;

namespace Iot.Device.BrickPi3.Sensors
{
    /// <summary>
    /// Create a EV3 Ultrasound sensor
    /// </summary>
    public class EV3UltraSonicSensor : INotifyPropertyChanged, ISensor
    {
        private Brick _brick;
        private UltraSonicMode _mode;
        private Timer _timer;
        private int _value;
        private string? _valueAsString;
        private int _periodRefresh;

        /// <summary>
        /// Initialize an EV3 Ulrasonic sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public EV3UltraSonicSensor(Brick brick, SensorPort port)
            : this(brick, port, UltraSonicMode.Centimeter, 1000)
        {
        }

        /// <summary>
        /// Initialize an EV3 Ultrasonic sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor mode</param>
        /// <param name="usmode">Ultrasonic mode</param>
        public EV3UltraSonicSensor(Brick brick, SensorPort port, UltraSonicMode usmode)
            : this(brick, port, usmode, 1000)
        {
        }

        /// <summary>
        /// Initialize an EV3 Ultrasonic Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="usmode">Ultrasonic mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public EV3UltraSonicSensor(Brick brick, SensorPort port, UltraSonicMode usmode, int timeout)
        {
            _brick = brick;
            Port = port;
            if (UltraSonicMode.Listen == _mode)
            {
                _mode = UltraSonicMode.Centimeter;
            }

            _mode = usmode;
            brick.SetSensorType((byte)Port, (SensorType)usmode);
            _periodRefresh = timeout;
            _timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        private void StopTimerInternal()
        {
            _timer?.Dispose();
            _timer = null!;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// To notify a property has changed. The minimum time can be set up
        /// with timeout property
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Period to refresh the notification of property changed in milliseconds
        /// </summary>
        public int PeriodRefresh
        {
            get
            {
                return _periodRefresh;
            }

            set
            {
                _periodRefresh = value;
                _timer.Change(TimeSpan.FromMilliseconds(_periodRefresh), TimeSpan.FromMilliseconds(_periodRefresh));
            }
        }

        /// <summary>
        /// Return the raw value of the sensor
        /// </summary>
        public int Value
        {
            get
            {
                return ReadRaw();
            }

            internal set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        /// <summary>
        /// Return the raw value  as a string of the sensor
        /// </summary>
        public string ValueAsString => ReadAsString();

        /// <summary>
        /// Update the sensor and this will raised an event on the interface
        /// </summary>
        public void UpdateSensor(object? state)
        {
            Value = ReadRaw();
            string sensorState = ReadAsString();
            string? previousValue = _valueAsString;
            _valueAsString = sensorState;
            if (sensorState != previousValue)
            {
                OnPropertyChanged(nameof(ValueAsString));
            }
        }

        /// <summary>
        /// Gets or sets the Gyro mode.
        /// </summary>
        /// <value>The mode.</value>
        public UltraSonicMode Mode
        {
            get
            {
                return _mode;
            }

            set
            {
                if (_mode != value)
                {
                    _mode = value;
                    _brick.SetSensorType((byte)Port, GetEV3Type(_mode));
                }
            }
        }

        private SensorType GetEV3Type(UltraSonicMode usmode)
        {
            return (SensorType)usmode;
        }

        /// <summary>
        /// Sensor port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Reads the sensor value as a string.
        /// </summary>
        /// <returns>The value as a string</returns>
        public string ReadAsString() => _mode switch
        {
            UltraSonicMode.Centimeter => $"{Read().ToString()} cm",
            UltraSonicMode.Inch => $"{Read().ToString()} inch",
            UltraSonicMode.Listen => Read().ToString(),
            _ => string.Empty,
        };

        /// <summary>
        /// Read the sensor value. Result depends on the mode
        /// </summary>
        /// <returns>Value as a int</returns>
        public int Read()
        {
            var ret = ReadRaw();
            if (ret == int.MaxValue)
            {
                return ret;
            }

            return Mode == UltraSonicMode.Listen ? (ret != 0 ? 1 : 0) : ret;
        }

        /// <summary>
        /// Read the sensor value
        /// </summary>
        /// <returns>Value as a int</returns>
        public int ReadRaw()
        {
            try
            {
                byte[] ret = _brick.GetSensor((byte)Port);
                return _mode switch
                {
                    UltraSonicMode.Centimeter or UltraSonicMode.Inch => (ret[0] + (ret[1] >> 8)),
                    UltraSonicMode.Listen => ret[0],
                    _ => int.MaxValue,
                };
            }
            catch (Exception ex) when (ex is IOException)
            {
                return int.MaxValue;
            }
        }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName() => "EV3 Ultrasonic";

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode() => Mode = Mode.Next();

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode() => Mode = Mode.Previous();

        /// <summary>
        /// Number of modes supported
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes() => Enum.GetNames(typeof(UltraSonicMode)).Length;

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode() => Mode.ToString();
    }
}
