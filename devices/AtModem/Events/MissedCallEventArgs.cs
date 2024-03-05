// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for a missed call.
    /// </summary>
    public class MissedCallEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MissedCallEventArgs"/> class with the specified time and phone number.
        /// </summary>
        /// <param name="time">The time of the missed call.</param>
        /// <param name="phoneNumber">The phone number from which the missed call originated.</param>
        public MissedCallEventArgs(string time, string phoneNumber)
        {
            Time = time;
            PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// Gets the time of the missed call.
        /// </summary>
        public string Time { get; }

        /// <summary>
        /// Gets the phone number from which the missed call originated.
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="MissedCallEventArgs"/> class from the response string.
        /// </summary>
        /// <param name="response">The response string containing information about the missed call.</param>
        /// <returns>An instance of the <see cref="MissedCallEventArgs"/> class with data from the response string.</returns>
        public static MissedCallEventArgs CreateFromResponse(string response)
        {
            string[] split = response.Split(new char[] { ' ' }, 3);
            return new MissedCallEventArgs(split[1], split[2]);
        }
    }
}
