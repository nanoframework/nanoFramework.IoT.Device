// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using nanoFramework.UI;

namespace Iot.Device.Cst816D
{
    /// <summary>
    /// CST816D single-point capacitive touch controller.
    /// </summary>
    public class Cst816D : IDisposable
    {
        /// <summary>
        /// The default I2C address of the controller.
        /// </summary>
        public const byte DefaultI2cAddress = 0x15;

        private readonly int _touchAreaWidth;
        private readonly int _touchAreaHeight;
        private readonly bool _shouldDispose;
        private readonly byte[] _registerBuffer = new byte[1];
        private readonly byte[] _pointBuffer = new byte[5];
        private readonly byte[] _byteBuffer = new byte[1];
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cst816D"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="shouldDispose">Whether to dispose the I2C device when disposing this instance.</param>
        /// <exception cref="ArgumentNullException">The I2C device is null.</exception>
        public Cst816D(I2cDevice i2cDevice, bool shouldDispose = true)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException();
            _shouldDispose = shouldDispose;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Cst816D"/> class with display orientation support.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="width">The touch area's width in pixels in portrait orientation (1 to 4096).</param>
        /// <param name="height">The touch area's height in pixels in portrait orientation (1 to 4096).</param>
        /// <param name="orientation">The orientation used for returned coordinates.</param>
        /// <param name="shouldDispose">Whether to dispose the I2C device when disposing this instance.</param>
        /// <exception cref="ArgumentNullException">The I2C device is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The width or height is outside the range 1 to 4096.</exception>
        public Cst816D(I2cDevice i2cDevice, int width, int height, DisplayOrientation orientation = DisplayOrientation.Portrait, bool shouldDispose = true)
            : this(i2cDevice, shouldDispose)
        {
            if (width <= 0 || width > 4096)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (height <= 0 || height > 4096)
            {
                throw new ArgumentOutOfRangeException();
            }

            _touchAreaWidth = width;
            _touchAreaHeight = height;
            Orientation = orientation;
        }

        /// <summary>
        /// Gets or sets the display orientation used for returned coordinates.
        /// </summary>
        /// <remarks>Non-portrait orientations require the constructor that specifies the touch area's dimensions.</remarks>
        public DisplayOrientation Orientation { get; set; } = DisplayOrientation.Portrait;

        /// <summary>
        /// Gets the chip identifier (0xB6 for the CST816D).
        /// </summary>
        /// <exception cref="ObjectDisposedException">The controller has been disposed.</exception>
        /// <exception cref="InvalidOperationException">The I2C read failed.</exception>
        public byte ChipId
        {
            get
            {
                Read(0xA7, _byteBuffer);
                return _byteBuffer[0];
            }
        }

        /// <summary>
        /// Reads a single coherent report containing the touch state, coordinates and event.
        /// </summary>
        /// <returns>The touch point transformed to <see cref="Orientation"/>, without scaling or clipping.</returns>
        /// <exception cref="InvalidOperationException">The I2C read failed, the controller reported more than one touch point, or a rotated orientation is requested without touch area dimensions.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The orientation is not supported.</exception>
        /// <exception cref="ObjectDisposedException">The controller has been disposed.</exception>
        public Point GetPoint()
        {
            Read(0x02, _pointBuffer);
            return new Point(_pointBuffer, _touchAreaWidth, _touchAreaHeight, Orientation);
        }

        /// <summary>
        /// Releases the resources used by the controller.
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _i2cDevice?.Dispose();
            }

            _i2cDevice = null;
        }

        private void Read(byte register, byte[] buffer)
        {
            if (_i2cDevice == null)
            {
                throw new ObjectDisposedException();
            }

            _registerBuffer[0] = register;
            I2cTransferResult result = _i2cDevice.WriteRead(_registerBuffer, buffer);
            if (result.Status != I2cTransferStatus.FullTransfer)
            {
                throw new InvalidOperationException();
            }
        }
    }
}
