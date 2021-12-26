// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.IO;

namespace Iot.Device.Ft6xx6x
{
    /// <summary>
    /// Ft6xx6x touch screen
    /// </summary>
    public class Ft6xx6x : IDisposable
    {
        private I2cDevice _i2cDevice;
        private byte[] _data = new byte[2];

        /// <summary>
        /// Ft6xx6x I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x38;

        /// <summary>
        /// Gets or sets the consumption mode.
        /// </summary>
        public ConsumptionMode ConsumptionMode
        {
            get => (ConsumptionMode)ReadByte(Register.ID_G_PMODE);
            set => WriteByte(Register.ID_G_PMODE, (byte)value);
        }

        /// <summary>
        /// Gets or sets the proximity sensing.
        /// </summary>
        public bool ProximitySensingEnabled
        {
            get => ReadByte(Register.ID_G_FACE_DEC_MODE) == 0x01;
            set => WriteByte(Register.ID_G_FACE_DEC_MODE, (byte)(value ? 0x01 : 0x00));
        }

        /// <summary>
        /// Gets or sets the charger mode type.
        /// </summary>
        public bool ChargerOn
        {
            get => ReadByte(Register.ID_G_FREQ_HOPPING_EN) == 0x01;
            set => WriteByte(Register.ID_G_FREQ_HOPPING_EN, (byte)(value ? 0x01 : 0x00));
        }

        /// <summary>
        /// Creates a new instance of the Ft6xx6x
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ft6xx6x(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException(nameof(i2cDevice));
            // Check if we do have a valid chip code high. Must be 0x64
            //if (ReadByte(Register.ID_G_CIPHER_HIGH) != 0x64)
            //{
            //    throw new IOException("Not a valid Ft6xx6x");
            //}

            // Switch to normal mode
            WriteByte(Register.Mode_Switch, (byte)workingMode.Normal);
        }

        /// <summary>
        /// Gets the library version.
        /// </summary>
        /// <returns>The library version.</returns>
        public Version GetVersion()
        {
            var low = ReadByte(Register.ID_G_LIB_VERSION_L);
            var high = ReadByte(Register.ID_G_LIB_VERSION_H); ;
            return new Version(high, low);
        }

        /// <summary>
        /// Sets the interrupt mode.
        /// </summary>
        /// <param name="modeLow">True to have int low when extending the report point otherwise when reporting poirt is not etended.</param>
        public void SetInterruptMode(bool modeLow)
        {
            WriteByte(Register.ID_G_MODE, (byte)(modeLow ? 0x01 : 0x00));
        }

        /// <summary>
        /// Gets the chip type.
        /// </summary>
        /// <returns>The chip type.</returns>
        public ChipType GetChipType()
        {
            return (ChipType)ReadByte(Register.ID_G_CIPHER_LOW);
        }

        /// <summary>
        /// Sets the factory mode.
        /// </summary>
        /// <param name="mode">The factory mode.</param>
        public void SetFactoryMode(FactoryMode mode)
        {
            WriteByte(Register.Mode_Switch, (byte)workingMode.Factory);
            WriteByte(Register.ID_G_FACTORY_MODE, (byte)mode);
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
            if (first)
            {
                byte touchId = ReadByte(Register.P1_YH);
                pt.X = (ReadByte(Register.P1_XH) << 8 | ReadByte(Register.P1_XL)) & 0x0FFF;
                pt.Y = (touchId << 8 | ReadByte(Register.P1_YL)) & 0x0FFF;
                pt.TouchId = (byte)(touchId >> 4);
                pt.Weigth = ReadByte(Register.P1_WEIGHT);
                pt.Miscelaneous = ReadByte(Register.P1_MISC);
            }
            else
            {
                byte touchId = ReadByte(Register.P2_YH);
                pt.X = (ReadByte(Register.P2_XH) << 8 | ReadByte(Register.P2_XL)) & 0x0FFF;
                pt.Y = (touchId << 8 | ReadByte(Register.P2_YL)) & 0x0FFF;
                pt.TouchId = (byte)(touchId >> 4);
                pt.Weigth = ReadByte(Register.P2_WEIGHT);
                pt.Miscelaneous = ReadByte(Register.P2_MISC);

            }

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
        /// Cleanup
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
    }
}
