// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents an SMS message.
    /// </summary>
    public class Sms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sms"/> class.
        /// </summary>
        /// <param name="status">The status of the SMS message.</param>
        /// <param name="sender">The phone number of the sender.</param>
        /// <param name="receiveTime">The receive time of the SMS message.</param>
        /// <param name="message">The content of the SMS message.</param>
        public Sms(SmsStatus status, PhoneNumber sender, DateTime receiveTime, string message)
        {
            Status = status;
            Sender = sender;
            ReceiveTime = receiveTime;
            Message = message;
        }

        /// <summary>
        /// Gets the status of the SMS message.
        /// </summary>
        public SmsStatus Status { get; }

        /// <summary>
        /// Gets the phone number of the sender.
        /// </summary>
        public PhoneNumber Sender { get; }

        /// <summary>
        /// Gets the receive time of the SMS message.
        /// </summary>
        public DateTime ReceiveTime { get; }

        /// <summary>
        /// Gets the content of the SMS message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Returns a string representation of the SMS message.
        /// </summary>
        /// <returns>The string representation of the SMS message.</returns>
        public override string ToString()
        {
            return $"Sender: {Sender}, ReceiveTime: {ReceiveTime}, Message:\r\n{Message}";
        }
    }
}
