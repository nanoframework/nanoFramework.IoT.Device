// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24CXX
{
    /// <summary>
    /// I2C EEPROM with 1Kbit (128 bytes) of memory internally organized as 16 pages containing 8 bytes each.
    /// </summary>
    public sealed class AT24C01C : AT24Base
    {
        /// <summary>
        /// Initializes a new instance of an AT24C01C device.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        public AT24C01C(I2cDevice i2cDevice)
                : base(i2cDevice, 8, 16)
        {

        }
    }
}
