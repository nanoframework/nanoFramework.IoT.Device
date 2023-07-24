// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem.DTOs;

namespace IoT.Device.AtModem.PDU
{
    /// <summary>
    /// Represents a Protocol Data Unit (PDU) message, which is the base class for SMS PDU messages.
    /// </summary>
    public abstract class PduMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PduMessage"/> class with the specified information.
        /// </summary>
        /// <param name="serviceCenterNumber">The phone number of the service center that handles the SMS.</param>
        /// <param name="senderNumber">The phone number of the sender of the SMS.</param>
        /// <param name="message">The content of the SMS message.</param>
        public PduMessage(PhoneNumber serviceCenterNumber, PhoneNumber senderNumber, string message)
        {
            ServiceCenterNumber = serviceCenterNumber;
            SenderNumber = senderNumber;
            Message = message;
        }

        /// <summary>
        /// Gets the phone number of the service center that handles the SMS.
        /// </summary>
        public PhoneNumber ServiceCenterNumber { get; }

        /// <summary>
        /// Gets the phone number of the sender of the SMS.
        /// </summary>
        public PhoneNumber SenderNumber { get; }

        /// <summary>
        /// Gets the content of the SMS message.
        /// </summary>
        public string Message { get; }
    }
}
