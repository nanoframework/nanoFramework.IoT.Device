// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.DTOs
{
    /// <summary>
    /// Represents the details of a call.
    /// </summary>
    public class CallDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallDetails"/> class with the specified duration.
        /// </summary>
        /// <param name="duration">The duration of the call.</param>
        public CallDetails(TimeSpan duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// Gets the duration of the call.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Returns a string representation of the call details.
        /// </summary>
        /// <returns>A string representation of the call details.</returns>
        public override string ToString()
        {
            return $"Duration: {Duration}";
        }
    }
}
