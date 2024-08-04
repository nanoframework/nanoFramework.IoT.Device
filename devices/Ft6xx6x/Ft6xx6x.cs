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
        private byte[] _toReadPoint = new byte[6];

        private readonly int _touchAreaWidth;
        private readonly int _touchAreaHeight;

        /// <summary>
        /// Ft6xx6x I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x38;

        /// <summary>
        /// Gets or sets the touch area's coordinates rotation.
        /// </summary>
        /// <remarks>Applying a rotation requires the touch area's width and height to be known, use the appropriate constructor - <see cref="Ft6xx6x(I2cDevice, int, int, Rotation)"/>.</remarks>
        public Rotation Rotation { get; set; } = Rotation.None;

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
        /// Gets or sets the touch screen to go to monitor mode.
        /// </summary>
        public bool MonitorModeEnabled
        {
            get => ReadByte(Register.ID_G_CTRL) == 0x01;
            set => WriteByte(Register.ID_G_CTRL, (byte)(value ? 0x01 : 0x00));
        }

        /// <summary>
        /// Gets or sets the time to go to monitor mode in seconds.
        /// Maximum is 0x64.
        /// </summary>
        public byte MonitorModeDelaySeconds
        {
            get => ReadByte(Register.ID_G_TIMEENTERMONITOR);
            set => WriteByte(Register.ID_G_TIMEENTERMONITOR, (byte)(value > 0x64 ? 0x64 : value));
        }

        /// <summary>
        /// Period for scaning and making results available.
        /// Values between 0x04 and 0x14.
        /// </summary>
        /// <remarks>Do not pull results faster than this perdiod.</remarks>
        public byte PeriodActive
        {
            get => ReadByte(Register.ID_G_PERIODACTIVE);
            set => WriteByte(Register.ID_G_PERIODACTIVE, (byte)(value > 0x14 ? 0x14 : (value < 0x04 ? 0x04 : value)));
        }

        /// <summary>
        /// Gets or sets the period for scaning and making results available.
        /// Values between 0x04 and 0x14.
        /// </summary>
        /// <remarks>Do not pull results faster than this perdiod.</remarks>
        public byte MonitorModePeriodActive
        {
            get => ReadByte(Register.ID_G_PERIODMONITOR);
            set => WriteByte(Register.ID_G_PERIODMONITOR, (byte)(value > 0x14 ? 0x14 : (value < 0x04 ? 0x04 : value)));
        }

        /// <summary>
        /// Gets or setes the touch threshold
        /// </summary>
        public byte TouchThreshold
        {
            get => ReadByte(Register.ID_G_THGROUP);
            set => WriteByte(Register.ID_G_THGROUP, value);
        }

        /// <summary>
        /// Gets or setes the point filter threshold
        /// </summary>
        public byte PointFilterThreshold
        {
            get => ReadByte(Register.ID_G_THDIFF);
            set => WriteByte(Register.ID_G_THDIFF, value);
        }

        /// <summary>
        /// Gets or sets the gesture.
        /// </summary>
        public bool GestureEnabled
        {
            get => ReadByte(Register.ID_G_SPEC_GESTURE_ENABLE) == 0x01;
            set => WriteByte(Register.ID_G_SPEC_GESTURE_ENABLE, (byte)(value ? 0x01 : 0x00));
        }

        /// <summary>
        /// Creates a new instance of the Ft6xx6x
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ft6xx6x(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentException(nameof(i2cDevice));
            // Check if we do have a valid chip code high. Must be 0x64
            if (ReadByte(Register.ID_G_CIPHER_HIGH) != 0x64)
            {
                throw new IOException("Not a valid Ft6xx6x");
            }

            // Switch to normal mode
            WriteByte(Register.Mode_Switch, (byte)workingMode.Normal);
        }

        /// <summary>
        /// Creates a new instance of the Ft6xx6x with defined touch area width and height, which need to be known to support rotation (<see cref="Rotation"/>).
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        /// <param name="width">The touch area's width, in pixels, when in its default orientation.</param>
        /// <param name="height">The touch area's height, in pixels, when in its default orientation.</param>
        /// <param name="rotation">Rotate the touch area, which affects the X and Y coordinates returned by <see cref="GetPoint(bool)"/> and <see cref="GetDoublePoints"/>.</param>
        public Ft6xx6x(I2cDevice i2cDevice, int width, int height, Rotation rotation) : this(i2cDevice)
        {
            _touchAreaWidth = width;
            _touchAreaHeight = height;
            Rotation = rotation;
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
        /// <exception cref="NotImplementedException">An unexpected <see cref="Rotation"/> value was encountered.</exception>
        /// <exception cref="InvalidOperationException"><see cref="Rotation"/> is other than <see cref="Rotation.None"/>, but the touch area's default width and height are not set.</exception>
        public Point GetPoint(bool first)
        {
            Point pt = new Point();
            Read(first ? Register.P1_XH : Register.P2_XH, _toReadPoint);
            pt.TouchId = (byte)(_toReadPoint[2] >> 4);
            pt.Weigth = _toReadPoint[4];
            pt.Miscelaneous = _toReadPoint[5];
            pt.Event = (Event)(_toReadPoint[0] >> 6);

            int rawX = (_toReadPoint[0] << 8 | _toReadPoint[1]) & 0x0FFF;
            int rawY = (_toReadPoint[2] << 8 | _toReadPoint[3]) & 0x0FFF;

            if (Rotation == Rotation.None)
            {
                pt.X = rawX;
                pt.Y = rawY;
            }
            else if (_touchAreaHeight == default || _touchAreaWidth == default)
            {
                throw new InvalidOperationException();
            }
            else if (Rotation == Rotation.Left)
            {
                pt.X = _touchAreaHeight - rawY;
                pt.Y = rawX;
            }
            else if (Rotation == Rotation.Invert)
            {
                pt.X = _touchAreaWidth - rawX;
                pt.Y = _touchAreaHeight - rawY;
            }
            else if (Rotation == Rotation.Right)
            {
                pt.X = rawY;
                pt.Y = _touchAreaWidth - rawX;
            }
            else
            {
                throw new NotImplementedException(nameof(Rotation));
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

        private void Read(Register reg, SpanByte data)
        {
            _i2cDevice.WriteByte((byte)reg);
            _i2cDevice.Read(data);
        }
    }
}
