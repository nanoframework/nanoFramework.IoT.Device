// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Class to hold the AT response.
    /// </summary>
    public class AtResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the final response message.
        /// </summary>
        public string FinalResponse { get; set; }

        /// <summary>
        /// Gets or sets the collection of intermediate responses.
        /// </summary>
        public ArrayList Intermediates { get; set; } = new ArrayList();

        /// <summary>
        /// Returns a string representation of the AtResponse object.
        /// </summary>
        /// <returns>A string containing the success status, final response, and the count of intermediates.</returns>
        public override string ToString()
        {
            return $"Success: {Success}, FinalResponse: {FinalResponse}, Intermediates: {Intermediates.Count}";
        }
    }
}
