﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24Cxx
{
    /// <summary>
    /// I2C EEPROM with 32Kbit (4096 bytes) of memory internally organized as 128 pages containing 32 bytes each.
    /// </summary>
    public sealed class At24C32C : At24Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="At24C32C" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        public At24C32C(I2cDevice i2cDevice)
            : base(i2cDevice, 32, 128)
        {
        }
    }
}
