// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;

namespace Iot.Device.ExplorerHat
{
    /// <summary>
    /// Represents a led light
    /// </summary>
    public class Led : IDisposable
    {
        private GpioController _controller;

        /// <summary>
        /// GPIO pin to which led is attached
        /// </summary>
        public int Pin { get; private set; }

        /// <summary>
        /// Gets if led is switched on or not
        /// </summary>
        /// <value></value>
        public bool IsOn { get; private set; }

        /// <summary>
        /// Initializes a <see cref="Led"/> instance
        /// </summary>
        /// <param name="pin">Underlying rpi GPIO pin number</param>
        /// <param name="controller"><see cref="GpioController"/> used by <see cref="Led"/> to manage GPIO resources</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller</param>
        internal Led(int pin, GpioController? controller = null, bool shouldDispose = true)
        {
            _controller = controller ?? new();
            _shouldDispose = shouldDispose || controller is null;
            Pin = pin;
            IsOn = false;

            _controller.OpenPin(Pin, PinMode.Output);
        }

        /// <summary>
        /// Switch on this led light
        /// </summary>
        public void On()
        {
            if (!IsOn && _controller is object)
            {
                _controller.Write(Pin, PinValue.High);
                IsOn = true;
            }
        }

        /// <summary>
        /// Switch off this led light
        /// </summary>
        public void Off()
        {
            if (IsOn && _controller is object)
            {
                _controller.Write(Pin, PinValue.Low);
                IsOn = false;
            }
        }

        private bool _shouldDispose;

        /// <summary>
        /// Disposes the <see cref="Led"/> instance
        /// </summary>
        public void Dispose()
        {
            Off();
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null!;
            }
        }
    }
}
