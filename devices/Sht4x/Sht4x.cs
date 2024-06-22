// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using UnitsNet;

namespace Iot.Device.Sht4x
{
    /// <summary>
    /// Humidity and Temperature Sensor Sht4x.
    /// </summary>
    public class Sht4x : IDisposable
    {
        /// <summary>
        /// Default I2C address of the Sht4x sensor.
        /// </summary>
        public const byte DefautAddess = 0xFF; // TODO: Valid address

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Gets Sht4x Temperature.
        /// </summary>
        public Temperature Temperature
        {
            get
            {
                return Temperature.FromDegreesCelsius(10);
            }
        }

        /// <summary>
        /// Gets Sht4x Relative Humidity (%).
        /// </summary>
        public RelativeHumidity Humidity
        {
            get
            {
                return RelativeHumidity.FromPercent(10);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sht4x" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Sht4x(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;
        }
    }
}
