using System.Device.Gpio;

namespace Iot.Device.Button
{
    /// <summary>
    /// GPIO implementation of Button.
    /// Inherits from ButtonBase.
    /// </summary>
    public class GpioButton : ButtonBase
    {
        private const int DEFAULT_BUTTON_PIN = 37;

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
        public GpioButton(int buttonPin = DEFAULT_BUTTON_PIN, bool pullUp = true, int doublePressMs = DefaultDoublePressTicks, int holdingMs = DefaultHoldingMilliseconds)
            : base(doublePressMs, holdingMs)
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
        /// Handle pull up or pull down setting. In case of pull down,
        /// we flip the event type for consistent handling in <see cref="PinStateChanged(object, PinValueChangedEventArgs)"/>.
        /// </summary>
        /// <param name="changeType">Original type.</param>
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
