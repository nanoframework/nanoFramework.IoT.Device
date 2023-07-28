////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Input Current Optimizer (ICO) status.
    /// </summary>
    public enum IcoStatus : byte
    {
        /// <summary>
        /// ICO disabled.
        /// </summary>
        Disabled = 0b0000_0000,

        /// <summary>
        /// Optimization in progress.
        /// </summary>
        InProgress = 0b0100_0000,

        /// <summary>
        /// Maximum input current detected.
        /// </summary>
        MaximumCurrentDetected = 0b1000_0000
    }
}
