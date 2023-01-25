// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.At24cxx
{
    /// <summary>
    /// I2C EEPROM with 128Kbit (16384 bytes) of memory internally organized as 256 pages containing 64 bytes each.
    /// </summary>
    public sealed class At24c128 : At24Base
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="At24c128" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        public At24c128(I2cDevice i2cDevice)
            : base(i2cDevice, pageSize: 64, pageCount: 256)
        {
        }
    }
}
