// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapAddressNotResolvedEventArgs.
    /// </summary>
    public class GapAddressNotResolvedEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle for which the private address
        /// could not be resolved with any of the stored IRK's.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal GapAddressNotResolvedEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}
