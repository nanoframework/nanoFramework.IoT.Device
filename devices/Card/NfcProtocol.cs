// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Card
{
    /// <summary>
    /// NFC protocol used for communication with a card.
    /// This is used by the CardTransceiver to apply the correct timing and framing.
    /// </summary>
    [Flags]
    public enum NfcProtocol
    {
        /// <summary>
        /// Unknown protocol.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// ISO/IEC 14443-3 protocol (Type A or B, anticollision level)
        /// </summary>
        Iso14443_3 = (1 << 0),

        /// <summary>
        /// ISO/IEC 14443-4 protocol (T=CL, half-duplex block transmission)
        /// </summary>
        Iso14443_4 = (1 << 1),

        /// <summary>
        /// Mifare protocol (Mifare Classic, Plus SL1)
        /// </summary>
        Mifare = (1 << 2),

        /// <summary>
        /// Innovision Jewel/Topaz protocol
        /// </summary>
        Jewel = (1 << 3),

        /// <summary>
        /// JIS X 6319-4 (FeliCa) protocol
        /// </summary>
        JisX6319_4 = (1 << 4),

        /// <summary>
        /// ISO/IEC 15693 protocol (Vicinity / NFC-V / ICODE)
        /// </summary>
        Iso15693 = (1 << 5),
    }
}
