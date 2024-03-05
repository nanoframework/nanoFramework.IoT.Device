// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Driver for RTC PCF85263 with key functions implemented for time.
    /// </summary>
    /// <remarks>
    /// See datasheet for details: https://www.nxp.com/docs/en/data-sheet/PCF85263A.pdf.
    /// </remarks>
    public class Pcf85263 : RtcBase
    {
        /// <summary>
        /// Pcf8563 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x51;
        private readonly I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Pcf85263"/> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Pcf85263(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Read time from the Pcf85263.
        /// </summary>
        /// <returns>Time from the device.</returns>
        protected override DateTime ReadTime()
        {
            // Sec, Min, Hour, Date, Day, Month & Century, Year
            SpanByte readBuffer = new byte[7];

            _i2cDevice.WriteByte((byte)Rtc.PCF85263RtcRegisters.RTC_SECOND_ADDR);
            _i2cDevice.Read(readBuffer);

            return new DateTime(
                ((1900 + (readBuffer[5] >> 7)) * 100) + NumberHelper.Bcd2Dec(readBuffer[6]),
                NumberHelper.Bcd2Dec((byte)(readBuffer[5] & 0b0001_1111)),
                NumberHelper.Bcd2Dec((byte)(readBuffer[3] & 0b0011_1111)),
                NumberHelper.Bcd2Dec((byte)(readBuffer[2] & 0b0011_1111)),
                NumberHelper.Bcd2Dec((byte)(readBuffer[1] & 0b0111_1111)),
                NumberHelper.Bcd2Dec((byte)(readBuffer[0] & 0b0111_1111)));
        }

        /// <summary>
        /// Set Pcf85263 Time.
        /// </summary>
        /// <param name="time">Time.</param>
        protected override void SetTime(DateTime time)
        {
            SpanByte writeBuffer = new byte[8];

            writeBuffer[0] = (byte)Rtc.PCF85263RtcRegisters.RTC_SECOND_ADDR;

            // Set bit8 as 0 to guarantee clock integrity
            writeBuffer[1] = (byte)(NumberHelper.Dec2Bcd(time.Second) & 0b0111_1111);
            writeBuffer[2] = NumberHelper.Dec2Bcd(time.Minute);
            writeBuffer[3] = NumberHelper.Dec2Bcd(time.Hour);
            writeBuffer[4] = NumberHelper.Dec2Bcd(time.Day);
            writeBuffer[5] = NumberHelper.Dec2Bcd((int)time.DayOfWeek);
            if (time.Year >= 2000)
            {
                writeBuffer[6] = (byte)(NumberHelper.Dec2Bcd(time.Month) | 0b1000_0000);
                writeBuffer[7] = NumberHelper.Dec2Bcd(time.Year - 2000);
            }
            else
            {
                writeBuffer[6] = NumberHelper.Dec2Bcd(time.Month);
                writeBuffer[7] = NumberHelper.Dec2Bcd(time.Year - 1900);
            }

            _i2cDevice.Write(writeBuffer);
        }

        /// <summary>
        /// Cleanup.
        /// </summary>
        /// <param name="disposing">Is disposing.</param>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();

            base.Dispose(disposing);
        }

        /// <summary>
        /// Set alarm for seconds/min/hour/day/month, a full calandar like alarm.
        /// </summary>
        /// <remarks>
        /// Support for this is not yet available.
        /// </remarks>
        /// <param name="alarmDateTime">Date and time to set the alarm, must be in the future from present RTC time.</param>
        /// <param name="alarm">The alarm to set.</param>
        /// <returns>True if alarm set, False if the alarm date is in the past from the RTC time.</returns>
        public bool SetAlarm(DateTime alarmDateTime, int alarm)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the alarm if any, for month, day, hour, min seconds. The year part is just picked to be current year but
        /// alarm will fire every time for that month and the date/time.
        /// </summary>
        /// <remarks>
        /// Support for this is not yet available.
        /// </remarks>
        /// <param name="alarm">The alarm to set.</param>
        /// <returns>Date and time of alarm if set, or DateTime.MinValue if alarm is not set.</returns>
        public DateTime GetAlarm(int alarm)
        {
            throw new NotImplementedException();
        }
    }
}
