// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the status of an SMS message.
    /// </summary>
    public enum SmsStatus
    {
        /// <summary>
        /// Received, unread status.
        /// </summary>
        REC_UNREAD,

        /// <summary>
        /// Received, read status.
        /// </summary>
        REC_READ,

        /// <summary>
        /// Stored, unsent status.
        /// </summary>
        STO_UNSENT,

        /// <summary>
        /// Stored, sent status.
        /// </summary>
        STO_SENT,

        /// <summary>
        /// All statuses.
        /// </summary>
        ALL,
    }
}
