// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;

using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Iot.Device.Board
{
    /// <summary>
    /// A GPIO Driver for testing on Windows
    /// </summary>
    [ExcludeFromCodeCoverage] // Windows-specific
    internal class KeyboardGpioDriver : GpioDriver
    {
        private enum LedKey
        {
            NumLock,
            CapsLock,
            ScrollLock,
        }

        private const int SupportedPinCount = 256;

        private KeyState[] _state;

        private Thread? _pollThread;
        private bool _terminateThread;

        public KeyboardGpioDriver()
        {
            _state = new KeyState[SupportedPinCount];
            for (int i = 0; i < SupportedPinCount; i++)
            {
                _state[i] = new KeyState((ConsoleKey)i, i);
            }

            _pollThread = null;
            _terminateThread = true;
        }

        protected override int PinCount
        {
            get
            {
                // The ConsoleKey enum is used to index into our pins, if needed. This one does not use values below 8, so
                // we'll use 3 for the LEDs.
                return SupportedPinCount;
            }
        }

        protected override int ConvertPinNumberToLogicalNumberingScheme(int pinNumber)
        {
            return pinNumber;
        }

        protected override void OpenPin(int pinNumber)
        {
        }

        protected override void ClosePin(int pinNumber)
        {
        }

        protected override void SetPinMode(int pinNumber, PinMode mode)
        {
            if (IsPinModeSupported(pinNumber, mode))
            {
                _state[pinNumber].Mode = mode;
            }
        }

        protected override PinMode GetPinMode(int pinNumber)
        {
            return _state[pinNumber].Mode;
        }

        protected override bool IsPinModeSupported(int pinNumber, PinMode mode)
        {
            if (pinNumber < 3)
            {
                // Output-only pins (the three LEDs on the keyboard)
                if (mode == PinMode.Output)
                {
                    return true;
                }

                return false;
            }

            if (pinNumber >= 8)
            {
                if (mode == PinMode.Input || mode == PinMode.InputPullDown || mode == PinMode.InputPullUp)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsKeyPressed(ConsoleKey key)
        {
            short state = Interop.GetKeyState((int)key);
            return (state & 0xFFFE) != 0; // any bits except the lowest
        }

        private void SetLedState(LedKey key, PinValue state)
        {
            int virtualKey = 0;
            if (key == LedKey.NumLock)
            {
                virtualKey = Interop.VK_NUMLOCK;
            }
            else if (key == LedKey.CapsLock)
            {
                virtualKey = Interop.VK_CAPITAL;
            }
            else if (key == LedKey.ScrollLock)
            {
                virtualKey = Interop.VK_SCROLL;
            }
            else
            {
                throw new NotSupportedException("No such key");
            }

            // Bit 1 indicates whether the LED is currently on or off (or, whether Scroll lock, num lock, caps lock is on)
            int currentKeyState = Interop.GetKeyState(virtualKey) & 1;
            if ((state == PinValue.High && currentKeyState == 0) ||
                (state == PinValue.Low && currentKeyState != 0))
            {
                // Simulate a key press
                Interop.keybd_event((byte)virtualKey,
                    0x45,
                    Interop.KEYEVENTF_EXTENDEDKEY | 0,
                    IntPtr.Zero);

                // Simulate a key release
                Interop.keybd_event((byte)virtualKey,
                    0x45,
                    Interop.KEYEVENTF_EXTENDEDKEY | Interop.KEYEVENTF_KEYUP,
                    IntPtr.Zero);
            }
        }

        protected override PinValue Read(int pinNumber)
        {
            short currentKeyState = Interop.GetAsyncKeyState(pinNumber);
            if ((currentKeyState & 0xFFFE) != 0)
            {
                return PinValue.High;
            }
            else
            {
                return PinValue.Low;
            }
        }

        protected override void Write(int pinNumber, PinValue value)
        {
            if (pinNumber == 0)
            {
                SetLedState(LedKey.NumLock, value);
            }

            if (pinNumber == 1)
            {
                SetLedState(LedKey.ScrollLock, value);
            }

            if (pinNumber == 2)
            {
                SetLedState(LedKey.CapsLock, value);
            }
        }

        protected override WaitForEventResult WaitForEvent(int pinNumber, PinEventTypes eventTypes, CancellationToken cancellationToken)
        {
            PinValue oldState = Read(pinNumber);
            while (!cancellationToken.IsCancellationRequested)
            {
                PinValue newState = Read(pinNumber);
                if (oldState != newState)
                {
                    if (eventTypes == PinEventTypes.Rising && newState == PinValue.High)
                    {
                        return new WaitForEventResult()
                        {
                            EventTypes = PinEventTypes.Rising, TimedOut = false
                        };
                    }
                    else if (eventTypes == PinEventTypes.Falling && newState == PinValue.Low)
                    {
                        return new WaitForEventResult()
                        {
                            EventTypes = PinEventTypes.Falling,
                            TimedOut = false
                        };
                    }
                    else
                    {
                        return new WaitForEventResult()
                        {
                            EventTypes = newState == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling,
                            TimedOut = false
                        };
                    }
                }
            }

            return new WaitForEventResult()
            {
                TimedOut = true
            };
        }

        protected override void AddCallbackForPinValueChangedEvent(int pinNumber, PinEventTypes eventTypes, PinChangeEventHandler callback)
        {
            lock (_state)
            {
                if (_pollThread == null)
                {
                    _terminateThread = false;
                    _pollThread = new Thread(PollingKeyThread);
                    _pollThread.IsBackground = true;
                    _pollThread.Start();
                }

                _state[pinNumber].State = Read(pinNumber);
                _state[pinNumber].EventModes = _state[pinNumber].EventModes | eventTypes;
                _state[pinNumber].Callback += callback;
            }
        }

        protected override void RemoveCallbackForPinValueChangedEvent(int pinNumber, PinChangeEventHandler callback)
        {
            bool terminate;
            lock (_state)
            {
                _state[pinNumber].Callback -= callback;
                if (_state[pinNumber].CallbacksExist == false)
                {
                    _state[pinNumber].EventModes = PinEventTypes.None;
                }

                terminate = _state.All(x => x.CallbacksExist == false);
            }

            // Can't do this within the lock - would risk a deadlock
            if (terminate && _pollThread != null)
            {
                _terminateThread = true;
                _pollThread.Join();
                _pollThread = null;
            }
        }

        /// <summary>
        /// Poor man's interrupt handling. This class is not for real production use, so doesn't really matter
        /// </summary>
        private void PollingKeyThread()
        {
            while (!_terminateThread)
            {
                lock (_state)
                {
                    foreach (var s in _state)
                    {
                        if (s.EventModes != PinEventTypes.None)
                        {
                            var newState = Read(s.PinNumber);
                            if (s.State != newState)
                            {
                                s.State = newState;
                                // Fire either way - the client has to handle that anyway (because other clients may request the other edge)
                                s.FireCallback(this, new PinValueChangedEventArgs(newState == PinValue.High ? PinEventTypes.Rising : PinEventTypes.Falling, s.PinNumber));
                            }
                        }
                    }
                }

                Thread.Sleep(10);
            }
        }

        private sealed class KeyState
        {
            public KeyState(ConsoleKey key, int pinNumber)
            {
                Key = key;
                PinNumber = pinNumber;
                State = PinValue.Low;
            }

            public event PinChangeEventHandler? Callback;

            public ConsoleKey Key
            {
                get;
            }

            public int PinNumber { get; }

            public PinMode Mode
            {
                get;
                set;
            }

            public PinValue State
            {
                get;
                set;
            }

            public PinEventTypes EventModes { get; set; }

            public bool CallbacksExist
            {
                get
                {
                    return Callback != null;
                }
            }

            public void FireCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
            {
                Callback?.Invoke(sender, pinValueChangedEventArgs);
            }
        }
    }
}
