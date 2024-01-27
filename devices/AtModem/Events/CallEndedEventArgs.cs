// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace IoT.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for the call ended event.
    /// </summary>
    public class CallEndedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallEndedEventArgs"/> class with the specified call duration.
        /// </summary>
        /// <param name="duration">The duration of the call.</param>
        public CallEndedEventArgs(TimeSpan duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// Gets the duration of the call.
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="CallEndedEventArgs"/> class from the provided response string.
        /// </summary>
        /// <param name="response">The response string containing call end information.</param>
        /// <returns>A new <see cref="CallEndedEventArgs"/> instance if the response contains valid call end data; otherwise, the default value of <see cref="CallEndedEventArgs"/>.</returns>
        public static CallEndedEventArgs CreateFromResponse(string response)
        {
            ////var match = Regex.Match(response, @"VOICE CALL: END: (?<duration>\d+)");            
            if (response.StartsWith("VOICE CALL: END: "))
            {
                int durationSeconds = int.Parse(response.Substring(17));
                TimeSpan duration = TimeSpan.FromSeconds(durationSeconds);
                return new CallEndedEventArgs(duration);
            }

            return default;
        }
    }
}
