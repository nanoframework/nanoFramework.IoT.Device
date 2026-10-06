// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    internal enum Register : byte
    {
        InterruptStatus = 0x00,
        InterruptEnable = 0x01,
        FifoWritePointer = 0x02,
        FifoOverflowCounter = 0x03,
        FifoReadPointer = 0x04,
        FifoData = 0x05,
        ModeConfiguration = 0x06,
        SpO2Configuration = 0x07,
        LedConfiguration = 0x09,
        TemperatureInteger = 0x16,
        TemperatureFraction = 0x17,
        RevisionId = 0xFE,
        PartId = 0xFF,
    }
}
