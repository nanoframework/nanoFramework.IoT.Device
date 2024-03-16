// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.BlueNrg2.Aci
{
    public enum OobDataType : byte
    {
        TemporaryKey = 0x00,
        RandomValue = 0x01,
        ConfirmValue = 0x02
    }
}
