// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the Number Plan Identification (NPI).
    /// </summary>
    public enum NumberPlanIdentification : byte
    {
        /// <summary>
        /// Unknown number plan.
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// ISDN number plan.
        /// </summary>
        ISDN = 0x01,

        /// <summary>
        /// Data numbering plan.
        /// </summary>
        DataNumbering = 0x03,

        /// <summary>
        /// Telex number plan.
        /// </summary>
        Telex = 0x04,

        /// <summary>
        /// Service Centre Specific 1 number plan.
        /// </summary>
        ServiceCentreSpecific1 = 0x05,

        /// <summary>
        /// Service Centre Specific 2 number plan.
        /// </summary>
        ServiceCentreSpecific2 = 0x06,

        /// <summary>
        /// National numbering plan.
        /// </summary>
        NationalNumbering = 0x08,

        /// <summary>
        /// Private numbering plan.
        /// </summary>
        PrivateNumbering = 0x09,

        /// <summary>
        /// Ermes numbering plan.
        /// </summary>
        ErmesNumbering = 0x0A,

        /// <summary>
        /// Reserved for extension.
        /// </summary>
        ReservedForExtension = 0x0F,
    }
}
