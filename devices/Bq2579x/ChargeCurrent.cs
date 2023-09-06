////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charge current setting.
    /// </summary>
    public enum ChargeCurrent : byte
    {
        /// <summary>
        /// Suspend charge. 
        /// </summary>
        ChargeSuspend = 0,

        /// <summary>
        /// Set ICHG to 20%* ICHG.
        /// </summary>
        Ichg20 = 0b0000_0001,

        /// <summary>
        /// Set ICHG to 40%* ICHG.
        /// </summary>
        Ichg40 = 0b0000_0010,

        /// <summary>
        /// ICHG uncheged.
        /// </summary>
        IregUnchanged = 0b0000_0011
    }
}
