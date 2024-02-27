// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for a USSD response event.
    /// </summary>
    public class UssdResponseEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UssdResponseEventArgs"/> class with the specified status, response message, and coding scheme.
        /// </summary>
        /// <param name="status">The status code indicating the USSD response status.</param>
        /// <param name="response">The response message received from the USSD session.</param>
        /// <param name="codingScheme">The coding scheme used for the USSD response message.</param>
        public UssdResponseEventArgs(int status, string response, int codingScheme)
        {
            Status = status;
            Response = response;
            CodingScheme = codingScheme;
        }

        /// <summary>
        /// Gets the status code indicating the USSD response status.
        /// </summary>
        public int Status { get; }

        /// <summary>
        /// Gets the response message received from the USSD session.
        /// </summary>
        public string Response { get; }

        /// <summary>
        /// Gets the coding scheme used for the USSD response message.
        /// </summary>
        public int CodingScheme { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="UssdResponseEventArgs"/> class from the response string.
        /// </summary>
        /// <param name="response">The response string containing information about the USSD response.</param>
        /// <returns>An instance of the <see cref="UssdResponseEventArgs"/> class with data from the response string.</returns>
        public static UssdResponseEventArgs CreateFromResponse(string response)
        {
            if (response.StartsWith("+CUSD: "))
            {
                string[] parts = response.Split(',');
                int status = int.Parse(parts[0]);
                string message = parts[1].Trim('"');
                int codingScheme = int.Parse(parts[2]);
                return new UssdResponseEventArgs(status, message, codingScheme);
            }

            return default;
        }
    }
}
