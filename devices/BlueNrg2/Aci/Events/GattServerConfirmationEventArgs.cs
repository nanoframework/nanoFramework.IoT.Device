// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattServerConfirmationEventArgs.
    /// </summary>
    public class GattServerConfirmationEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the event.
        /// </summary>
        public readonly ushort ConnectionHandle;

        internal GattServerConfirmationEventArgs(ushort connectionHandle)
        {
            ConnectionHandle = connectionHandle;
        }
    }
}
