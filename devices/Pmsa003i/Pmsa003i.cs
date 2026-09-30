// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Pmsx003.Shared;

namespace Iot.Device.Pmsa003i
{
    /// <summary>
    /// Plantower PMSA003I particulate-matter sensor using its I2C interface.
    /// </summary>
    public sealed class Pmsa003i : IDisposable
    {
        /// <summary>The default I2C address.</summary>
        public const byte DefaultI2cAddress = 0x12;

        private readonly bool _shouldDispose;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pmsa003i"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device.</param>
        /// <param name="shouldDispose">True to dispose the I2C device when this instance is disposed.</param>
        public Pmsa003i(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Reads and validates one sensor measurement.
        /// </summary>
        /// <returns>The particulate-matter measurement.</returns>
        public PmsReading Read()
        {
            if (_i2cDevice == null)
            {
                throw new ObjectDisposedException(nameof(Pmsa003i));
            }

            byte[] frame = new byte[Pmsx003Parser.FrameLength];
            _i2cDevice.Read(frame);
            return new PmsReading(Pmsx003Parser.Parse(frame));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_shouldDispose && _i2cDevice != null)
            {
                _i2cDevice.Dispose();
            }

            _i2cDevice = null;
        }
    }
}