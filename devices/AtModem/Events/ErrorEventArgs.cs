// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for the error event.
    /// </summary>
    public class ErrorEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEventArgs"/> class with the specified error message.
        /// </summary>
        /// <param name="error">The error message associated with the event.</param>
        public ErrorEventArgs(string error)
        {
            Error = error;
        }

        /// <summary>
        /// Gets the error message associated with the event.
        /// </summary>
        public string Error { get; }
    }
}
