// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Sdc4x
{
    /// <summary>
    /// Humidity and Temperature Sensor Sdc4x.
    /// </summary>
    public class Sdc4x : IDisposable
    {
        /// <summary>
        /// Default I2C address of the Sdc4x sensor.
        /// </summary>
        public const byte I2cDefaultAddress = 0x44;

        /// <summary>
        /// Represents an I2C device used for communication with the Sdc4x sensor.
        /// </summary>
        private I2cDevice _i2CDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Sdc4x" /> class.
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication.</param>
        public Sdc4x(I2cDevice i2CDevice)
        {
            _i2CDevice = i2CDevice ?? throw new ArgumentNullException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2CDevice?.Dispose();
            _i2CDevice = null;
        }
    }
}
