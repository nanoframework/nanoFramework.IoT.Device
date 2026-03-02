// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// Represents an ISO/IEC 15693 (NFC-V / Vicinity) card target operating at 26 or 53 kbps.
    /// </summary>
    public class Data26_53kbps
    {
        /// <summary>
        /// Creates a 26/53 kbps ISO 15693 card data structure.
        /// </summary>
        /// <param name="targetNumber">Reader-specific target identifier. For PN532 this is 1 or 2; for PN5180 ISO 15693 16-slot inventory this is the slot index (0-15).</param>
        /// <param name="afi">Application Family Identifier.</param>
        /// <param name="eas">Electronic Article Surveillance status.</param>
        /// <param name="dsfid">Data Storage Format Identifier.</param>
        /// <param name="nfcId">The 8-byte UID of the ISO 15693 card.</param>
        public Data26_53kbps(byte targetNumber, byte afi, byte eas, byte dsfid, byte[] nfcId)
        {
            TargetNumber = targetNumber;
            Afi = afi;
            Eas = eas;
            Dsfid = dsfid;
            NfcId = nfcId;
        }

        /// <summary>
        /// Reader-specific target identifier.
        /// For PN532 this is 1 or 2; for PN5180 ISO 15693 16-slot inventory this is the slot index (0-15).
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// Application Family Identifier (AFI).
        /// Represents the type of application targeted by the card (e.g., transport, banking, etc.).
        /// </summary>
        public byte Afi { get; set; }

        /// <summary>
        /// Electronic Article Surveillance (EAS) status.
        /// Used for theft-detection systems.
        /// </summary>
        public byte Eas { get; set; }

        /// <summary>
        /// Data Storage Format Identifier (DSFID).
        /// Indicates the data structure of the card memory.
        /// </summary>
        public byte Dsfid { get; set; }

        /// <summary>
        /// The 8-byte UID of the ISO 15693 card.
        /// UID bytes are stored in LSB-first order as received from the card.
        /// </summary>
        public byte[] NfcId { get; set; }
    }
}
