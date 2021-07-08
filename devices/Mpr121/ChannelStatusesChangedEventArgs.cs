// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;

namespace Iot.Device.Mpr121
{
    /// <summary>
    /// Notifies about a the channel statuses have been changed.
    /// Refresh period can be changed by setting PeriodRefresh property.
    /// </summary>
    /// <param name="sender">The sender MPR121</param>
    /// <param name="e">The even arguments</param>
    public delegate void ChannelStatusesChanged(object sender, ChannelStatusesChangedEventArgs e);

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
