// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.RegularExpressions;

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Provides utility methods for parsing AT command error codes from response strings.
    /// </summary>
    public static class AtErrorParsers
    {
        /// <summary>
        /// Tries to extract the error code from the AT command response.
        /// </summary>
        /// <param name="response">The AT command response.</param>
        /// <param name="error">When this method returns, contains the parsed error code if successful; otherwise, the default value of the error type.</param>
        /// <returns><see langword="true"/> if the error code was successfully extracted; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetError(string response, out Error error)
        {
            Match errorMatch = Regex.Match(response, @"\+(?<errorType>[A-Z]{3})\sERROR:\s(?<errorCode>\d+)");
            if (errorMatch.Success)
            {
                string errorType = errorMatch.Groups["errorType"].Value;
                int errorCode = int.Parse(errorMatch.Groups["errorCode"].Value);
                switch (errorType)
                {
                    case "CME":
                        error = new Error(errorCode, "CME error");
                        return true;
                    case "CMS":
                        error = new Error(errorCode, "CMS error");
                        return true;
                    default:
                        break;
                }
            }

            error = default;
            return false;
        }
    }
}
