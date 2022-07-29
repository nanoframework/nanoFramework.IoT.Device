// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Rtc
{
    internal enum PCF85263ControlRegisters : byte
    {
        //Offset register
        CTRL_OFFSET_ADDR = 0x24,
        //Control registers
        CTRL_OSCILLATOR_ADDR = 0x25,
        CTRL_BATTERY_SWITCH_ADDR = 0x26,
        CTRL_PIN_IO_ADDR = 0x27,
        CTRL_FUNCTION_ADDR = 0x28,
        CTRL_INTA_ENABLE_ADDR = 0x29,
        CTRL_INTB_ENABLE_ADDR = 0x2A,
        CTRL_FLAGS_ADDR = 0x2B,
        //RAM byte
        CTRL_RAM_BYTE_ADDR = 0x2C,
        //WatchDog registers
        CTRL_WATCHDOG_ADDR = 0x2D,
        //Stop
        CTRL_STOP_ADDR = 0x2E,
        //Reset
        CTRL_RESET_ADDR = 0x2F
    }
}
