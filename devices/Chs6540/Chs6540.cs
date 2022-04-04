// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Chs6540
{
    /// <summary>
    /// Chs6540 touch screen.
    /// </summary>
    public class Chs6540 : IDisposable
    {
        private I2cDevice _i2cDevice;
        private byte[] _data = new byte[2];
        private byte[] _toReadPoint = new byte[6];

        /// <summary>
        /// Chs6540 I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x2E;

        /// <summary>
        /// Creates a new instance of the Chs6540.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Chs6540(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException();        
        }

        /// <summary>
        /// Sets the interrupt mode.
        /// </summary>
        /// <param name="modeLow">True to have int low when extending the report point otherwise when reporting poirt is not etended.</param>
        public void SetInterruptMode(bool modeLow)
        {
            WriteByte(Register.InteruptOn, (byte)(modeLow ? 0x05A : 0x00));
        }

        /// <summary>
        /// Gets the number of points detected.
        /// </summary>
        /// <returns>Number of points detected.</returns>
        public byte GetNumberPoints()
        {
            return ReadByte(Register.TD_STATUS);
        }

        /// <summary>
        /// Gets a point.
        /// </summary>
        /// <param name="first">True to get the first point.</param>
        /// <returns>The point.</returns>
        public Point GetPoint(bool first)
        {
            Point pt = new Point();
            Read(first ? Register.P1_XH : Register.P2_XH, _toReadPoint);
            pt.X = (_toReadPoint[0] << 8 | _toReadPoint[1]) & 0x0FFF;
            pt.Y = (_toReadPoint[2] << 8 | _toReadPoint[3]) & 0x0FFF;
            pt.TouchId = (byte)(_toReadPoint[2] >> 4);
            pt.Weigth = _toReadPoint[4];
            pt.Miscelaneous = _toReadPoint[5];
            pt.Event = (Event)(_toReadPoint[0] >> 6);
            return pt;
        }

        /// <summary>
        /// Get the double touch points.
        /// </summary>
        /// <returns>Double touch points.</returns>
        public DoublePoints GetDoublePoints()
        {
            DoublePoints pt = new DoublePoints();
            pt.Point1 = GetPoint(true);
            pt.Point2 = GetPoint(false);
            return pt;
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        public void Dispose()
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;
        }

        private byte ReadByte(Register reg)
        {
            _i2cDevice.WriteByte((byte)reg);
            return _i2cDevice.ReadByte();
        }

        private void WriteByte(Register reg, byte data)
        {
            _data[0] = (byte)reg;
            _data[1] = data;
            _i2cDevice.Write(_data);
        }

        private void Read(Register reg, SpanByte data)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(data);
        }
    }
}
