// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Seesaw
{
    /// <summary>
    /// Seesaw function.
    /// </summary>
    public enum SeesawFunction : byte
    {
        // status functions

        /// <summary>Hardware identifier status.</summary>
        StatusHwId = 0x01,

        /// <summary>Version status.</summary>
        StatusVersion = 0x02,

        /// <summary>Options status.</summary>
        StatusOptions = 0x03,

        /// <summary>Temperature status.</summary>
        StatusTemp = 0x04,

        /// <summary>Software reset status.</summary>
        StatusSwrst = 0x7F,

        // GPIO functions

        /// <summary>Set direction for bulk GPIO operation.</summary>
        GpioDirsetBulk = 0x02,

        /// <summary>Clear direction for bulk GPIO operation.</summary>
        GpioDirclrBulk = 0x03,

        /// <summary>Bulk set GPIO.</summary>
        GpioBulk = 0x04,

        /// <summary>Bulk set GPIO.</summary>
        GpioBulkSet = 0x05,

        /// <summary>Bulk clear GPIO.</summary>
        GpioBulkClr = 0x06,

        /// <summary>Bulk toggle GPIO.</summary>
        GpioBulkToggle = 0x07,

        /// <summary>Enable interrupt.</summary>
        GpioIntenset = 0x08,

        /// <summary>Clear interrupt.</summary>
        GpioIntenclr = 0x09,

        /// <summary>Interrupt status.</summary>
        GpioIntflag = 0x0A,

        /// <summary>Sets pull up or down for pins.</summary>
        GpioPullenset = 0x0B,

        /// <summary>Clears pull up or down for pins.</summary>
        GpioPullenclr = 0x0C,

        // ADC Functions

        /// <summary>ADC status.</summary>
        AdcStatus = 0x00,

        /// <summary>Set ADC interrupt enable.</summary>
        AdcInten = 0x02,

        /// <summary>Clear ADC interrupt enable.</summary>
        AdcIntenclr = 0x03,

        /// <summary>ADC window mode.</summary>
        AdcWinmode = 0x04,

        /// <summary>ADC window threshold.</summary>
        AdcWinthresh = 0x05,

        /// <summary>ADC channel offset.</summary>
        AdcChannelOffset = 0x07,

        // EEProm Addresses

        /// <summary>EEPROM I2C address.</summary>
        EepromI2cAddr = 0x3F,

        // touch functions

        /// <summary>Touch channel offset.</summary>
        TouchChannelOffset = 0x10,
    }
}
