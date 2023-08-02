////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    /// <summary>
    /// Charge voltage setting. JEITA high temperature range (TWARN – THOT).
    /// </summary>
    public enum ChargeVoltage : byte
    {
        /// <summary>
        /// Suspend charge. 
        /// </summary>
        ChargeSuspend = 0,

        /// <summary>
        /// Set VREG to VREG-800mV.
        /// </summary>
        Vreg800mV = 0b0010_0000,

        /// <summary>
        /// Set VREG to VREG-600mV.
        /// </summary>
        Vreg600mV = 0b0100_0000,
        
        /// <summary>
        /// Set VREG to VREG-400mV.
        /// </summary>
        /// <remarks>
        /// This is the default value.
        /// </remarks>
        Vreg400mV = 0b0011_0000,

        /// <summary>
        /// Set VREG to VREG-300mV.
        /// </summary>
        Vreg300mV = 0b1000_0000,

        /// <summary>
        /// Set VREG to VREG-200mV.
        /// </summary>
        Vreg200mV = 0b1010_0000,
        
        /// <summary>
        /// Set VREG to VREG-100mV.
        /// </summary>
        Vreg100mV = 0b1100_0000,

        /// <summary>
        /// VREG unchanged.
        /// </summary>
        VregUnchanged = 0b1110_0000
    }
}
