// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Reason for <see cref="GapPairingCompleteEventArgs"/>.
    /// </summary>
    public enum GapPairingCompleteReason
    {
        /// <summary>
        /// Status is not <see cref="GapPairingCompleteStatus.PairingFailed"/>
        /// </summary>
        NoValue = 0x00,

        /// <summary>
        /// Passkey entry failed.
        /// </summary>
        PasskeyEntryFailed = 0x01,

        /// <summary>
        /// OOB not available.
        /// </summary>
        OobNotAvailable = 0x02,

        /// <summary>
        /// Authentication request cannot be met.
        /// </summary>
        AuthReqCannotBeMet = 0x03,

        /// <summary>
        /// Confirm value failed.
        /// </summary>
        ConfirmValueFailed = 0x04,

        /// <summary>
        /// Pairing not supported.
        /// </summary>
        PairingNotSupported = 0x05,

        /// <summary>
        /// Insufficient encryption key size.
        /// </summary>
        InsufficientEncryptionKeySize = 0x06,

        /// <summary>
        /// Command not supported.
        /// </summary>
        CmdNotSupported = 0x07,

        /// <summary>
        /// Unspecified reason.
        /// </summary>
        UnspecifiedReason = 0x08,

        /// <summary>
        /// Very early next attempt.
        /// </summary>
        VeryEarlyNextAttempt = 0x09,

        /// <summary>
        /// Some invalid parameters.
        /// </summary>
        SmInvalidParams = 0x0A,

        /// <summary>
        /// Security: Dhkey check failed.
        /// </summary>
        SmpScDhKeyCheckFailed = 0x0B,

        /// <summary>
        /// Security: Numeric comparison failed.
        /// </summary>
        SmpScNumComparisonFailed = 0x0C
    }
}
