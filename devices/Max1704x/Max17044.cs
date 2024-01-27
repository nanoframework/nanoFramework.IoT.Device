// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// Represents a Max17044 battery fuel gauge.
    /// </summary>
    public class Max17044 : Max17043
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float FullScale => 10.24f;
        
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float Divider => 4096.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Max17044" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used to communicate with the Max17044.</param>
        public Max17044(I2cDevice i2CDevice) : base(i2CDevice)
        {
        }
    }
}