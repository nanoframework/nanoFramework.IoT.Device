// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represent a specific type of number.
    /// </summary>
    public enum TypeOfNumber : byte
    {
        /// <summary>
        /// Unknown type of number.
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// International number.
        /// </summary>
        International = 0x01,

        /// <summary>
        /// National number.
        /// </summary>
        National = 0x02,

        /// <summary>
        /// Network-specific number.
        /// </summary>
        NetworkSpecific = 0x03,

        /// <summary>
        /// Subscriber number.
        /// </summary>
        Subscriber = 0x04,

        /// <summary>
        /// Alphanumeric number.
        /// </summary>
        AlphaNumeric = 0x05,

        /// <summary>
        /// Abbreviated number.
        /// </summary>
        Abbreviated = 0x06,

        /// <summary>
        /// Reserved for extension.
        /// </summary>
        ReservedForExtension = 0x07,
    }
}
