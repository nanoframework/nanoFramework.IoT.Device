// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm;

namespace Iot.Device.RgbDiode
{
    /// <summary>
    /// Represents an RGB (Red, Green, Blue) diode. This class encapsulates the functionality 
    /// required to control the color and intensity of an RGB diode using PWM (Pulse Width Modulation).
    /// </summary>
    public class RgbDiode
    {
        private const ushort Frequency = 5000;
        private readonly PwmChannel _rChannel;
        private readonly PwmChannel _gChannel;
        private readonly PwmChannel _bChannel;
        private readonly bool _inverse;
        private readonly double _rFactor;
        private readonly double _gFactor;
        private readonly double _bFactor;

        /// <summary>
        /// Initializes a new instance of the RgbDiode class for RGB LED control.
        /// This driver sets up PWM channels for red, green, and blue pins, and applies optional factors to adjust color intensity.
        /// </summary>
        /// <param name="rPin">Pin number for red color channel.</param>
        /// <param name="gPin">Pin number for green color channel.</param>
        /// <param name="bPin">Pin number for blue color channel.</param>
        /// <param name="inverse">Indicates if the PWM signal should be inverted.</param>
        /// <param name="rFactor">Factor to adjust intensity of red color. Default is 1.</param>
        /// <param name="gFactor">Factor to adjust intensity of green color. Default is 1.</param>
        /// <param name="bFactor">Factor to adjust intensity of blue color. Default is 1.</param>
        public RgbDiode(byte rPin, byte gPin, byte bPin, bool inverse = false, double rFactor = 1, double gFactor = 1, double bFactor = 1)
        {
            GuardFactor(rFactor, gFactor, bFactor);

            _inverse = inverse;

            _rChannel = PwmChannel.CreateFromPin(rPin, Frequency);
            _gChannel = PwmChannel.CreateFromPin(gPin, Frequency);
            _bChannel = PwmChannel.CreateFromPin(bPin, Frequency);

            _rFactor = rFactor;
            _gFactor = gFactor;
            _bFactor = bFactor;
        }

        /// <summary>
        /// Sets the color of an RGB LED using PWM channels. The intensity of each color (red, green, blue) 
        /// is adjusted by their respective factors set during the initialization of the PwmPixelDriver.
        /// </summary>
        /// <param name="red">Value for red color intensity (0-255).</param>
        /// <param name="green">Value for green color intensity (0-255).</param>
        /// <param name="blue">Value for blue color intensity (0-255).</param>
        public void SetColor(byte red, byte green, byte blue)
        {
            SetValue(_rChannel, red * _rFactor);
            SetValue(_gChannel, green * _gFactor);
            SetValue(_bChannel, blue * _bFactor);
        }

        private static void GuardFactor(params double[] factors)
        {
            foreach (var factor in factors)
            {
                if (factor < 0 || factor > 1)
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SetValue(PwmChannel channel, double value)
        {
            var dc = value / 255;
            channel.DutyCycle = _inverse ? 1 - dc : dc;
        }
    }
}