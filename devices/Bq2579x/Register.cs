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
        REG01_Charge_Voltage_Limit = 0x01,
        REG03_Charge_Current_Limit = 0x03,
        REG05_Input_Voltage_Limit = 0x05,
        REG06_Input_Current_Limit = 0x06,
        REG08_Precharge_Control = 0x08,
        REG10_Charger_Control_1 = 0x10,
        REG1B_Charger_Status_0 = 0x1B,
        REG1C_Charger_Status_1 = 0x1C,
        REG2E_ADC_Control = 0x2E,
        REG35_VBUS_ADC = 0x35,
        REG37_VAC1_ADC = 0x37,
        REG39_VAC2_ADC = 0x39,
        REG3B_VBAT_ADC = 0x3B,
        REG3D_VSYS_ADC = 0x3D,
        REG41_TDIE_ADC = 0x41,
        REG48_Part_Information = 0x48,
    }
}
