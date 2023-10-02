// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents a Personal Identification Number (PIN).
    /// </summary>
    public class PersonalIdentificationNumber
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalIdentificationNumber"/> class with the specified PIN.
        /// </summary>
        /// <param name="pin">The PIN.</param>
        /// <exception cref="ArgumentException">Thrown when the PIN is invalid.</exception>
        public PersonalIdentificationNumber(string pin)
        {            
            if (!long.TryParse(pin, out long res))
            {
                throw new ArgumentException("Invalid PIN");
            }

            Pin = pin;
        }

        /// <summary>
        /// Gets the PIN.
        /// </summary>
        public string Pin { get; }

        /// <summary>
        /// Returns a string representation of the PIN.
        /// </summary>
        /// <returns>The PIN as a string.</returns>
        public override string ToString()
        {
            return Pin;
        }
    }
}
