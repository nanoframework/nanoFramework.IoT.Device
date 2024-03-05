// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.PDU
{
    /// <summary>
    /// Specifies the type of PDU (Protocol Data Unit) used in SMS (Short Message Service) communication.
    /// </summary>
    public enum PduType : byte
    {
        /// <summary>
        /// SMS Deliver Report PDU. Represents a status report for an SMS message delivered to the recipient's device.
        /// </summary>
        SMS_DELIVER_REPORT = 0x00,

        /// <summary>
        /// SMS Deliver PDU. Represents an SMS message delivered to the recipient's device.
        /// </summary>
        SMS_DELIVER = 0x00,

        /// <summary>
        /// SMS Submit PDU. Represents an SMS message submitted for delivery.
        /// </summary>
        SMS_SUBMIT = 0x01,

        /// <summary>
        /// SMS Submit Report PDU. Represents a status report for an SMS message that has been submitted for delivery.
        /// </summary>
        SMS_SUBMIT_REPORT = 0x01,

        /// <summary>
        /// SMS Command PDU. Reserved for future use. Represents a command message for the SMS center.
        /// </summary>
        SMS_COMMAND = 0x10,

        /// <summary>
        /// SMS Status Report PDU. Reserved for future use. Represents a status report for an SMS message.
        /// </summary>
        SMS_STATUS_REPORT = 0x10,

        /// <summary>
        /// Reserved PDU type. Its usage is not specified.
        /// </summary>
        Reserved = 0x11
    }
}
