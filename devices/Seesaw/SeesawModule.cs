// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// Seesaw module.
    /// </summary>
    public enum SeesawModule : byte
    {
        /// <summary>Status module.</summary>
        Status = 0x00,

        /// <summary>GPIO module.</summary>
        Gpio = 0x01,

        /// <summary>Serial communication 0 module.</summary>
        Sercom0 = 0x02,

        /// <summary>Timer module.</summary>
        Timer = 0x08,

        /// <summary>ADC module.</summary>
        Adc = 0x09,

        /// <summary>DAC module.</summary>
        Dac = 0x0A,

        /// <summary>Interrupt module.</summary>
        Interrupt = 0x0B,

        /// <summary>DAP module.</summary>
        Dap = 0x0C,

        /// <summary>EEPROM module.</summary>
        Eeprom = 0x0D,

        /// <summary>Neopixel module.</summary>
        Neopixel = 0x0E,

        /// <summary>Touch module.</summary>
        Touch = 0x0F,

        /// <summary>Keypad module.</summary>
        Keypad = 0x10,

        /// <summary>Encoder module.</summary>
        Encoder = 0x11
    }   
}
