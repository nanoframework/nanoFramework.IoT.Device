// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents product identification information.
    /// </summary>
    public class ProductIdentificationInformation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductIdentificationInformation"/> class with the specified information.
        /// </summary>
        /// <param name="information">The product identification information.</param>
        public ProductIdentificationInformation(string information)
        {
            Information = information.TrimEnd('\r', '\n');
        }

        /// <summary>
        /// Gets the product identification information.
        /// </summary>
        public string Information { get; }

        /// <summary>
        /// Returns a string representation of the product identification information.
        /// </summary>
        /// <returns>The product identification information.</returns>
        public override string ToString()
        {
            return Information;
        }
    }
}
