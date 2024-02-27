// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtModem.DTOs;

namespace Iot.Device.AtModem.PDU
{
    /// <summary>
    /// Represents an SMS submit message to be sent to the service center.
    /// </summary>
    public class SmsSubmit : PduMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsSubmit"/> class with the specified information.
        /// </summary>
        /// <param name="serviceCenterNumber">The phone number of the service center to which the SMS is being sent.</param>
        /// <param name="senderNumber">The phone number of the sender who is sending the SMS.</param>
        /// <param name="message">The content of the SMS message to be sent.</param>
        public SmsSubmit(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message)
            : base(serviceCenterNumber, senderNumber, message)
        {
        }
    }
}
