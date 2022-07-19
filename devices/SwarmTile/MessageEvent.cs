// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Message event.
    /// </summary>
    public enum MessageEvent
    {
        /// <summary>
        /// No event.
        /// </summary>
        None = 0,

        /// <summary>
        /// Message received.
        /// </summary>
        Received,

        /// <summary>
        /// Message help time expired.
        /// </summary>
        Expired,

        /// <summary>
        /// Device is in sleep mode.
        /// </summary>
        Sent,
    }
}
