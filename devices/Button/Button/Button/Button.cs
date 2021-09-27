using System;
using System.Device.Gpio;

namespace Iot.Device.Button
{
    /// <summary>
    /// Class implementing buttons.
    /// </summary>
    public class Button : IDisposable
    {
        // TO DO: Click --> press
        // Setting up variables and stuff
        private GpioController _gpioController;
        private bool _shouldDispose = true;
        private int _ButtonPin;
        private int _DoublePressMs;
        private int _LongPressMs;

        private int _State = 0;
        private EventsRegistration _EventsRegistration = 0;

        private DateTime _TimeButtonPressed;
        private DateTime _TimeButtonReleased;

        public event ButtonClicked OnButtonClicked;
        public event ButtonDoubleClicked OnButtonDoubleClicked;
        public event ButtonLongClicked OnButtonLongClicked;

        // TO DO: Add those rising types
        // Changetype 1 = Rising = released
        // Changetype 2 = Falling = pressed

        // TO DO: Change long to something else?

        // TO DO: Move to new file
        public enum EventsRegistration
        {
            SinglePress = 1,
            SingleAndDoublePress = 2,
            SingleAndLongPress = 3,
            SingleAndDoubleAndLongPress = 4
        }

        /// <summary>
        /// Initializes the buttons
        /// </summary>
        public Button() : this(
            buttonPin: 37,
            doublePressMs: 500,
            longPressMs: 1000,
            eventsRegistration: EventsRegistration.SingleAndDoubleAndLongPress
            )
        {
        }

        public Button(int buttonPin, int doublePressMs, int longPressMs, EventsRegistration eventsRegistration)
        {
            _gpioController = new GpioController();
            _ButtonPin = buttonPin;
            _DoublePressMs = doublePressMs;
            _LongPressMs = longPressMs;
            _EventsRegistration = eventsRegistration;
            _TimeButtonPressed = DateTime.UtcNow;

            // Add function that sets pin as buttons 
            _gpioController.OpenPin(_ButtonPin, PinMode.Input);

            if (_EventsRegistration == EventsRegistration.SinglePress)
            {
                _gpioController.RegisterCallbackForPinValueChangedEvent(_ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChangedSinglePress);
            }
            if (_EventsRegistration == EventsRegistration.SingleAndDoublePress)
            {
                _gpioController.RegisterCallbackForPinValueChangedEvent(_ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChangedSingleAndDoublePress);
            }
            if (_EventsRegistration == EventsRegistration.SingleAndLongPress)
            {
                _gpioController.RegisterCallbackForPinValueChangedEvent(_ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChangedSingleAndLongPress);
            }
            if (_EventsRegistration == EventsRegistration.SingleAndDoubleAndLongPress)
            {
                _gpioController.RegisterCallbackForPinValueChangedEvent(_ButtonPin, PinEventTypes.Falling | PinEventTypes.Rising, ButtonStateChangedSingleAndDoubleAndLongPress);
            }
        }

        internal void ButtonStateChangedSinglePress(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if ((int)pinValueChangedEventArgs.ChangeType == 1)
            {
                OnButtonPressed();
            }
        }

        private void ButtonStateChangedSingleAndDoublePress(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if ((int)pinValueChangedEventArgs.ChangeType == 1)
            {
                ButtonDoublePressed();

                if (_State == 0)
                {
                    OnButtonPressed();
                }

                _State = 0;
            }
        }

        private void ButtonStateChangedSingleAndLongPress(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if ((int)pinValueChangedEventArgs.ChangeType == 2)
            {
                _TimeButtonPressed = DateTime.UtcNow;
            }
            if ((int)pinValueChangedEventArgs.ChangeType == 1)
            {
                ButtonLongPressed(_TimeButtonPressed);

                if (_State == 0)
                {
                    OnButtonPressed();
                }

                _State = 0;
            }
        }


        private void ButtonStateChangedSingleAndDoubleAndLongPress(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            if ((int)pinValueChangedEventArgs.ChangeType == 2)
            {
                _TimeButtonPressed = DateTime.UtcNow;
            }
            if ((int)pinValueChangedEventArgs.ChangeType == 1)
            {
                ButtonLongPressed(_TimeButtonPressed);

                ButtonDoublePressed();

                if (_State == 0)
                {
                    OnButtonPressed();
                }
                else
                {
                    _State = 0;
                }
            }
        }

        private void ButtonDoublePressed()
        {
            DateTime timeReleased = DateTime.UtcNow;

            //Time between previous released and new released
            double betweenClicksMs = timeReleased.Subtract(_TimeButtonReleased).TotalMilliseconds;

            if (betweenClicksMs <= _DoublePressMs)
            {
                OnButtonDoublePressed();
                _State = 2;
                // TO DO: double click after double click?
            }

            _TimeButtonReleased = DateTime.UtcNow;
        }

        private void ButtonLongPressed(DateTime timePressed)
        {
            DateTime timeReleased = DateTime.UtcNow;

            //Time between pressed and released
            double buttonPressedMs = timeReleased.Subtract(timePressed).TotalMilliseconds;

            if (buttonPressedMs >= _LongPressMs)
            {
                OnButtonLongPressed();
                _State = 1;
            }
        }

        private void OnButtonPressed()
        {
            //Change these from strings into enum
            OnButtonClicked?.Invoke(this, new ButtonClickedEventArgs("click"));
        }

        private void OnButtonDoublePressed()
        {
            OnButtonDoubleClicked?.Invoke(this, new ButtonDoubleClickedEventArgs("double click"));
        }

        private void OnButtonLongPressed()
        {
            OnButtonLongClicked?.Invoke(this, new ButtonLongClickedEventArgs("long click"));
        }

        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null!;
            }
        }
    }
}
