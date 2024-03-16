// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Type of keypress the notification reports.
    /// </summary>
    public enum KeyPressNotificationType : byte
    {
        /// <summary>
        /// Entry started.
        /// </summary>
        EntryStarted = 0x00,

        /// <summary>
        /// Digit entered.
        /// </summary>
        DigitEntered = 0x01,

        /// <summary>
        /// Digit erased.
        /// </summary>
        DigitErased = 0x02,

        /// <summary>
        /// Cleared.
        /// </summary>
        Cleared = 0x03,

        /// <summary>
        /// Entry Completed.
        /// </summary>
        EntryCompleted = 0x04
    }
}
