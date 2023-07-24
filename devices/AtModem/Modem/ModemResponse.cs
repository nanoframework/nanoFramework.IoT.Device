// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.Modem
{
    /// <summary>
    /// Represents a response from the modem.
    /// </summary>
    public class ModemResponse
    {
        /// <summary>
        /// Creates a new instance of <see cref="ModemResponse"/> representing a successful response.
        /// </summary>
        /// <returns>A new instance of <see cref="ModemResponse"/> representing a successful response.</returns>
        public static ModemResponse Success() =>
            new ModemResponse(true, default, null);

        /// <summary>
        /// Creates a new instance of <see cref="ModemResponse"/> representing a response with the specified success status.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the command was successful or not.</param>
        /// <returns>A new instance of <see cref="ModemResponse"/> representing the specified response status.</returns>
        public static ModemResponse Success(bool isSuccess) =>
            new ModemResponse(isSuccess, default, null);

        /// <summary>
        /// Creates a new instance of <see cref="ModemResponse"/> representing an error response with the specified error message.
        /// </summary>
        /// <param name="error">The error message associated with the response.</param>
        /// <returns>A new instance of <see cref="ModemResponse"/> representing an error response.</returns>
        public static ModemResponse Error(string error = "") =>
            new ModemResponse(false, error, null);

        /// <summary>
        /// Creates a new instance of <see cref="ModemResponse"/> representing a successful response with the specified result.
        /// </summary>
        /// <param name="result">The result object associated with the response.</param>
        /// <returns>A new instance of <see cref="ModemResponse"/> representing a successful response with the specified result.</returns>
        public static ModemResponse ResultSuccess(object result) =>
            new ModemResponse(true, default, result);

        /// <summary>
        /// Creates a new instance of <see cref="ModemResponse"/> representing an error response with the specified error message and default result.
        /// </summary>
        /// <param name="error">The error message associated with the response.</param>
        /// <returns>A new instance of <see cref="ModemResponse"/> representing an error response with the specified error message and default result.</returns>
        public static ModemResponse ResultError(string error = "") =>
            new ModemResponse(false, error, default);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModemResponse"/> class.
        /// </summary>
        /// <param name="isSuccess">Indicates whether the command was successful or not.</param>
        /// <param name="errorMessage">The error message associated with the response. This property is only valid if <paramref name="isSuccess"/> is false.</param>
        /// <param name="result">The result object associated with the response.</param>
        public ModemResponse(bool isSuccess, string errorMessage, object result)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            Result = result;
        }

        /// <summary>
        /// Gets a value indicating whether the command was executed successfully.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the error message associated with the response. This property is only valid if the command execution was not successful (IsSuccess = false).
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Gets the result object associated with the response. The type of the result object may vary depending on the specific API call and its response.
        /// </summary>
        public object Result { get; }

        /// <summary>
        /// Returns a string representation of the <see cref="ModemResponse"/> object.
        /// </summary>
        /// <returns>A string representation of the object.</returns>
        public override string ToString()
        {
            return IsSuccess ? "Success" : $"Error: {ErrorMessage}";
        }
    }
}
