// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the format of an SMS text.
    /// </summary>
    public enum SmsTextFormat
    {
        /// <summary>
        /// Protocol Data Unit (PDU) format.
        /// </summary>
        PDU = 0,

        /// <summary>
        /// Text format.
        /// </summary>
        Text,
    }
}
