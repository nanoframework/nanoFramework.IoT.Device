////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Model of the device.
    /// </summary>
    public enum Model : byte
    {
        /// <summary>
        /// No device.
        /// </summary>
        None = 0,

        /// <summary>
        /// BQ25792 device.
        /// </summary>
        Bq25792 = 0b001000,

        /// <summary>
        /// BQ25798 device.
        /// </summary>
        Bq25798 = 0b011000,
    }
}
