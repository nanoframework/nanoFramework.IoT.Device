////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Top-off timer control.
    /// </summary>
    public enum FastChargeTimerSetting
    {
        /// <summary>
        /// Fast charge timer set to 5 hours.
        /// </summary>
        Time_5H = 0b0000_0000,

        /// <summary>
        /// Fast charge timer set to 8 hours.
        /// </summary>
        Time_8H = 0b0000_0010,

        /// <summary>
        /// Fast charge timer set to 12 hours.
        /// </summary>
        /// <remarks>
        /// This is the default value.
        /// </remarks>
        Time_12H = 0b0000_0100,

        /// <summary>
        /// Fast charge timer set to 24 hours.
        /// </summary>
        Time_24H = 0b0000_0110,
    }
}
