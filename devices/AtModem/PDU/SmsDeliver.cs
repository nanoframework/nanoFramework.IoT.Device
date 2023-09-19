// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem.DTOs;
using System;

namespace IoT.Device.AtModem.PDU
{
    /// <summary>
    /// Represents an SMS deliver message received from the service center.
    /// </summary>
    public class SmsDeliver : PduMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsDeliver"/> class with the specified information.
        /// </summary>
        /// <param name="serviceCenterNumber">The phone number of the service center that sent the SMS.</param>
        /// <param name="senderNumber">The phone number of the sender who sent the SMS.</param>
        /// <param name="message">The content of the SMS message.</param>
        /// <param name="timestamp">The date and time when the SMS was received from the service center.</param>
        public SmsDeliver(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message, DateTime timestamp)
            : base(serviceCenterNumber, senderNumber, message)
        {
            TimeStamp = timestamp;
        }

        /// <summary>
        /// Gets the date and time when the SMS was received from the service center.
        /// </summary>
        public DateTime TimeStamp { get; }
    }
}
