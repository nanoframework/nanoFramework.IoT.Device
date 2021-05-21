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
    /// The color mode offered by the NXT Color sensor
    /// </summary>
    public enum ColorSensorMode
    {
        /// <summary>Color mode</summary>
        Color = SensorType.NXTColorFull,

        /// <summary>Reflection mode</summary>
        Reflection = SensorType.NXTColorRed,

        /// <summary>Green mode</summary>
        Green = SensorType.NXTColorGreen,

        /// <summary>Blue mode</summary>
        Blue = SensorType.NXTColorBlue,

        /// <summary>Ambient mode</summary>
        Ambient = SensorType.NXTColorOff
    }

    /// <summary>
    /// Colors that can be read from the color sensor
    /// </summary>
    public enum Color
    {
        /// <summary>No color was read</summary>
        None = 0,

        /// <summary>Black</summary>
        Black = 1,

        /// <summary>Blue</summary>
        Blue = 2,

        /// <summary>Green</summary>
        Green = 3,

        /// <summary>Yellow</summary>
        Yellow = 4,

        /// <summary>Red</summary>
        Red = 5,

        /// <summary>White</summary>
        White = 6,

        /// <summary>Brown</summary>
        Brown = 7
    }

    /// <summary>
    /// Class that holds RGB colors
    /// </summary>
    public class RGBColor
    {
        /// <summary>
        /// Initializes a new instance of the RGBColor class.
        /// </summary>
        /// <param name='red'>Red value</param>
        /// <param name='green'>Green value</param>
        /// <param name='blue'>Blue value</param>
        public RGBColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        /// <summary>
        /// Gets the red value
        /// </summary>
        /// <value>The red value</value>
        public byte Red { get; }

        /// <summary>
        /// Gets the green value
        /// </summary>
        /// <value>The green value</value>
        public byte Green { get; }

        /// <summary>
        /// Gets the blue value
        /// </summary>
        /// <value>The blue value</value>
        public byte Blue { get; }
    }

    /// <summary>
    /// Create a NXT Color sensor
    /// </summary>
    public class NXTColorSensor : INotifyPropertyChanged, ISensor
    {
        private const int RedIndex = 0;
        private const int GreenIndex = 1;
        private const int BlueIndex = 2;
        private const int BackgroundIndex = 3;

        private Brick _brick;
        private ColorSensorMode _colorMode;
        private Timer _timer;
        private short[] _rawValues = new short[4];
        private int _periodRefresh;
        private int _value;
        private string? _valueAsString;

        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        public NXTColorSensor(Brick brick, SensorPort port)
            : this(brick, port, ColorSensorMode.Color, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        public NXTColorSensor(Brick brick, SensorPort port, ColorSensorMode mode)
            : this(brick, port, mode, 1000)
        {
        }

        /// <summary>
        /// Initialize a NXT Color Sensor
        /// </summary>
        /// <param name="brick">Interface to main Brick component</param>
        /// <param name="port">Sensor port</param>
        /// <param name="mode">Color mode</param>
        /// <param name="timeout">Period in millisecond to check sensor value changes</param>
        public NXTColorSensor(Brick brick, SensorPort port, ColorSensorMode mode, int timeout)
        {
            _brick = brick;
            Port = port;
            _colorMode = mode;
            brick.SetSensorType((byte)Port, (SensorType)mode);
            _periodRefresh = timeout;
            UpdateSensor(this);
            _timer = new Timer(UpdateSensor, this, TimeSpan.FromMilliseconds(timeout), TimeSpan.FromMilliseconds(timeout));
        }

        private void StopTimerInternal()
        {
            _timer?.Dispose();
            _timer = null!;
        }

        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
            get => _periodRefresh;
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
            get => ReadRaw();
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
        /// Color mode
        /// </summary>
        public ColorSensorMode ColorMode
        {
            get => _colorMode;
            set
            {
                if (value != _colorMode)
                {
                    _colorMode = value;
                    _brick.SetSensorType((byte)Port, (SensorType)_colorMode);
                }
            }
        }

        /// <summary>
        /// Sensor port
        /// </summary>
        public SensorPort Port { get; }

        /// <summary>
        /// Gets sensor name
        /// </summary>
        /// <returns>Sensor name</returns>
        public string GetSensorName() => "NXT Color Sensor";

        private void GetRawValues()
        {
            try
            {
                var ret = _brick.GetSensor((byte)Port);
                for (int i = 0; i < _rawValues.Length; i++)
                {
                    _rawValues[i] = ret[i];
                }
            }
            catch (Exception ex) when (ex is IOException)
            {
            }
        }

        /// <summary>
        /// Reads raw value from the sensor
        /// </summary>
        /// <returns>Value read from the sensor</returns>
        public int ReadRaw() => _colorMode switch
        {
                ColorSensorMode.Color => (int)ReadColor(),
                ColorSensorMode.Reflection or ColorSensorMode.Green or ColorSensorMode.Blue
                    => CalculateRawAverage(),
                ColorSensorMode.Ambient => CalculateRawAverage(),
                _ => 0,
        };

        /// <summary>
        /// Read the intensity of the reflected or ambient light in percent. In color mode the color index is returned
        /// </summary>
        public int Read() => _colorMode switch
        {
            ColorSensorMode.Ambient => CalculateRawAverageAsPct(),
            ColorSensorMode.Color => (int)ReadColor(),
            ColorSensorMode.Reflection => CalculateRawAverageAsPct(),
            _ => CalculateRawAverageAsPct(),
        };

        private int CalculateRawAverage()
        {
            if (_colorMode == ColorSensorMode.Color)
            {
                GetRawValues();
                return (_rawValues[RedIndex] + _rawValues[BlueIndex] + _rawValues[GreenIndex]) / 3;
            }
            else
            {
                try
                {
                    return _brick.GetSensor((byte)Port)[0];
                }
                catch (Exception ex) when (ex is IOException)
                {
                    return -1;
                }
            }
        }

        // Need to find out what is the ADC resolution
        // 1023 is probably the correct one
        private int CalculateRawAverageAsPct() => (CalculateRawAverage() * 100) / 1023;

        /// <summary>
        /// Reads value from sensor represented as string
        /// </summary>
        /// <returns>Sensor value as string</returns>
        public string ReadAsString() => _colorMode switch
        {
            ColorSensorMode.Color => ReadColor().ToString(),
            ColorSensorMode.Reflection or ColorSensorMode.Green or ColorSensorMode.Blue
                => Read().ToString(),
            ColorSensorMode.Ambient => Read().ToString(),
            _ => string.Empty,
        };

        /// <summary>
        /// Reads the color.
        /// </summary>
        /// <returns>The color.</returns>
        public Color ReadColor()
        {
            Color color = Color.None;
            if (_colorMode == ColorSensorMode.Color)
            {
                try
                {
                    color = (Color)_brick.GetSensor((byte)Port)[0];
                }
                catch (Exception ex) when (ex is IOException)
                {
                    color = Color.None;
                }
            }

            return color;
        }

        /// <summary>
        /// Reads the color of the RGB.
        /// </summary>
        /// <returns>The RGB color.</returns>
        public RGBColor ReadRGBColor()
        {
            GetRawValues();
            return new RGBColor((byte)_rawValues[RedIndex], (byte)_rawValues[GreenIndex], (byte)_rawValues[BlueIndex]);
        }

        /// <summary>
        /// Moves to next mode
        /// </summary>
        public void SelectNextMode() => _colorMode = ColorMode.Next();

        /// <summary>
        /// Moves to previous mode
        /// </summary>
        public void SelectPreviousMode() => _colorMode = ColorMode.Previous();

        /// <summary>
        /// Number of modes supported
        /// </summary>
        /// <returns>Number of modes</returns>
        public int NumberOfModes() => Enum.GetNames(typeof(ColorSensorMode)).Length;

        /// <summary>
        /// Selected mode
        /// </summary>
        /// <returns>String representing selected mode</returns>
        public string SelectedMode() => ColorMode.ToString();
    }
}
