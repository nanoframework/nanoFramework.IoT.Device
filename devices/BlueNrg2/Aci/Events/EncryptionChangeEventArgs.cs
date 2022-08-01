// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing EncryptionChangeEventArgs.
    /// </summary>
    public class EncryptionChangeEventArgs : EventArgs
    {
        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Connection handle that identifies the connection.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Link Level Encryption.
        /// </summary>
        public readonly byte EncryptionEnabled;

        internal EncryptionChangeEventArgs(byte status, ushort connectionHandle, byte encryptionEnabled)
        {
            Status = status;
            ConnectionHandle = connectionHandle;
            EncryptionEnabled = encryptionEnabled;
        }
    }
}
