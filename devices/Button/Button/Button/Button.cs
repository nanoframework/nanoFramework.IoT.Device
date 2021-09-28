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

        // TO DO: Long to hold?
        // Setting up variables and stuff
        private GpioController _gpioController;
        private bool _disposed = false;
        private int _ButtonPin;
        private int _DoublePressMs;
        private int _LongPressMs;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private DateTime _lastClick = DateTime.MinValue;

        public delegate void ButtonPressedDelegate(object sender, EventArgs e);
        public delegate void ButtonHoldingDelegate(object sender, ButtonHoldingEventArgs e);

        public event ButtonPressedDelegate ButtonUp;
        public event ButtonPressedDelegate ButtonDown;
        public event ButtonPressedDelegate Click;
        public event ButtonPressedDelegate DoubleClick;
        public event ButtonHoldingDelegate Holding;

        private Timer _holdingTimer;

        // TO DO: Add rising types to revert - gpio controller?
        // Changetype 1 = Rising = released
        // Changetype 2 = Falling = pressed

        public bool IsHoldingEnabled { get; set; } = false;
        public bool IsDoubleClickEnabled { get; set; } = false;
        public bool IsPressed { get; set; } = false;


        public Button(int buttonPin = DEFAULT_BUTTON_PIN, int doublePressMs = 500, int longPressMs = 1000)
        {
            _gpioController = new GpioController();
            _ButtonPin = buttonPin;
            _DoublePressMs = doublePressMs;
            _LongPressMs = longPressMs;
            _lastClick = DateTime.UtcNow;

            // Add function that sets pin as buttons 
            _gpioController.OpenPin(_ButtonPin, PinMode.Input);

            _gpioController.RegisterCallbackForPinValueChangedEvent(_ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChanged);

        }

        internal void ButtonStateChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            switch (pinValueChangedEventArgs.ChangeType)
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
                            if (DateTime.UtcNow.Subtract(_lastClick).TotalMilliseconds <= _DoublePressMs)
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
                        _holdingTimer = new Timer(StartHoldingHandler, null, _LongPressMs, Timeout.Infinite);
                    }
                    break;
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
