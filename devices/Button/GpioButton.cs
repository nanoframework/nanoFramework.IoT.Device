// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Button
{
    /// <summary>
    /// GPIO implementation of Button.
    /// Inherits from ButtonBase.
    /// </summary>
    public class GpioButton : ButtonBase
    {
        private GpioController _gpioController;
        private int _buttonPin;
        private bool _pullUp;

        private bool _disposed = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="buttonPin">GPIO pin of the button.</param>
        /// <param name="pullUp">If the system is pullup (false = pulldown).</param>
        /// <param name="doublePressMs">Max ticks between button presses to count as doublepress.</param>
        /// <param name="holdingMs">Min ms a button is pressed to count as holding.</param>
        public GpioButton(int buttonPin, bool pullUp = true, int doublePressMs = DefaultDoublePressTicks, int holdingMs = DefaultHoldingMilliseconds)
            : base(doublePressMs, holdingMs)
        {
            _gpioController = new GpioController();
            _buttonPin = buttonPin;
            _pullUp = pullUp;

            _gpioController.OpenPin(_buttonPin, PinMode.Input);
            _gpioController.RegisterCallbackForPinValueChangedEvent(_buttonPin, PinEventTypes.Falling | PinEventTypes.Rising, PinStateChanged);
        }

        /// <summary>
        /// Handles changes in GPIO pin, based on whether the system is pullup or pulldown.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pinValueChangedEventArgs"></param>
        private void PinStateChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            switch (pinValueChangedEventArgs.ChangeType)
            {
                case PinEventTypes.Falling:
                    if (_pullUp)
                    {
                        HandleButtonPressed();
                    }
                    else
                    {
                        HandleButtonReleased();
                    }
                    break;
                case PinEventTypes.Rising:
                    if (_pullUp)
                    {
                        HandleButtonReleased();
                    }
                    else
                    {
                        HandleButtonPressed();
                    }
                    break;
            }
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Internal cleanup.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _gpioController?.Dispose();
                _gpioController = null!;
                base.Dispose(disposing);
            }
        }
    }
}
