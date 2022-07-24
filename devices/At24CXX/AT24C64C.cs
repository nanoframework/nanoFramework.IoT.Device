// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24CXX
{
    /// <summary>
    /// I2C EEPROM with 64Kbit (8192 bytes) of memory internally organized as 256 pages containing 32 bytes each.
    /// </summary>
    public sealed class AT24C64C : AT24Base
    {
        /// <summary>
        /// Initializes a new instance of an AT24C64C device.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public AT24C64C(I2cDevice i2cDevice)
            : base(i2cDevice, 32, 256)
        {

        }
    }
}
