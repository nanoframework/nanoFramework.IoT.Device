// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum PacketPayloadType : byte
    {
        PseudoRandomBitSequence9 = 0x00,
        AlternatingBits11110000 = 0x01,
        AlternatingBits10101010 = 0x02,
        PseudoRandomBitSequence15 = 0x03,
        All1 = 0x04,
        All0 = 0x05,
        AlternatingBits00001111 = 0x06,
        AlternatingBits01010101 = 0x07
    }
}
