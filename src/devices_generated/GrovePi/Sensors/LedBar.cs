// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using Iot.Device.GrovePiDevice.Models;

namespace Iot.Device.GrovePiDevice.Sensors
{
    /// <summary>
    /// The led bar orientation
    /// </summary>
    public enum LedBarOrientation
    {
        /// <summary>Red to green</summary>
        RedToGreen = 0,

        /// <summary>Green to red</summary>
        GreenToRed = 1
    }

    /// <summary>
    /// LedBar class to support grove LedBar http://wiki.seeedstudio.com/Grove-LED_Bar/
    /// </summary>
    public class LedBar
    {
        /// <summary>
        /// Only Digital ports only but you can't create more than 4 bars as each bar is using 2 Pins
        /// So you have to have at least 1 Grove Port empty between 2 bars
        /// </summary>
        public static ListGrovePort SupportedPorts => new ListGrovePort()
        {
            GrovePort.DigitalPin2,
            GrovePort.DigitalPin3,
            GrovePort.DigitalPin4,
            GrovePort.DigitalPin5,
            GrovePort.DigitalPin6,
            GrovePort.DigitalPin7,
            GrovePort.DigitalPin8,
        };

        private GrovePi _grovePi;
        private byte _level;
        private LedBarOrientation _orientation;

        /// <summary>
        /// grove sensor port
        /// </summary>
        private GrovePort _port;

        /// <summary>
        /// LedBar constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        public LedBar(GrovePi grovePi, GrovePort port)
            : this(grovePi, port, LedBarOrientation.GreenToRed)
        {
        }

        /// <summary>
        /// LedBar constructor
        /// </summary>
        /// <param name="grovePi">The GrovePi class</param>
        /// <param name="port">The grove Port, need to be in the list of SupportedPorts</param>
        /// <param name="orientation">Orientation, Green to red is default</param>
        public LedBar(GrovePi grovePi, GrovePort port, LedBarOrientation orientation)
        {
            if (!SupportedPorts.Contains(port))
            {
                throw new ArgumentException(nameof(port), "Grove port not supported.");
            }

            _grovePi = grovePi;
            _port = port;
            _orientation = orientation;
            _grovePi.WriteCommand(GrovePiCommand.LedBarInitialization, port, (byte)_orientation, 0);
            _level = 0;
        }

        /// <summary>
        /// Get/Set the Led bar orientation
        /// </summary>
        public LedBarOrientation Orientation
        {
            get
            {
                return _orientation;
            }

            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
                    _grovePi.WriteCommand(GrovePiCommand.LedBarOrientation, _port, (byte)_orientation, 0);
                }
            }
        }

        /// <summary>
        /// Get/Set the level from 0 (no led on) to 10 (all leds on)
        /// </summary>
        public byte Value
        {
            get
            {
                return _level;
            }

            set
            {
                if (value > 10)
                {
                    throw new ArgumentException(nameof(Value), "Only 10 leds can be controlled, 1-10.");
                }

                _grovePi.WriteCommand(GrovePiCommand.LedBarLevel, _port, _level, 0);
            }
        }

        /// <summary>
        /// Set one led
        /// </summary>
        /// <param name="led">The led number from 0 to 10</param>
        /// <param name="status">true for on, false for off</param>
        public void SetOneLed(byte led, bool status)
        {
            if (led > 10)
            {
                throw new ArgumentException(nameof(led), "Only 10 leds can be controlled, 1-10.");
            }

            _grovePi.WriteCommand(GrovePiCommand.LedBarSetOneLed, _port, led, status ? (byte)1 : (byte)0);
        }

        /// <summary>
        /// Set all leds to either on or off by setting 1 as byte for on
        /// 0 for off. lowser bit is led 0
        /// </summary>
        /// <param name="leds">Pixel array, 1 bit for each LED</param>
        public void SetAllLeds(int leds)
        {
            leds = leds & 0b00000111111111;
            _grovePi.WriteCommand(GrovePiCommand.LedBarSet, _port, (byte)(leds & 0xFF), (byte)((leds >> 8) & 0xFF));
        }

        /// <summary>
        /// Get the status of all leds
        /// </summary>
        /// <returns>Returns all leds status in an int, lower bit is led 0, 1 is on, 0 is off</returns>
        public int GetAllLeds()
        {
            _grovePi.WriteCommand(GrovePiCommand.LetBarGet, _port, 0, 0);
            var ret = _grovePi.ReadCommand(GrovePiCommand.LetBarGet, _port);
            if (ret is object)
            {
                return ret[1] + (ret[2] >> 8);
            }

            throw new Exception("Cannot find all LEDs");
        }

        /// <summary>
        /// Toggle the state of a led
        /// </summary>
        /// <param name="led">The led from 0 to 10</param>
        public void ToggleLeds(byte led)
        {
            led = MathExtensions.Clamp(led, (byte)0, (byte)10);
            _grovePi.WriteCommand(GrovePiCommand.LedBarToggleOneLed, _port, led, 0);
        }

        /// <summary>
        /// Returns the Level formatted
        /// </summary>
        /// <returns>Returns the LED Level formatted</returns>
        public override string ToString() => $"Level {_level}";

        /// <summary>
        /// Get the name Led Bar
        /// </summary>
        public string SensorName => "Led Bar";
    }
}
