// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Magnetometer
{
    /// <summary>
    /// Abstract class for AK8963 the I2C interface. This sensor can be found as a sub
    /// I2C sensor like in the MPU9250. The access is done thru another I2C device and the
    /// core I2C primitive are different. Use those 3 primitive to define the access to read
    /// and write bytes to the AK8963.
    /// </summary>
    public abstract class Ak8963I2cBase
    {
        /// <summary>
        /// Write a register of the AK8963.
        /// </summary>
        /// <param name="i2CDevice">I2C device.</param>
        /// <param name="reg">The register to write.</param>
        /// <param name="data">The data byte to write.</param>
        public abstract void WriteRegister(I2cDevice i2CDevice, byte reg, byte data);

        /// <summary>
        /// Read a byte on a specific register.
        /// </summary>
        /// <param name="i2CDevice">I2C device.</param>
        /// <param name="reg">The register to read.</param>
        /// <returns>Byte from specific register.</returns>
        public abstract byte ReadByte(I2cDevice i2CDevice, byte reg);

        /// <summary>
        /// Read bytes on a specific AK8963 register.
        /// </summary>
        /// <param name="i2CDevice">I2C device.</param>
        /// <param name="reg">The register to read.</param>
        /// <param name="readBytes">Span of byte to store the data read.</param>
        public abstract void ReadBytes(I2cDevice i2CDevice, byte reg, SpanByte readBytes);
    }
}
