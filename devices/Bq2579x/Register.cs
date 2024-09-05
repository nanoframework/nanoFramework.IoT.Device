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
        REG09_Charge_Termination_Current = 0x09,
        REG0E_Timer_Control = 0x0E,
        REG0F_Charger_Control_0 = 0x0F,
        REG10_Charger_Control_1 = 0x10,
        REG11_Charger_Control_2 = 0x11,
        REG12_Charger_Control_3 = 0x12,
        REG13_Charger_Control_4 = 0x13,
        REG14_Charger_Control_5 = 0x14,
        REG16_Temperature_Control = 0x16,
        REG17_NTC_Control_0 = 0x17,
        REG18_NTC_Control_1 = 0x18,
        REG1B_Charger_Status_0 = 0x1B,
        REG1C_Charger_Status_1 = 0x1C,
        REG1D_Charger_Status_2 = 0x1D,
        REG1E_Charger_Status_3 = 0x1E,
        REG1F_Charger_Status_4 = 0x1F,
        REG20_FAULT_Status_0 = 0x20,
        REG21_FAULT_Status_1 = 0x21,
        REG22_Charger_Flag_0 = 0x22,
        REG23_Charger_Flag_1 = 0x23,
        REG24_Charger_Flag_2 = 0x24,
        REG25_Charger_Flag_3 = 0x25,
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
