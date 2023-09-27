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
        Absent = 0,

        /// <summary>
        /// SIM card not ready.
        /// </summary>
        NotReady = 1,

        /// <summary>
        /// SIM card ready.
        /// </summary>
        Ready = 2,

        /// <summary>
        /// SIM card requires PIN.
        /// </summary>
        PinRequired = 3,

        /// <summary>
        /// SIM card requires PUK.
        /// </summary>
        PukRequired = 4,

        /// <summary>
        /// SIM card network personalization required.
        /// </summary>
        NetworkPersonalization = 5,
    }
}
