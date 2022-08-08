// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2
{
    /// <summary>
    /// Type of power level to transmit.
    /// </summary>
    public enum TransmitPowerLevelType
    {
        /// <summary>
        /// The current power level.
        /// </summary>
        Current = 0x00,

        /// <summary>
        /// The maximum power level.
        /// </summary>
        Maximum = 0x01
    }
}
