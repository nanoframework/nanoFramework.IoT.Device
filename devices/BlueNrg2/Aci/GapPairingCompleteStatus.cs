// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Status for <see cref="GapPairingCompleteEventArgs"/>.
    /// </summary>
    public enum GapPairingCompleteStatus
    {
        /// <summary>
        /// Success.
        /// </summary>
        Success = 0x00,

        /// <summary>
        /// Timeout.
        /// </summary>
        Timeout = 0x01,

        /// <summary>
        /// Pairing failed.
        /// </summary>
        PairingFailed = 0x02,

        /// <summary>
        /// Encryption failed, LTK missing on local device.
        /// </summary>
        LtkMissingOnLocalDevice = 0x03,

        /// <summary>
        /// Encryption failed, LTK missing on peer device.
        /// </summary>
        LtkMissingOnPeerDevice = 0x04,

        /// <summary>
        /// Encryption not supported by remote device.
        /// </summary>
        EncryptionNotSupportedByRemoteDevice = 0x05
    }
}
