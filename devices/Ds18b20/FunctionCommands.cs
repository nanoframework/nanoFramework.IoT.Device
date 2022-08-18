// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ds18b20
{
    /// <summary>
    /// Functions commands, see data sheet, page 11, section DS18B20 Function Commands.
    /// </summary>
    public enum FunctionCommands : byte
    {
        /// <summary>
        /// Recalls the alarm trigger values and configuration
        /// from EEPROM to scratchpad registers.
        /// </summary>
        RecallAlarmTriggerValues = 0xb8,

        /// <summary>
        /// Command to trigger a temperature conversion.
        /// </summary>
        ConvertTemperature = 0x44,

        /// <summary>
        /// Command copy scratchpad registers to EEPROM.
        /// </summary>
        CopyScratchpad = 0x48,

        /// <summary>
        /// Command to write to scratchpad registers.
        /// </summary>
        WriteScratchpad = 0x4e,

        /// <summary>
        /// Command to read scratchpad registers.
        /// </summary>
        ReadScratchpad = 0xbe,

        /// <summary>
        /// Check if any DS18B20s on the bus are using parasite power
        /// Return false for parasite power, true for external power.
        /// </summary>
        ReadPowerSupply = 0xb4,
    }
}
