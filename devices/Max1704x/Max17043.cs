// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;

namespace Iot.Device.Max1704x
{
    /// <summary>
    /// Represents a MAX17043 gauge which extends from Max1704X.
    /// </summary>
    public class Max17043 : Max1704X
    {
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float FullScale => 5.12f;
        
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        protected override float Divider => 4096.0f;

        /// <summary>
        /// Initializes a new instance of the Max17043 class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication with device.</param>
        public Max17043(I2cDevice i2CDevice) : base(i2CDevice)
        {
        }
    }
}