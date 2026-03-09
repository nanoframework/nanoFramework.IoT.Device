// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Battery-Backed I2C Real-Time Clock/Calendar with SRAM.
    /// </summary>
    public partial class Mcp7940n
    {
        /// <summary>
        /// Represents a power event on a Mcp7940xx series device.
        /// </summary>
        public struct PowerEvent
        {
            /// <summary>
            /// Gets the minute the event occurred on.
            /// </summary>
            public byte Minute { get; private set; }

            /// <summary>
            /// Gets the hour the event occurred on.
            /// </summary>
            public byte Hour { get; private set; }

            /// <summary>
            /// Gets the day-of-the-week the event occurred on.
            /// </summary>
            public DayOfWeek DayOfWeek { get; private set; }

            /// <summary>
            /// Gets the day-of-the-month the event occurred on.
            /// </summary>
            public byte Day { get; private set; }

            /// <summary>
            /// Gets the month the event occurred on.
            /// </summary>
            public byte Month { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="PowerEvent" /> structure.
            /// </summary>
            /// <param name="i2cDevice">The I2C device to use for communication.</param>
            /// <param name="eventRegister">The minute register of the power event to be read.</param>
            /// <remarks>
            /// Parameter <paramref name="eventRegister"/> must only be set to <see cref="Register.PowerUpMinute" /> or <see cref="Register.PowerDownMinute" />.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="eventRegister"/> is not <see cref="Register.PowerUpMinute" /> or <see cref="Register.PowerDownMinute" />.</exception>
            internal PowerEvent(I2cDevice i2cDevice, Register eventRegister)
            {
                if (i2cDevice == null)
                {
                    throw new ArgumentNullException();
                }

                if (eventRegister != Register.PowerUpMinute && eventRegister != Register.PowerDownMinute)
                {
                    throw new ArgumentException();
                }

                // Read minute, hour, day, and month registers for the power down event.
                Span<byte> readBuffer = new byte[4];

                i2cDevice.WriteByte((byte)eventRegister);
                i2cDevice.Read(readBuffer);

                Minute = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[0] & (byte)RegisterMask.PowerEventMinuteMask));
                Hour = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[1] & (byte)RegisterMask.PowerEventHourMask));
                Day = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[2] & (byte)RegisterMask.PowerEventDayMask));
                DayOfWeek = (DayOfWeek)((readBuffer[3] & (byte)RegisterMask.PowerEventWeekdayMask) >> 5);
                Month = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[3] & (byte)RegisterMask.PowerEventMonthMask));
            }
        }
    }
}
