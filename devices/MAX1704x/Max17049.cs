// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// Represents the MAX17049 class.
    /// </summary>
    public class Max17049 : Max17048
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float FullScale => 10.24f;
        
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float Divider => 65536.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max17049" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device object representing the MAX17049.</param>
        public Max17049(I2cDevice i2CDevice) : base(i2CDevice)
        {
        }
    }
}