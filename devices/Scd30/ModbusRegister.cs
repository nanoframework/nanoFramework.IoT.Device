// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Scd30
{
    internal enum ModbusRegister : byte
    {
        START_MEASUREMENT_ADDR = 0x36,
        STOP_MEASUREMENT_ADDR = 0x37,
        MEASUREMENT_INTERVAL_ADDR = 0x25,
        DATA_READY_STATUS_ADDR = 0x27,
        READ_MEASUREMENT_ADDR = 0x28,
        AUTOMATIC_SELF_CALIBRATION_ADDR = 0x3a,
        FORCED_RECALIBRATION_ADDR = 0x39,
        TEMPERATURE_OFFSET_ADDR = 0x3b,
        ALTITUDE_COMPENSATION_ADDR = 0x38,
        FIRMWARE_VERSION_ADDR = 0x20,
        SOFT_RESET_ADDR = 0x34
    }
}
