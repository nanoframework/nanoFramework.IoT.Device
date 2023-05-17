////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Bq2579x
{
    // refer to Table 9-12. I2C Registers in device datasheet for register definitions
    internal enum Register : byte
    {
        REG00_Minimal_System_Voltage = 0x01,
        REG08_Precharge_Control = 0x08,
        REG35_VBUS_ADC_Register = 0x35,
        REG48_Part_Information = 0x48,
    }
}
