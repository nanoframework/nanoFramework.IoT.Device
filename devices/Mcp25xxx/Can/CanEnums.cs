// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Can
{
    public enum MASK
    {
        MASK0,
        MASK1
    };

    public enum RXF
    {
        RXF0 = 0,
        RXF1 = 1,
        RXF2 = 2,
        RXF3 = 3,
        RXF4 = 4,
        RXF5 = 5
    };

    public enum InterruptEnable : byte
    {
        DisableAll = 0,
        RXB0 = 0x01,
        RXB1 = 0x02,
        TXB0 = 0x04,
        TXB1 = 0x08,
        TXB2 = 0x10,
        ERR = 0x20,
        WAKE = 0x40,
        MSG_ERR = 0x80
    }

}
