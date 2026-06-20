// Copyright (c) 2024 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.EPaper.Drivers.LcmEn2r13
{
    /// <summary>
    /// Commands used by the LCMEN2R13 display driver.
    /// </summary>
    internal enum Command : byte
    {
        PanelSetting = 0x00,
        PowerSetting = 0x01,
        PowerOff = 0x02,
        PowerOn = 0x04,
        BoosterSoftStart = 0x06,
        DeepSleep = 0x07,
        WritePreviousImage = 0x10,
        DisplayRefresh = 0x12,
        WriteCurrentImage = 0x13,
        TemperatureSensorControl = 0x18,
        WriteLutVcom = 0x20,
        WriteLutWhiteToWhite = 0x21,
        WriteLutBlackToWhite = 0x22,
        WriteLutWhiteToBlack = 0x23,
        WriteLutBlackToBlack = 0x24,
        PLLControl = 0x30,
        BorderWaveform = 0x3C,
        SetXAddressRange = 0x44,
        SetYAddressRange = 0x45,
        SetXAddressCounter = 0x4E,
        SetYAddressCounter = 0x4F,
        VcomAndDataIntervalSetting = 0x50,
        TconSetting = 0x60,
        ResolutionSetting = 0x61,
        VcmDcSetting = 0x82,
        PartialWindow = 0x90,
        PartialIn = 0x91,
        PartialOut = 0x92,
        CascadeSetting = 0xE0,
        ForceTemperature = 0xE5,
    }
}
