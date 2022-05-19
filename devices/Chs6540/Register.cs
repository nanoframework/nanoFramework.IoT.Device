// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Chs6540
{
    internal enum Register
    {
        FullRead = 0x00,
        TD_STATUS = 0x02,
        P1_XH = 0x03,
        P1_XL = 0x04,
        P1_YH = 0x05,
        P1_YL = 0x06,
        P1_WEIGHT = 0x07,
        P1_MISC = 0x08,
        P2_XH = 0x09,
        P2_XL = 0x0A,
        P2_YH = 0x0B,
        P2_YL = 0x0C,
        P2_WEIGHT = 0x0D,
        P2_MISC = 0x0E,
        InteruptOn = 0x5A
    }
}
