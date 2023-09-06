////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charge status.
    /// </summary>
    public enum WatchdogSetting : byte
    {
        /// <summary>
        /// Watchdog disabled.
        /// </summary>
        Disable = 0,

        /// <summary>
        /// Timeout 0.5 seconds.
        /// </summary>
        TimeoutHalfSecond = 1,

        /// <summary>
        /// Timeout 1 second.
        /// </summary>
        Timeout1Second = 2,

        /// <summary>
        /// Timeout 2 seconds.
        /// </summary>
        Timeout2Seconds = 3,

        /// <summary>
        /// Timeout 20 seconds.
        /// </summary>
        Timeout20Seconds = 4,

        /// <summary>
        /// Timeout 40 seconds.
        /// </summary>
        Timeout40Seconds = 5,

        /// <summary>
        /// Timeout 80 seconds.
        /// </summary>
        Timeout80Seconds = 6,

        /// <summary>
        /// Timeout 160 seconds.
        /// </summary>
        Timeout160Seconds = 7,
    }
}
