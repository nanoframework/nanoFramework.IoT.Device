﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;

namespace Iot.Device.Button
{
    /// <summary>
    /// Base implementation of Button logic.
    /// Hardware independent. Inherit for specific hardware handling.
    /// </summary>
    public class ButtonBase : IDisposable
    {
        internal const long DefaultDoublePressTicks = 15000000;
        internal const long DefaultHoldingMilliseconds = 2000;

        private bool _disposed = false;

        private long _doublePressTicks;
        private long _holdingMs;
        private TimeSpan _debounceTime;
        private long _debounceStartTicks;

        private ButtonHoldingState _holdingState = ButtonHoldingState.Completed;

        private long _lastPress = DateTime.MinValue.Ticks;
        private Timer _holdingTimer;

        /// <summary>
        /// Delegate for button pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ButtonPressedDelegate(object sender, EventArgs e);

        /// <summary>
        /// Delegate for button holding.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void ButtonHoldingDelegate(object sender, ButtonHoldingEventArgs e);

        /// <summary>
        /// Delegate for button up event.
        /// </summary>
        public event ButtonPressedDelegate ButtonUp;

        /// <summary>
        /// Delegate for button down event.
        /// </summary>
        public event ButtonPressedDelegate ButtonDown;

        /// <summary>
        /// Delegate for button pressed event.
        /// </summary>
        public event ButtonPressedDelegate Press;

        /// <summary>
        /// Delegate for button double pressed event.
        /// </summary>
        public event ButtonPressedDelegate DoublePress;

        /// <summary>
        /// Delegate for button holding event.
        /// </summary>
        public event ButtonHoldingDelegate Holding;

        /// <summary>
        /// Define if holding event is enabled or disabled on the button.
        /// </summary>
        public bool IsHoldingEnabled { get; set; } = false;

        /// <summary>
        /// Define if double press event is enabled or disabled on the button.
        /// </summary>
        public bool IsDoublePressEnabled { get; set; } = false;

        /// <summary>
        /// Define if single press event is enabled or disabled on the button.
        /// </summary>
        public bool IsPressed { get; set; } = false;

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        public ButtonBase() : this(TimeSpan.FromTicks(DefaultDoublePressTicks), TimeSpan.FromMilliseconds(DefaultHoldingMilliseconds))
        {
        }

        /// <summary>
        /// Initialization of the button.
        /// </summary>
        /// <param name="doublePress"></param>
        /// <param name="holding"></param>
        /// <param name="debounceTime">The amount of time during which the transitions are ignored, or zero</param>
        public ButtonBase(TimeSpan doublePress, TimeSpan holding, TimeSpan debounceTime = default(TimeSpan))
        {
            if (debounceTime.TotalMilliseconds * 3 > doublePress.TotalMilliseconds)
            {
                throw new ArgumentException($"The parameter {nameof(doublePress)} should be at least three times {nameof(debounceTime)}");
            }

            _doublePressTicks = doublePress.Ticks;
            _holdingMs = (long)holding.TotalMilliseconds;
            _debounceTime = debounceTime;
        }

        /// <summary>
        /// Handler for pressing the button.
        /// </summary>
        protected void HandleButtonPressed()
        {
            if (DateTime.UtcNow.Ticks - _debounceStartTicks < _debounceTime.Ticks)
            {
                return;
            }

            IsPressed = true;

            ButtonDown?.Invoke(this, new EventArgs());

            if (IsHoldingEnabled)
            {
                _holdingTimer = new Timer(StartHoldingHandler, null, (int)_holdingMs, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Handler for releasing the button.
        /// </summary>
        protected void HandleButtonReleased()
        {
            if (_debounceTime.Ticks > 0 && !IsPressed)
            {
                return;
            }

            _debounceStartTicks = DateTime.UtcNow.Ticks;
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
                if (_lastPress == DateTime.MinValue.Ticks)
                {
                    _lastPress = DateTime.UtcNow.Ticks;
                }
                else
                {
                    if (DateTime.UtcNow.Ticks - _lastPress <= _doublePressTicks)
                    {
                        DoublePress.Invoke(this, new EventArgs());
                    }

                    _lastPress = DateTime.MinValue.Ticks;
                }
            }
        }

        /// <summary>
        /// Handler for holding the button.
        /// </summary>
        private void StartHoldingHandler(object state)
        {
            _holdingTimer?.Dispose();
            _holdingTimer = null;
            _holdingState = ButtonHoldingState.Started;

            Holding?.Invoke(this, new ButtonHoldingEventArgs { HoldingState = ButtonHoldingState.Started });
        }

        /// <summary>
        /// Cleanup resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
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

            _disposed = true;
        }

        /// <summary>
        /// Public dispose method for IDisposable interface.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
