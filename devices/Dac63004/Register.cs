////
// Copyright (c) .NET Foundation and Contributors
// See LICENSE file in the project root for full license information.
////

namespace Iot.Device.Dac63004
{
    // refer to Table 7-21. Register Names in device datasheet for register definitions
    internal enum Register : byte
    {
        Reg03_DAC0_VoutCompareConfig = 0x03,
        Reg09_DAC1_VoutCompareConfig = 0x09,
        Reg0F_DAC2_VoutCompareConfig = 0x0F,
        Reg15_DAC3_VoutCompareConfig = 0x15,
        Reg19_DAC0_Data = 0x19,
        Reg1A_DAC1_Data = 0x1A,
        Reg1B_DAC2_Data = 0x1B,
        Reg1C_DAC3_Data = 0x1C,
        Reg1F_CommonConfig = 0x1F,
        Reg22_GeneralStatus = 0x22,
    }
}
