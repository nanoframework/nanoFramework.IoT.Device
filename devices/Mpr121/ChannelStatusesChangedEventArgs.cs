// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Represents the arguments of event rising when the channel statuses have been changed.
    /// </summary>
    public class ChannelStatusesChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The channel statuses.
        /// </summary>
        public bool[] ChannelStatuses { get; private set; }

        /// <summary>
        /// Initialize event arguments.
        /// </summary>
        /// <param name="channelStatuses">The channel statuses.</param>
        public ChannelStatusesChangedEventArgs(bool[] channelStatuses)
            : base()
        {
            ChannelStatuses = channelStatuses;
        }
    }
}
