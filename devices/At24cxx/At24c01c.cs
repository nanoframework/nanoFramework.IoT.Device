﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24cxx
{
    /// <summary>
    /// I2C EEPROM with 1Kbit (128 bytes) of memory internally organized as 16 pages containing 8 bytes each.
    /// </summary>
    public sealed class At24c01c : At24Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="At24c01c" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        public At24c01c(I2cDevice i2cDevice)
            : base(i2cDevice, 8, 16)
        {
        }
    }
}
