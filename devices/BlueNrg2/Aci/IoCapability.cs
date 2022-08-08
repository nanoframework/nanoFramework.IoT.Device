// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum IoCapability : byte
    {
        DisplayOnly = 0x00,
        DisplayYesNo = 0x01,
        KeyboardOnly = 0x02,
        NoInputNoOutput = 0x03,
        KeyboardDisplay = 0x04
    }
}
