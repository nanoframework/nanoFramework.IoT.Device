// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Hdc1080
{
    /// <summary>
    /// Registers addresses, check data sheet, page 14, 8.6 section
    /// </summary>
    internal enum RegisterAddress : byte
    {
        TemperatureMeasurement = 0x00,
        HumidityMeasurement = 0x01,
        Configuration = 0x02,
        SerialIdFirstByte = 0xFB,
        SerialIdSecondByte = 0xFC,
        SerialIdThirdByte = 0xFD,
        ManufacturerId = 0xFE,
        DeviceId = 0xFF,
    }
}