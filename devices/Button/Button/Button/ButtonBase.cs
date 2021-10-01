using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Button
{
    /// <summary>
    /// Base implementation of Button logic. 
    /// Hardware independent. Inherit for specific hardware handling.
    /// </summary>
    public class ButtonBase : IDisposable
    {
        internal const int DEFAULT_DOUBLE_PRESS_MS = 1500;
        internal const int DEFAULT_HOLDING_MS = 2000;

        private bool _disposed = false;

        private int _doublePressMs;
        private int _holdingMs;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private DateTime _lastPress = DateTime.MinValue;
        private Timer _holdingTimer;

        public delegate void ButtonPressedDelegate(object sender, EventArgs e);
        public delegate void ButtonHoldingDelegate(object sender, ButtonHoldingEventArgs e);

        public event ButtonPressedDelegate ButtonUp;
        public event ButtonPressedDelegate ButtonDown;
        public event ButtonPressedDelegate Press;
        public event ButtonPressedDelegate DoublePress;
        public event ButtonHoldingDelegate Holding;

        public bool IsHoldingEnabled { get; set; } = false;
        public bool IsDoublePressEnabled { get; set; } = false;
        public bool IsPressed { get; set; } = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        public ButtonBase(int doublePressMs = DEFAULT_DOUBLE_PRESS_MS, int holdingMs = DEFAULT_HOLDING_MS)
        {
            _doublePressMs = doublePressMs;
            _holdingMs = holdingMs;
        }

        /// <summary>
        /// Handler for pressing the button.
        /// </summary>
        protected void HandleButtonPressed()
        {
            IsPressed = true;

            ButtonDown?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled)
            {
                _holdingTimer = new Timer(StartHoldingHandler, null, _holdingMs, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Handler for releasing the button.
        /// </summary>
        protected void HandleButtonReleased()
        {
            _holdingTimer?.Dispose();
            _holdingTimer = null;

            IsPressed = false;

            ButtonUp?.Invoke(this, new EventArgs());
            Press?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
            {
                _holdingState = ButtonHoldingState.Completed;
                Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
            }

            if (IsDoublePressEnabled)
            {
                if (_lastPress == DateTime.MinValue)
                {
                    _lastPress = DateTime.UtcNow;
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(_lastPress).TotalMilliseconds <= _doublePressMs) // TO DO: Ticks per ms
                    {
                        DoublePress.Invoke(this, new EventArgs());
                    }
                    _lastPress = DateTime.MinValue;
                }
            }
        }

        /// <summary>
        /// Handler for holding the button.
        /// </summary>
        private void StartHoldingHandler(object state)
        {
            _holdingTimer.Dispose();
            _holdingTimer = null;
            _holdingState = ButtonHoldingState.Started;

            Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Started});
        }

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        /// <param name="disposing"></param>
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
            }
        }

        /// <summary>
        /// public dispose method for IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
