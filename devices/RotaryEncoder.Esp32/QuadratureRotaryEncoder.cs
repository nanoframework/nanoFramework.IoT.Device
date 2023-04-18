// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.RotaryEncoder.Esp32
{
    /// <summary>
    /// Event handler to allow the notification of value changes.
    /// </summary>
    /// <param name="sender">Object that created an event.</param>
    /// <param name="e">Event args.</param>
    public delegate void RotaryEncoderEventHandler(
        object sender,
        RotaryEncoderEventArgs e);

    /// <summary>
    /// Binding that exposes a quadrature rotary encoder.
    /// </summary>
    public class QuadratureRotaryEncoder : IDisposable
    {
        private readonly GpioPulseCounter _pulseCounter;
        private readonly int _pinA;
        private readonly int _pinB;
        private uint _debounceMillisec;
        private Thread _refreshThread;
        private CancellationToken _threadRunning;
        private CancellationTokenSource _threadRunningSource;

        /// <summary>
        /// Gets the number of pulses expected per rotation of the encoder.
        /// </summary>
        public int PulsesPerRotation { get; private set; }

        /// <summary>
        /// Gets or sets the number of pulses before or after the start position of the encoder.
        /// </summary>
        public long PulseCount { get; set; }

        /// <summary>
        /// Gets the number of rotations backwards or forwards from the initial position of the encoder.
        /// </summary>
        public float Rotations { get => (float)PulseCount / PulsesPerRotation; }

        /// <summary>
        /// Gets or sets the Debounce property represents the minimum amount of delay
        /// allowed between falling edges of the A (clk) pin. The recommended value are few milliseconds typically around 5.
        /// This depends from your usage.
        /// </summary>
        public TimeSpan Debounce
        {
            get => TimeSpan.FromMilliseconds(_debounceMillisec);

            set
            {
                _debounceMillisec = (uint)value.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Gets the refresh rate used to read the encoder.
        /// </summary>
        public TimeSpan RefreshRate { get; internal set; }

        /// <summary>
        /// EventHandler to allow the notification of value changes.
        /// </summary>
        public event RotaryEncoderEventHandler? PulseCountChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuadratureRotaryEncoder" /> class.
        /// </summary>
        /// <param name="pinA">Pin A that is connected to the rotary encoder. Sometimes called clk.</param>
        /// <param name="pinB">Pin B that is connected to the rotary encoder. Sometimes called data.</param>
        /// <param name="pulsesPerRotation">The number of pulses to be received for every full rotation of the encoder.</param>
        /// <param name="refreshRate">The refresh rate.</param>
        public QuadratureRotaryEncoder(int pinA, int pinB, int pulsesPerRotation, TimeSpan refreshRate)
        {
            PulsesPerRotation = pulsesPerRotation;
            _debounceMillisec = 5;
            _pinA = pinA;
            _pinB = pinB;
            RefreshRate = refreshRate;
            _pulseCounter = new GpioPulseCounter(_pinA, _pinB);
            _threadRunningSource = new CancellationTokenSource();
            _threadRunning = _threadRunningSource.Token;
            _refreshThread = new Thread(RefreashPulse);
            _refreshThread.Start();
            _pulseCounter.Reset();
            _pulseCounter.Start();
        }

        private void RefreashPulse()
        {
            int millisec = (int)RefreshRate.TotalMilliseconds;
            long oldCount = PulseCount;
            long lastPulseTime = 0;
            GpioPulseCount pulseCount;
            while (!_threadRunning.IsCancellationRequested)
            {
                pulseCount = _pulseCounter.Read();
                PulseCount = pulseCount.Count;
                if (oldCount != PulseCount)
                {                    
                    oldCount = PulseCount;

                    // fire an event if an event handler has been attached
                    PulseCountChanged?.Invoke(this, new RotaryEncoderEventArgs(PulseCount, (int)(pulseCount.RelativeTime.TotalMilliseconds - lastPulseTime)));
                    lastPulseTime = (int)pulseCount.RelativeTime.TotalMilliseconds;
                }

                _threadRunning.WaitHandle.WaitOne(millisec, true);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _threadRunningSource?.Cancel();
            _pulseCounter?.Stop();
            _pulseCounter?.Dispose();
            if (_refreshThread != null && _refreshThread.IsAlive)
            {
                // Just make sure it's stopping as well
                _refreshThread?.Abort();
            }
        }
    }
}
