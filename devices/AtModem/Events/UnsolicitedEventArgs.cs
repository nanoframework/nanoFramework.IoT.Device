// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.AtModem.Events
{
    /// <summary>
    /// Provides event data for an unsolicited response event.
    /// </summary>
    public class UnsolicitedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnsolicitedEventArgs"/> class with the specified lines of unsolicited response data.
        /// </summary>
        /// <param name="line1">The first line of unsolicited response data.</param>
        /// <param name="line2">The second line of unsolicited response data.</param>
        public UnsolicitedEventArgs(string line1, string line2)
        {
            Line1 = line1;
            Line2 = line2;
        }

        /// <summary>
        /// Gets the first line of unsolicited response data.
        /// </summary>
        public string Line1 { get; }

        /// <summary>
        /// Gets the second line of unsolicited response data.
        /// </summary>
        public string Line2 { get; }
    }
}
