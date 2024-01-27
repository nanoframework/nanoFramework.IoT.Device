////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charge status.
    /// </summary>
    public enum ChargeStatus : byte
    {
        /// <summary>
        /// Not charging.
        /// </summary>
        NotCharging = 0b0000_0000,

        /// <summary>
        /// Trickle Charge.
        /// </summary>
        Trickle = 0b0010_0000,

        /// <summary>
        /// Pre-Charge.
        /// </summary>
        PreCharge = 0b0100_0000,

        /// <summary>
        /// Fast Charge (CC mode).
        /// </summary>
        FastCharge = 0b0110_0000,

        /// <summary>
        /// Taper Charge (CV mode).
        /// </summary>
        TaperCharge = 0b1000_0000,

        /// <summary>
        /// Top-off Timer Active Charging.
        /// </summary>
        TopOffTimer = 0b1100_0000,
        
        /// <summary>
        /// Charge termination done.
        /// </summary>
        ChargeTerminationDone = 0b1110_0000,
    }
}
