// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Adc
{
    internal enum Ina236Register : byte
    {
        Configuration = 0x00,
        ShuntVoltage = 0x01,
        BusVoltage = 0x02,
        Power = 0x03,
        Current = 0x04,
        Calibration = 0x05,
        MaskEnable = 0x06,
        AlertLimit = 0x07,
        ManufacturerId = 0x3E,
        DeviceId = 0x3F
    }
}