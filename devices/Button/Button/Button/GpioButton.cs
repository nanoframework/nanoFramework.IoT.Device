using System;
using System.Device.Gpio;
using System.Diagnostics;

namespace Iot.Device.Button
{
    /// <summary>
    /// 
    /// </summary>
    public class GpioButton : ButtonBase
    {
        private const int DEFAULT_BUTTON_PIN = 37;

        private GpioController _gpioController;
        private int _buttonPin;
        private bool _pullUp;

        private bool _disposed = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonPin"></param>
        /// <param name="pullUp"></param>
        /// <param name="doublePressMs"></param>
        /// <param name="longPressMs"></param>
        public GpioButton(int buttonPin = DEFAULT_BUTTON_PIN, bool pullUp = true, int doublePressMs = DEFAULT_DOUBLE_PRESS_MS, int longPressMs = DEFAULT_LONG_PRESS_MS)
            : base(doublePressMs, longPressMs)
        {
            _gpioController = new GpioController();
            _buttonPin = buttonPin;
            _pullUp = pullUp;

            _gpioController.OpenPin(_buttonPin, PinMode.Input);
            _gpioController.RegisterCallbackForPinValueChangedEvent(_buttonPin, PinEventTypes.Falling | PinEventTypes.Rising, PinStateChanged);
        }

        /// <summary>
        /// Handles changes in GPIO pin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pinValueChangedEventArgs"></param>
        private void PinStateChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            switch (GetPinEvent(pinValueChangedEventArgs.ChangeType))
            {
                case PinEventTypes.Falling:
                    HandleButtonPressed();
                    break;
                case PinEventTypes.Rising:
                    HandleButtonReleased();
                    break;
            }
        }

        /// <summary>
        /// Handle pull up or pull down setting. In case of pull down
        /// we'll flip the event type for consistant handling in <see cref="PinStateChanged(object, PinValueChangedEventArgs)"/>.
        /// </summary>
        /// <param name="changeType">Original type</param>
        /// <returns>Proper type for handling.</returns>
        private PinEventTypes GetPinEvent(PinEventTypes changeType)
        {
            if (_pullUp)
            {
                return changeType;
            }

            switch (changeType)
            {
                case PinEventTypes.Falling:
                    return PinEventTypes.Rising;
                case PinEventTypes.Rising:
                    return PinEventTypes.Falling;
                default:
                    return PinEventTypes.None;
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
        /// Iternal cleanup.
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
