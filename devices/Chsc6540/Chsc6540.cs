// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;

namespace Iot.Device.Chsc6540
{
    /// <summary>
    /// Chsc6540 touch screen.
    /// </summary>
    public class Chsc6540 : IDisposable
    {
        private const int MaxX = 320;
        private const int MaxY = 280;

        private I2cDevice _i2cDevice;
        private byte[] _data = new byte[2];
        private byte[] _readBuffer;

        /// <summary>
        /// <see cref="Chsc6540"/> I2C address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x2E;

        /// <summary>
        /// Creates a new instance of the Chsc6540.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Chsc6540(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException();

            // create read buffer
            _readBuffer = new byte[11];
        }

        /// <summary>
        /// Sets the interrupt mode.
        /// </summary>
        /// <param name="modeLow">True to have int low when extending the report point otherwise when reporting point is not extended.</param>
        public void SetInterruptMode(bool modeLow) => WriteByte(Register.InteruptOn, (byte)(modeLow ? 0x05A : 0x00));

        /// <summary>
        /// Gets the number of points detected.
        /// </summary>
        /// <returns>Number of points detected.</returns>
        public byte GetNumberPoints() => ReadByte(Register.TD_STATUS);

        /// <summary>
        /// Read a <see cref="DoublePoints"/> from the controller.
        /// </summary>
        /// <returns>A <see cref="DoublePoints"/>.</returns>
        public DoublePoints GetDoublePoints()
        {
            DoublePoints pt = new();

            Read(Register.TD_STATUS, _readBuffer);

            //Point pt = new();

            if (_readBuffer[0] > 0 && _readBuffer[0] <= 2)
            {
                // touch reports 1 or 2 points so OK to process

                // according to M5Stack driver no point trying to read the "weight" and
                // "size" properties or using the built-in gestures as they are not working

                int tempX = ((_readBuffer[1] << 8)
                             | _readBuffer[2]) & 0x0fff;
                int tempY = ((_readBuffer[3] << 8)
                             | _readBuffer[4]) & 0x0fff;
     
                // sanity check 
                if(tempX >= MaxX || tempY >= MaxY)
                {
                    // invalid data, done here
                    return pt;
                }

                // fill point 1
                pt.Point1 = new Point()
                {
                    X = tempX,
                    Y = tempY
                };

                // check for touch left
                if ((_readBuffer[3] >> 4) > 0)
                {
                    pt.Point1.Event = Event.LiftUp;
                }
                else
                {
                    pt.Point1.Event = Event.PressDown;
                }

                if (_readBuffer[0] == 2)
                {
                    tempX = ((_readBuffer[7] << 8)
                             | _readBuffer[8]) & 0x0fff;
                    tempY = ((_readBuffer[9] << 8)
                             | _readBuffer[10]) & 0x0fff;

                    // sanity check 
                    if (tempX < MaxX && tempY < MaxY)
                    {
                        // valid data for point 2
                        pt.Point2 = new Point()
                        {
                            X = tempX,
                            Y = tempY,
                        };

                        // check for touch left
                        if ((_readBuffer[3] >> 4) > 0)
                        {
                            pt.Point2.Event = Event.LiftUp;
                        }
                    }
                }
            }

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

        private void Read(Register reg, Span<byte> data)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(data);
        }
    }
}
