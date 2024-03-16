// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Type of packet payload.
    /// </summary>
    public enum PacketPayloadType : byte
    {
        /// <summary>
        /// Pseudo-Random bit sequence 9.
        /// </summary>
        BitSequence9 = 0x00,

        /// <summary>
        /// Pattern of alternating bits '11110000'.
        /// </summary>
        AlternatingBits1 = 0x01,

        /// <summary>
        /// Pattern of alternating bits '10101010'.
        /// </summary>
        AlternatingBits2 = 0x02,

        /// <summary>
        /// Pseudo-Random bit sequence 15.
        /// </summary>
        BitSequence15 = 0x03,

        /// <summary>
        /// Pattern of All '1' bits.
        /// </summary>
        All1 = 0x04,

        /// <summary>
        /// Pattern of All '0' bits.
        /// </summary>
        All0 = 0x05,

        /// <summary>
        /// Pattern of alternating bits '00001111'.
        /// </summary>
        AlternatingBits3 = 0x06,

        /// <summary>
        /// Pattern of alternating bits '0101'.
        /// </summary>
        AlternatingBits4 = 0x07
    }
}
