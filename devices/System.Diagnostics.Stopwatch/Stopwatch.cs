// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Diagnostics
{
    /// <summary>
    /// Provides a set of methods and properties that you can use to accurately measure elapsed time.
    /// </summary>
    public class Stopwatch
    {
        private long _initialMilliseconds;
        private long _finalMilliseconds;

        /// <summary>
        /// Gets the frequency of the timer as the number of ticks per second. This field is read-only.
        /// </summary>
        public static readonly long Frequency = TimeSpan.TicksPerSecond;

        /// <summary>
        /// Indicates whether the timer is based on a high-resolution performance counter.
        /// This field is read-only.
        /// </summary>
        public static readonly bool IsHighResolution = false;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance.
        /// </summary>
        public TimeSpan Elapsed => TimeSpan.FromTicks(ElapsedTicks);

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in milliseconds.
        /// </summary>
        public long ElapsedMilliseconds => ElapsedTicks / TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Gets the total elapsed time measured by the current instance, in timer ticks.
        /// </summary>
        public long ElapsedTicks => (IsRunning ? Environment.TickCount64 - _initialMilliseconds : _finalMilliseconds - _initialMilliseconds) * TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Gets a value indicating whether the System.Diagnostics.Stopwatch timer is running.
        /// </summary>
        public bool IsRunning { get; internal set; }

        /// <summary>
        /// Gets the current number of ticks in the timer mechanism.
        /// </summary>
        /// <returns>Number of ticks.</returns>
        public static long GetTimestamp() => Environment.TickCount64 * TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Initializes a new System.Diagnostics.Stopwatch instance, sets the elapsed time
        /// property to zero, and starts measuring elapsed time.
        /// </summary>
        /// <returns>A System.Diagnostics.Stopwatch that has just begun measuring elapsed time.</returns>
        public static Stopwatch StartNew()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            return stopwatch;
        }

        /// <summary>
        /// Stops time interval measurement and resets the elapsed time to zero.
        /// </summary>
        public void Reset()
        {
            IsRunning = false;
            _initialMilliseconds = Environment.TickCount64;
            _finalMilliseconds = _initialMilliseconds;
        }

        /// <summary>
        /// Stops time interval measurement, resets the elapsed time to zero, and starts
        /// measuring elapsed time.
        /// </summary>
        public void Restart()
        {
            Start();
        }

        /// <summary>
        /// Starts, or resumes, measuring elapsed time for an interval.
        /// </summary>
        public void Start()
        {
            _initialMilliseconds = Environment.TickCount64;
            IsRunning = true;
        }

        /// <summary>
        /// Stops measuring elapsed time for an interval.
        /// </summary>
        public void Stop()
        {
            _finalMilliseconds = Environment.TickCount64;
            IsRunning = false;
        }
    }
}