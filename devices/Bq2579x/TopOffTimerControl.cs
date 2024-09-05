////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Top-off timer control.
    /// </summary>
    public enum TopOffTimerControl
    {
        /// <summary>
        /// Disabled.
        /// </summary>
        Disabled = 0b0000_0000,

        /// <summary>
        /// Top-off time is 15 minutes.
        /// </summary>
        Time_15min = 0b0100_0000,

        /// <summary>
        /// Top-off time is 30 minutes.
        /// </summary>
        Time_30min = 0b1000_0000,

        /// <summary>
        /// Top-off time is 45 minutes.
        /// </summary>
        Time_45min = 0b1100_0000,
    }
}
