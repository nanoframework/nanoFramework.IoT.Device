// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents a subscriber number with its associated status and address type.
    /// </summary>
    public class SubscriberNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriberNumber"/> class with the specified parameters.
        /// </summary>
        /// <param name="number">The subscriber number.</param>
        /// <param name="status">The status associated with the subscriber number.</param>
        /// <param name="addressType">The address type of the subscriber number.</param>
        public SubscriberNumber(string number, string status, int addressType)
        {
            Number = number;
            Status = status;
            AddressType = addressType;
        }

        /// <summary>
        /// Gets or sets the subscriber number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets the status associated with the subscriber number.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the address type of the subscriber number.
        /// </summary>
        public int AddressType { get; set; }
    }
}
