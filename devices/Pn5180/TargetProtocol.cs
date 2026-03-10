// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Indicates which NFC technology was used by the external reader
    /// to activate the PN5180 during Autocoll (card emulation) mode.
    /// </summary>
    /// <remarks>
    /// Determined by reading RF_STATUS register (0x1D) bits [16:14]
    /// after the CARD_ACTIVATED_IRQ fires.
    /// See PN5180A0XX-C3.pdf Table 28 – RF_STATUS register.
    /// </remarks>
    public enum TargetProtocol
    {
        /// <summary>
        /// No technology has been detected.
        /// </summary>
        None = 0,

        /// <summary>
        /// NFC-A passive (ISO 14443-A) at 106 kbps.
        /// The external reader activated the PN5180 using the
        /// ISO 14443-3A anti-collision sequence.
        /// </summary>
        NfcA = 1,

        /// <summary>
        /// NFC-F (FeliCa) passive at 212 kbps.
        /// The external reader activated the PN5180 using a
        /// SENSF_REQ polling command at 212 kbps.
        /// </summary>
        NfcF_212 = 2,

        /// <summary>
        /// NFC-F (FeliCa) passive at 424 kbps.
        /// The external reader activated the PN5180 using a
        /// SENSF_REQ polling command at 424 kbps.
        /// </summary>
        NfcF_424 = 3,
    }
}
