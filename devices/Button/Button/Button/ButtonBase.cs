using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Button
{
    /// <summary>
    /// Base implementation of Button logic. Hardware independent. Inherit for specific hardware handling.
    /// </summary>
    public class ButtonBase : IDisposable
    {
        internal const int DEFAULT_DOUBLE_PRESS_MS = 500;
        internal const int DEFAULT_LONG_PRESS_MS = 1000;

        private bool _disposed = false;

        private int _doublePressMs;
        private int _longPressMs;

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
        public ButtonBase(int doublePressMs = DEFAULT_DOUBLE_PRESS_MS, int longPressMs = DEFAULT_LONG_PRESS_MS)
        {
            _doublePressMs = doublePressMs;
            _longPressMs = longPressMs;

            _lastClick = DateTime.UtcNow;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void HandleButtonReleased()
        {
            _holdingTimer?.Dispose();
            _holdingTimer = null;

            IsPressed = false;

            ButtonUp?.Invoke(this, new EventArgs());
            Click?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled && _holdingState == ButtonHoldingState.Started)
            {
                _holdingState = ButtonHoldingState.Completed;
                Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Completed });
            }

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
        }

        /// <summary>
        /// 
        /// </summary>
        internal void HandleButtonPressed()
        {
            IsPressed = true;

            ButtonDown?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled)
            {
                _holdingTimer = new Timer(StartHoldingHandler, null, _longPressMs, Timeout.Infinite);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
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
