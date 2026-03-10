// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Autocoll mode flags for the SWITCH_MODE command (byte 2).
    /// These values are OR-able to enable multiple NFC technologies
    /// simultaneously during card emulation.
    /// </summary>
    /// <remarks>
    /// See PN5180A0XX-C3.pdf §11.4.4 "SWITCH_MODE" for details.
    /// When entering Autocoll mode the PN5180 will listen for an
    /// external RF field and perform collision resolution for the
    /// selected technology/technologies automatically.
    /// </remarks>
    [Flags]
    public enum AutocollMode : byte
    {
        /// <summary>
        /// Collision resolution for NFC-A (ISO 14443-A) at 106 kbps.
        /// The PN5180 will respond to SENS_REQ / ALL_REQ and handle
        /// the full anti-collision and selection sequence using the
        /// SENS_RES, NFCID1 and SEL_RES values stored in EEPROM.
        /// </summary>
        CollisionResolutionNfcA = 0x01,

        /// <summary>
        /// Collision resolution for NFC-F (FeliCa) at 212 kbps.
        /// The PN5180 will respond to SENSF_REQ polling commands
        /// using the FeliCa Polling Response stored in EEPROM
        /// (addresses 0x46–0x50).
        /// </summary>
        CollisionResolutionNfcF_212 = 0x02,

        /// <summary>
        /// Collision resolution for NFC-F (FeliCa) at 424 kbps.
        /// Same as NFC-F 212 but at the higher bit rate.
        /// </summary>
        CollisionResolutionNfcF_424 = 0x04,

        /// <summary>
        /// Collision resolution for NFC-A and NFC-F at all supported speeds.
        /// Equivalent to <see cref="CollisionResolutionNfcA"/> |
        /// <see cref="CollisionResolutionNfcF_212"/> |
        /// <see cref="CollisionResolutionNfcF_424"/>.
        /// </summary>
        CollisionResolutionNfcAAndF = CollisionResolutionNfcA | CollisionResolutionNfcF_212 | CollisionResolutionNfcF_424,
    }
}
