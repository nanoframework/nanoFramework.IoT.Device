// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the International Mobile Subscriber Identity (IMSI).
    /// </summary>
    public class Imsi
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Imsi"/> class with the specified IMSI value.
        /// </summary>
        /// <param name="value">The IMSI value.</param>
        public Imsi(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the IMSI value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Returns a string representation of the IMSI.
        /// </summary>
        /// <returns>The IMSI value as a string.</returns>
        public override string ToString()
        {
            return Value;
        }
    }
}
