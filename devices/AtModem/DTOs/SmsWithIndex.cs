// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents an SMS message with an index.
    /// </summary>
    public class SmsWithIndex : Sms
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsWithIndex"/> class.
        /// </summary>
        /// <param name="index">The index of the SMS.</param>
        /// <param name="status">The status of the SMS.</param>
        /// <param name="sender">The phone number of the sender.</param>
        /// <param name="receiveTime">The receive time of the SMS.</param>
        /// <param name="message">The content of the SMS.</param>
        public SmsWithIndex(int index, SmsStatus status, PhoneNumber sender, DateTime receiveTime, string message)
            : base(status, sender, receiveTime, message)
        {
            Index = index;
        }

        /// <summary>
        /// Gets the index of the SMS.
        /// </summary>
        public int Index { get; }
    }
}
