// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rfid
{
    /// <summary>
    /// 212 and 424 card elements
    /// </summary>
    public class Data212_424kbps
    {
        /// <summary>
        /// 212 and 424 card elements.
        /// </summary>
        /// <param name="targetNumber">The target number, should be 1 or 2 with PN532.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="nfcId">The unique NFC ID.</param>
        /// <param name="pad">The Pad.</param>
        /// <param name="systemCode">The system code.</param>
        public Data212_424kbps(byte targetNumber, byte responseCode, byte[] nfcId, byte[] pad, byte[] systemCode)
        {
            TargetNumber = targetNumber;
            ResponseCode = responseCode;
            NfcId = nfcId;
            Pad = pad;
            SystemCode = systemCode;
        }

        /// <summary>
        /// The target number, should be 1 or 2 with PN532
        /// </summary>
        public byte TargetNumber { get; set; }

        /// <summary>
        /// The response code
        /// </summary>
        public byte ResponseCode { get; set; }

        /// <summary>
        /// The unique NFC ID
        /// </summary>
        public byte[] NfcId { get; set; }

        /// <summary>
        /// The Pad
        /// </summary>
        public byte[] Pad { get; set; }

        /// <summary>
        /// The system code
        /// </summary>
        public byte[] SystemCode { get; set; }
    }
}
