// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for an SMS received event.
    /// </summary>
    public class SmsReceivedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsReceivedEventArgs"/> class with the specified storage and index.
        /// </summary>
        /// <param name="storage">The storage location where the received SMS is stored.</param>
        /// <param name="index">The index of the received SMS in the storage.</param>
        public SmsReceivedEventArgs(string storage, int index)
        {
            Storage = storage;
            Index = index;
        }

        /// <summary>
        /// Gets the storage location where the received SMS is stored.
        /// </summary>
        public string Storage { get; }

        /// <summary>
        /// Gets the index of the received SMS in the storage.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Creates a new instance of the <see cref="SmsReceivedEventArgs"/> class from the response string.
        /// </summary>
        /// <param name="response">The response string containing information about the received SMS.</param>
        /// <returns>An instance of the <see cref="SmsReceivedEventArgs"/> class with data from the response string.</returns>
        public static SmsReceivedEventArgs CreateFromResponse(string response)
        {
            if (response.StartsWith("+CMTI: "))
            {
                string[] sms = response.Substring(8).Split(',');
                string storage = sms[0].Trim('"');
                int index = int.Parse(sms[1].Trim('"'));
                return new SmsReceivedEventArgs(storage, index);
            }

            return default;
        }
    }
}
