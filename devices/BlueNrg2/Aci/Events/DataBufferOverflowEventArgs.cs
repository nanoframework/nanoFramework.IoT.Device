// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing DataBufferOverflowEventArgs.
    /// </summary>
    public class DataBufferOverflowEventArgs : EventArgs
    {
        /// <summary>
        /// On which type of channel overflow has occurred.
        /// </summary>
        public readonly LinkType LinkType;

        internal DataBufferOverflowEventArgs(LinkType linkType)
        {
            LinkType = linkType;
        }
    }
}
