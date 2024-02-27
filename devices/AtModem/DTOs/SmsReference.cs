// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents a reference to an SMS message.
    /// </summary>
    public class SmsReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsReference"/> class.
        /// </summary>
        /// <param name="messageReference">The message reference.</param>
        public SmsReference(int messageReference)
        {
            MessageReference = messageReference;
        }

        /// <summary>
        /// Gets the message reference.
        /// </summary>
        public int MessageReference { get; }

        /// <summary>
        /// Returns a string representation of the SMS reference.
        /// </summary>
        /// <returns>The string representation of the SMS reference.</returns>
        public override string ToString()
        {
            return $"Message Reference: {MessageReference}";
        }
    }
}
