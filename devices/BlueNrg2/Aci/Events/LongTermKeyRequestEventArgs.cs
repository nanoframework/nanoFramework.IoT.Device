// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing LongTermKeyRequestEventArgs.
    /// </summary>
    public class LongTermKeyRequestEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle to be used to identify the connection with the peer device.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// 64-bit random number.
        /// </summary>
        public readonly ulong RandomNumber;

        /// <summary>
        /// 16-bit encrypted diversifier.
        /// </summary>
        public readonly ushort EncryptedDiversifier;

        internal LongTermKeyRequestEventArgs(ushort connectionHandle, ulong randomNumber, ushort encryptedDiversifier)
        {
            ConnectionHandle = connectionHandle;
            RandomNumber = randomNumber;
            EncryptedDiversifier = encryptedDiversifier;
        }
    }
}
