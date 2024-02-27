// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides generic event data with a message.
    /// </summary>
    public class GenericEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericEventArgs"/> class with the specified message.
        /// </summary>
        /// <param name="message">The message associated with the event.</param>
        public GenericEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message associated with the event.
        /// </summary>
        public string Message { get; }
    }
}
