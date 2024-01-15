// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm;
using System.Drawing;
using System.Threading;

namespace Iot.Device.RgbDiode
{
    /// <summary>
    /// Represents an RGB (Red, Green, Blue) diode. This class encapsulates the functionality 
    /// required to control the color and intensity of an RGB diode using PWM (Pulse Width Modulation).
    /// </summary>
    public class RgbDiode
    {
        private const int DefaultSteps = 100;
        private const int DefaultDelay = 10;
        private const ushort Frequency = 5000;
        private readonly PwmChannel _rChannel;
        private readonly PwmChannel _gChannel;
        private readonly PwmChannel _bChannel;
        private readonly bool _inverse;
        private readonly double _rFactor;
        private readonly double _gFactor;
        private readonly double _bFactor;

        /// <summary>
        /// Gets the current color.
        /// </summary>
        public Color CurrentColor { get; private set; }

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
            CurrentColor = new Color();
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
            var color = Color.FromArgb(red, green, blue);
            SetColor(color);
        }

        /// <summary>
        /// Sets the color of an RGB LED using PWM channels.
        /// is adjusted by their respective factors set during the initialization of the PwmPixelDriver.
        /// </summary>
        /// <param name="color">The color to set.</param>
        public void SetColor(Color color)
        {
            SetValue(_rChannel, color.R * _rFactor);
            SetValue(_gChannel, color.G * _gFactor);
            SetValue(_bChannel, color.B * _bFactor);
            CurrentColor = color;
        }
        
        /// <summary>
        /// Transitions the pixel to new color in a separate thread.
        /// </summary>
        /// <param name="color">The new color.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="steps">Number of transition steps.</param>
        /// <param name="delay">Delay between each transition step.</param>
        /// <returns>The new Thread or null if color is the same as current color.</returns>
        public Thread TransitionAsync(Color color, CancellationToken cancellationToken = default, int steps = DefaultSteps, int delay = DefaultDelay)
        {
            if (Equals(color, CurrentColor))
            {
                return null;
            }

            var dr = (byte)((color.R - CurrentColor.R) / (double)steps);
            var dg = (byte)((color.G - CurrentColor.G) / (double)steps);
            var db = (byte)((color.B - CurrentColor.B) / (double)steps);

            var thread = new Thread(() => TransitionInternal(cancellationToken, steps, delay, dr, dg, db));

            thread.Start();

            return thread;
        }

        /// <summary>
        /// Transitions the pixel to new color.
        /// </summary>
        /// <param name="color">The new color.</param>
        /// <param name="steps">Number of transition steps.</param>
        /// <param name="delay">Delay between each transition step.</param>
        public void Transition(Color color, int steps = DefaultSteps, int delay = DefaultDelay)
        {
            if (Equals(color, CurrentColor))
            {
                return;
            } 

            var dr = (byte)((color.R - CurrentColor.R) / (double)steps);
            var dg = (byte)((color.G - CurrentColor.G) / (double)steps);
            var db = (byte)((color.B - CurrentColor.B) / (double)steps);

            TransitionInternal(new CancellationToken(), steps, delay, dr, dg, db);
        }

        /// <summary>
        /// Fades out to black and transitions the pixel to new color in a separate thread.
        /// </summary>
        /// <param name="color">The new color.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="steps">Number of transition steps.</param>
        /// <param name="delay">Delay between each transition step.</param>
        /// <returns>The new Thread or null if color is the same as current color.</returns>
        public Thread FadeTransitionAsync(Color color, CancellationToken cancellationToken, int steps = DefaultSteps, int delay = DefaultDelay)
        {
            var thread = new Thread(() =>
                {
                    TransitionAsync(Color.FromArgb(0, 0, 0), cancellationToken, steps, delay)?.Join();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    TransitionAsync(color, cancellationToken, steps, delay)?.Join();
                });

            thread.Start();

            return thread;
        }

        /// <summary>
        /// Fades out and then back to current color in a separate thread.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="steps">Number of transition steps.</param>
        /// <param name="delay">Delay between each transition step.</param>
        /// <returns>The new Thread or null if color is the same as current color.</returns>
        public Thread BlinkSmoothAsync(CancellationToken cancellationToken, int steps = DefaultSteps, int delay = DefaultDelay)
        {
            return FadeTransitionAsync(CurrentColor, cancellationToken, steps, delay);
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
        
        private void TransitionInternal(CancellationToken cancellationToken, int steps, int delay, byte dr, byte dg, byte db)
        {
            var cr = CurrentColor.R;
            var cg = CurrentColor.G;
            var cb = CurrentColor.B;

            for (var i = 0; i < steps && !cancellationToken.IsCancellationRequested; i++)
            {
                cr += dr;
                cg += dg;
                cb += db;

                SetColor(Color.FromArgb(cr, cg, cb));
                cancellationToken.WaitHandle.WaitOne(delay, false);
            }
        }

        private void SetValue(PwmChannel channel, double value)
        {
            var dc = value / 255;
            channel.DutyCycle = _inverse ? 1 - dc : dc;
        }
    }
}