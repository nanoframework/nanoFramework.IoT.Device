// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem
{
    /// <summary>
    /// Represents an error in an AT command.
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Initializes a new instance of the Error class.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        public Error(int errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Returns a string representation of the Error object.
        /// </summary>
        /// <returns>A string containing the error message and error code.</returns>
        public override string ToString()
        {
            return $"{ErrorMessage} (Error code {ErrorCode})";
        }
    }
}
