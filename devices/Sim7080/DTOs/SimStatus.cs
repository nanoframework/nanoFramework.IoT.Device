// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the SIM card status.
    /// </summary>
    public enum SimStatus
    {
        /// <summary>
        /// SIM card absent.
        /// </summary>
        SIM_ABSENT = 0,

        /// <summary>
        /// SIM card not ready.
        /// </summary>
        SIM_NOT_READY = 1,

        /// <summary>
        /// SIM card ready.
        /// </summary>
        SIM_READY = 2,

        /// <summary>
        /// SIM card requires PIN.
        /// </summary>
        SIM_PIN = 3,

        /// <summary>
        /// SIM card requires PUK.
        /// </summary>
        SIM_PUK = 4,

        /// <summary>
        /// SIM card network personalization required.
        /// </summary>
        SIM_NETWORK_PERSONALIZATION = 5,
    }
}
