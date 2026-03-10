// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Globalization;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// I2C Real-Time Clock/Calendar with SRAM.
    /// </summary>
    public partial class Mcp7940m
    {
        /// <summary>
        /// Represents an alarm on a Mcp7940xx series device.
        /// </summary>
        public class Alarm
        {
            private byte _second;
            private byte _minute;
            private byte _hour;
            private byte _day;
            private byte _month;

            /// <summary>
            /// Gets or sets the second that the alarm will be triggerd on.
            /// </summary>
            /// <remarks>
            /// The value is clamped to the range 0 through 59.
            /// </remarks>
            public byte Second
            {
                get
                {
                    return _second;
                }

                set
                {
                    _second = (byte)Math.Clamp(value, 0, 59);
                }
            }

            /// <summary>
            /// Gets or sets the minute that the alarm will be triggerd on.
            /// </summary>
            /// <remarks>
            /// The value is clamped to the range 0 through 59.
            /// </remarks>
            public byte Minute
            {
                get
                {
                    return _minute;
                }

                set
                {
                    _minute = (byte)Math.Clamp(value, 0, 59);
                }
            }

            /// <summary>
            /// Gets or sets the hour that the alarm will be triggerd on.
            /// </summary>
            /// <remarks>
            /// The value is clamped to the range 0 through 23.
            /// </remarks>
            public byte Hour
            {
                get
                {
                    return _hour;
                }

                set
                {
                    _hour = (byte)Math.Clamp(value, 0, 59);
                }
            }

            /// <summary>
            /// Gets or sets the day-of-the-week that the alarm will be triggerd on.
            /// </summary>
            public DayOfWeek DayOfWeek { get; set; }

            /// <summary>
            /// Gets or sets the day-of-the-month that the alarm will be triggerd on.
            /// </summary>
            /// <remarks>
            /// The value is clamped to the range 1 through 31.
            /// </remarks>
            public byte Day
            {
                get
                {
                    return _day;
                }

                set
                {
                    _day = (byte)Math.Clamp(value, 1, 31);
                }
            }

            /// <summary>
            /// Gets or sets the month that the alarm will be triggerd on.
            /// </summary>
            /// <remarks>
            /// The value is clamped to the range 1 through 12.
            /// </remarks>
            public byte Month
            {
                get
                {
                    return _month;
                }

                set
                {
                    _month = (byte)Math.Clamp(value, 1, 12);
                }
            }

            /// <summary>
            /// Gets or sets the mode used to determine when the alarm should trigger.
            /// </summary>
            public AlarmMatchMode MatchMode { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Alarm" /> class.
            /// </summary>
            /// <param name="matchMode">The mode used to determine when the alarm should trigger.</param>
            /// <param name="month">The month that the alarm will be triggerd on.</param>
            /// <param name="day">The day-of-the-month that the alarm will be triggerd on.</param>
            /// <param name="hour">The hour that the alarm will be triggerd on.</param>
            /// <param name="minute">The minute that the alarm will be triggerd on.</param>
            /// <param name="second">The second that the alarm will be triggerd on.</param>
            /// <param name="dayOfWeek">The day-of-the-week that the alarm will be triggerd on.</param>
            /// <remarks>
            /// Month, day, hour, minute, and second will be clamped to their respective valid ranges.
            /// </remarks>
            public Alarm(AlarmMatchMode matchMode, byte month = 1, byte day = 1, byte hour = 0, byte minute = 0, byte second = 0, DayOfWeek dayOfWeek = DayOfWeek.Sunday)
            {
                MatchMode = matchMode;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
                Second = second;
                DayOfWeek = dayOfWeek;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Alarm" /> class.
            /// </summary>
            /// <param name="i2cDevice">The I2C device to use for communication.</param>
            /// <param name="alarmRegister">The second register of the alarm to be read.</param>
            /// <remarks>
            /// Parameter <paramref name="alarmRegister"/> must only be set to <see cref="Register.Alarm1Second" /> or <see cref="Register.Alarm2Second" />.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="alarmRegister"/> is not <see cref="Register.Alarm1Second" /> or <see cref="Register.Alarm2Second" />.</exception>
            internal Alarm(I2cDevice i2cDevice, Register alarmRegister)
            {
                if (i2cDevice == null)
                {
                    throw new ArgumentNullException();
                }

                if (alarmRegister != Register.Alarm1Second && alarmRegister != Register.Alarm2Second)
                {
                    throw new ArgumentException();
                }

                // Read the second, minute, hour, day-of-week, day, and month registers for the desired alarm.
                Span<byte> readBuffer = new byte[6];

                i2cDevice.WriteByte((byte)alarmRegister);
                i2cDevice.Read(readBuffer);

                MatchMode = (AlarmMatchMode)(readBuffer[3] & (byte)RegisterMask.AlarmMatchModeMask);
                Second = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[0] & (byte)RegisterMask.AlarmSecondMask));
                Minute = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[1] & (byte)RegisterMask.AlarmMinuteMask));
                Hour = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[2] & (byte)RegisterMask.AlarmHourMask));
                DayOfWeek = (DayOfWeek)(readBuffer[3] & (byte)RegisterMask.AlarmDayOfWeekMask);
                Day = (byte)NumberHelper.Bcd2Dec((byte)(readBuffer[4] & (byte)RegisterMask.AlarmDayMask));
                Month = (byte)NumberHelper.Bcd2Dec(readBuffer[5]);
            }

            /// <summary>
            /// Internal helper to write an alarms settings to the device.
            /// </summary>
            /// <param name="i2cDevice">The I2C device to use for communication.</param>
            /// <param name="alarmRegister">The second register of the alarm to be read.</param>
            /// <remarks>
            /// Parameter <paramref name="alarmRegister"/> must only be set to <see cref="Register.Alarm1Second" /> or <see cref="Register.Alarm2Second" />.
            /// </remarks>
            /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
            /// <exception cref="ArgumentException">Thrown when <paramref name="alarmRegister"/> is not <see cref="Register.Alarm1Second" /> or <see cref="Register.Alarm2Second" />.</exception>
            internal void WriteToDevice(I2cDevice i2cDevice, Register alarmRegister)
            {
                if (i2cDevice == null)
                {
                    throw new ArgumentNullException();
                }

                if (alarmRegister != Register.Alarm1Second && alarmRegister != Register.Alarm2Second)
                {
                    throw new ArgumentException();
                }

                byte weekdayRegister = (byte)((byte)MatchMode | (byte)DayOfWeek);

                // The weekday register for the alarm also contains the alarm interrupt polarity.
                // This setting must be maintained while the rest of the register is updated.
                if (RegisterHelper.RegisterBitIsSet(i2cDevice, (byte)(alarmRegister + 3), (byte)RegisterMask.AlarmInterruptPolarityMask))
                {
                    weekdayRegister |= (byte)RegisterMask.AlarmInterruptPolarityMask;
                }

                Span<byte> writeBuffer = new byte[7];

                writeBuffer[0] = (byte)alarmRegister;
                writeBuffer[1] = NumberHelper.Dec2Bcd(Second);
                writeBuffer[2] = NumberHelper.Dec2Bcd(Minute);
                writeBuffer[3] = NumberHelper.Dec2Bcd(Hour);
                writeBuffer[4] = weekdayRegister;
                writeBuffer[5] = NumberHelper.Dec2Bcd(Day);
                writeBuffer[6] = NumberHelper.Dec2Bcd(Month);

                i2cDevice.Write(writeBuffer);
            }

            /// <inheritdoc />
            public override string ToString()
            {
                switch (MatchMode)
                {
                    case AlarmMatchMode.Second:

                        return $"Second : {Second}";

                    case AlarmMatchMode.Minute:

                        return $"Minute : {Minute}";

                    case AlarmMatchMode.Hour:

                        return $"Hour : {Hour}";

                    case AlarmMatchMode.DayOfWeek:

                        return $"Day Of Week : {DateTimeFormatInfo.CurrentInfo.DayNames[(int)DayOfWeek]}";

                    case AlarmMatchMode.Day:

                        return $"Day : {Day}";

                    case AlarmMatchMode.Full:

                        return $"Full : {Month}/{Day} {DateTimeFormatInfo.CurrentInfo.DayNames[(int)DayOfWeek]} {Hour}:{Minute}:{Second}";

                    default:

                        return "Unrecognized";
                }
            }
        }
    }
}
