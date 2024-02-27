// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents a phone number.
    /// </summary>
    public class PhoneNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumber"/> class with the specified phone number.
        /// </summary>
        /// <param name="number">The phone number.</param>
        public PhoneNumber(string number)
        {
            Number = number;
        }

        /// <summary>
        /// Gets the phone number.
        /// </summary>
        public string Number { get; }

        /// <summary>
        /// Returns a string representation of the phone number.
        /// </summary>
        /// <returns>The phone number as a string.</returns>
        public override string ToString()
        {
            return Number;
        }
    }
}
