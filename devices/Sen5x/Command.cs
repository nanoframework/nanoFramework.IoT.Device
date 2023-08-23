// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Sen5x
{
    /// <summary>
    /// Commands for the SEN5x sensors.
    /// </summary>
    internal enum Command : ushort
    {
        StartMeasurement = 0x0021,
        StartMeasurementWithoutPM = 0x0037,
        StopMeasurement = 0x0104,
        DataReadyFlag = 0x0202,
        Measurement = 0x03c4,
        TemperatureCompensationParameters = 0x60b2,
        WarmStartParameters = 0x60c6,
        VocAlgorithmTuningParameters = 0x60d0,
        NoxAlgorithmTuningParameters = 0x60e1,
        RhtAccelerationModeParameters = 0x60f7,
        VocAlgorithmState = 0x6181,
        StartFanCleaning = 0x5607,
        AutoCleaningIntervalParameters = 0x8004,
        ProductName = 0xd014,
        SerialNumber = 0xd033,
        FirmwareVersion = 0xd100,
        ReadDeviceStatus = 0xd206,
        ClearDeviceStatus = 0xd210,
        DeviceReset = 0xd304
    }
}
