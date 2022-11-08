// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using Iot.Device.Common;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Realtime Clock DS1307.
    /// </summary>
    public class Ds1307 : RtcBase
    {
        /// <summary>
        /// DS1307 Default I2C Address.
        /// </summary>
        public const byte DefaultI2cAddress = 0x68;

        private I2cDevice _i2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds1307" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Ds1307(I2cDevice i2cDevice)
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));
        }

        /// <summary>
        /// Read Time from DS1307.
        /// </summary>
        /// <returns>DS1307 Time.</returns>
        protected override DateTime ReadTime()
        {
            SpanByte readBuffer = new byte[7];

            // Read all registers at the same time
            _i2cDevice.WriteByte((byte)Ds1307Register.RTC_SEC_REG_ADDR);
            _i2cDevice.Read(readBuffer);

            // Details in the Datasheet P8
            return new DateTime(
                2000 + 
                NumberHelper.Bcd2Dec(readBuffer[6]),
                NumberHelper.Bcd2Dec(readBuffer[5]),
                NumberHelper.Bcd2Dec(readBuffer[4]),
                NumberHelper.Bcd2Dec(readBuffer[2]),
                NumberHelper.Bcd2Dec(readBuffer[1]),
                NumberHelper.Bcd2Dec((byte)(readBuffer[0] & 0b0111_1111)));
        }

        /// <summary>
        /// Set DS1307 Time.
        /// </summary>
        /// <param name="time">Time.</param>
        protected override void SetTime(DateTime time)
        {
            SpanByte writeBuffer = new byte[8];

            writeBuffer[0] = (byte)Ds1307Register.RTC_SEC_REG_ADDR;

            // Details in the Datasheet P8
            // | bit 7: CH | bit 6-0: sec |
            writeBuffer[1] = (byte)(NumberHelper.Dec2Bcd(time.Second) & 0b0111_1111);
            writeBuffer[2] = NumberHelper.Dec2Bcd(time.Minute);

            // | bit 7: 0 | bit 6: 12/24 hour | bit 5-0: hour |
            writeBuffer[3] = (byte)(NumberHelper.Dec2Bcd(time.Hour) & 0b0011_1111);
            writeBuffer[4] = NumberHelper.Dec2Bcd((int)time.DayOfWeek + 1);
            writeBuffer[5] = NumberHelper.Dec2Bcd(time.Day);
            writeBuffer[6] = NumberHelper.Dec2Bcd(time.Month);
            writeBuffer[7] = NumberHelper.Dec2Bcd(time.Year - 2000);

            _i2cDevice.Write(writeBuffer);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null;

            base.Dispose(disposing);
        }
    }
}
