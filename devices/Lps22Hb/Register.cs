// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Lps22Hb
{
    internal enum Register : byte
    {
        /// <summary>
        /// WHO_AM_I register.
        /// </summary>
        WhoAmI = 0x0F,

        /// <summary>
        /// CTRL_REG1 register.
        /// </summary>
        ControlRegister1 = 0x10,

        /// <summary>
        /// CTRL_REG2 register.
        /// </summary>
        ControlRegister2 = 0x11,

        /// <summary>
        /// FIFO_CTRL register.
        /// </summary>
        FifoControl = 0x14,

        /// <summary>
        /// PRESS_OUT_XL register.
        /// </summary>
        PressureOutXl = 0x28,

        /// <summary>
        /// PRESS_OUT_L register.
        /// </summary>
        PressureOutL = 0x29,

        /// <summary>
        /// PRESS_OUT_H register.
        /// </summary>
        PressureOutH = 0x2A,

        /// <summary>
        /// TEMP_OUT_L register.
        /// </summary>
        TemperatureOutL = 0x2B,

        /// <summary>
        /// TEMP_OUT_H register.
        /// </summary>
        TemperatureOutH = 0x2C
    }
}
