using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.Button
{
    /// <summary>
    /// Class implementing buttons.
    /// </summary>
    public class Button : IDisposable
    {
        private const int DEFAULT_BUTTON_PIN = 37;
        // TO DO: Add other defaults

        // Setting up variables and stuff
        private GpioController _gpioController;
        private bool _disposed = false;
        private int _buttonPin;
        private int _doublePressMs;
        private int _longPressMs;
        private bool _pullUp;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private DateTime _lastClick = DateTime.MinValue;
        private Timer _holdingTimer;

        public delegate void ButtonPressedDelegate(object sender, EventArgs e);
        public delegate void ButtonHoldingDelegate(object sender, ButtonHoldingEventArgs e);

        public event ButtonPressedDelegate ButtonUp;
        public event ButtonPressedDelegate ButtonDown;
        public event ButtonPressedDelegate Click;
        public event ButtonPressedDelegate DoubleClick;
        public event ButtonHoldingDelegate Holding;

        public bool IsHoldingEnabled { get; set; } = false;
        public bool IsDoubleClickEnabled { get; set; } = false;
        public bool IsPressed { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttonPin"></param>
        /// <param name="pullUp"></param>
        /// <param name="doublePressMs"></param>
        /// <param name="longPressMs"></param>
        public Button(int buttonPin = DEFAULT_BUTTON_PIN, bool pullUp = true, int doublePressMs = 500, int longPressMs = 1000)
        {
            _gpioController = new GpioController();
            _buttonPin = buttonPin;
            _doublePressMs = doublePressMs;
            _longPressMs = longPressMs;
            _lastClick = DateTime.UtcNow;
            _pullUp = pullUp;

            // Add function that sets pin as buttons 
            _gpioController.OpenPin(_buttonPin, PinMode.Input);

            _gpioController.RegisterCallbackForPinValueChangedEvent(_buttonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChanged);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="pinValueChangedEventArgs"></param>
        internal void ButtonStateChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            switch (GetPinEvent(pinValueChangedEventArgs.ChangeType))
            {
                case PinEventTypes.Rising:
                    _holdingTimer?.Dispose();
                    _holdingTimer = null;
                    IsPressed = false;
                    if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
                    {
                        _holdingState = ButtonHoldingState.Completed;
                        Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
                    }
                    ButtonUp?.Invoke(this, new EventArgs());
                    Click?.Invoke(this, new EventArgs());

                    if (IsDoubleClickEnabled)
                    {
                        if (_lastClick == DateTime.MinValue)
                        {
                            _lastClick = DateTime.UtcNow;
                        }
                        else
                        {
                            if (DateTime.UtcNow.Subtract(_lastClick).TotalMilliseconds <= _doublePressMs)
                            {
                                DoubleClick.Invoke(this, new EventArgs());
                            }
                            _lastClick = DateTime.MinValue;
                        }
                    }

                    break;

                case PinEventTypes.Falling:
                    IsPressed = true;
                    ButtonDown?.Invoke(this, new EventArgs());
                    if (IsHoldingEnabled)
                    {
                        _holdingTimer = new Timer(StartHoldingHandler, null, _longPressMs, Timeout.Infinite);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handle pull up or pull down setting. In case of pull down
        /// we'll flip the event type for consistant handling in <see cref="ButtonStateChanged(object, PinValueChangedEventArgs)"/>.
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

        private void StartHoldingHandler(object state)
        {
            _holdingTimer.Dispose();
            _holdingTimer = null;
            _holdingState = ButtonHoldingState.Started;

            Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Started});
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _holdingTimer?.Dispose();
                _holdingTimer = null;

                _gpioController?.Dispose();
                _gpioController = null!;
            }
        }
    }
}
