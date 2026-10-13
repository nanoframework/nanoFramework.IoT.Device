// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Apds9960
{
    internal enum Register : byte
    {
        Enable = 0x80,
        AdcIntegrationTime = 0x81,
        WaitTime = 0x83,
        AmbientLightInterruptLowThresholdLow = 0x84,
        AmbientLightInterruptLowThresholdHigh = 0x85,
        AmbientLightInterruptHighThresholdLow = 0x86,
        AmbientLightInterruptHighThresholdHigh = 0x87,
        ProximityInterruptLowThreshold = 0x89,
        ProximityInterruptHighThreshold = 0x8B,
        InterruptPersistence = 0x8C,
        Configuration1 = 0x8D,
        ProximityPulseCountAndLength = 0x8E,
        Control = 0x8F,
        Configuration2 = 0x90,
        Id = 0x92,
        Status = 0x93,
        ClearDataLow = 0x94,
        ProximityData = 0x9C,
        ProximityUpOffset = 0x9D,
        ProximityDownOffset = 0x9E,
        Configuration3 = 0x9F,
        GestureEntryThreshold = 0xA0,
        GestureExitThreshold = 0xA1,
        GestureConfiguration1 = 0xA2,
        GestureConfiguration2 = 0xA3,
        GestureUpOffset = 0xA4,
        GestureDownOffset = 0xA5,
        GestureLeftOffset = 0xA7,
        GestureRightOffset = 0xA9,
        GesturePulseCountAndLength = 0xA6,
        GestureConfiguration3 = 0xAA,
        GestureConfiguration4 = 0xAB,
        GestureFifoLevel = 0xAE,
        GestureStatus = 0xAF,
        GestureFifoUp = 0xFC,
    }
}
