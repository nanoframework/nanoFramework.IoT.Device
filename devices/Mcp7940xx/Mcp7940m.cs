// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Threading;
using Iot.Device.Common;

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// I2C Real-Time Clock/Calendar with SRAM.
    /// </summary>
    public partial class Mcp7940m : IDisposable
    {
        /// <summary>
        /// Lower address of SRAM memory region.
        /// </summary>
        private const byte LowerAddressBoundSRAM = 0x20;

        /// <summary>
        /// Upper address of SRAM memory region.
        /// </summary>
        private const byte UpperAddressBoundSRAM = 0x5F;

        /// <summary>
        /// Default I2C address for the Mcp7940xx family.
        /// </summary>
        public const byte DefaultI2cAddress = 0x6F;

        /// <summary>
        /// The underlying I2C device used for communication.
        /// </summary>
        protected readonly I2cDevice _I2cDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mcp7940m" /> class.
        /// </summary>
        /// <param name="i2cDevice">The I2C device to use for communication.</param>
        /// <param name="clockSource">The oscillator configuration of the clock.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="i2cDevice"/> is <c>null</c>.</exception>
        public Mcp7940m(I2cDevice i2cDevice, ClockSource clockSource)
        {
            if (i2cDevice == null)
            {
                throw new ArgumentNullException(nameof(i2cDevice));
            }

            _I2cDevice = i2cDevice;

            // Set the clock to 24 hour mode.
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.TimekeepingHour, (byte)TimekeepingHourRegister.TimeFormat);

            // Set clock source.
            if (clockSource == ClockSource.ExternalClockInput)
            {
                RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.ExternalClockInput);
            }
            else
            {
                RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.ExternalClockInput);
            }
        }

        #region Time

        /// <summary>
        /// Gets a <see cref = "DateTime" /> object that is set to the current date and time of the device.
        /// </summary>
        /// <returns>A <see cref = "DateTime" /> object whose value is the current device date and time.</returns>
        public DateTime GetTime()
        {
            // Read second, minute, hour, day-of-week, day, month and year registers.
            SpanByte readBuffer = new byte[7];

            _I2cDevice.WriteByte((byte)Register.TimekeepingSecond);
            _I2cDevice.Read(readBuffer);

            int second = NumberHelper.Bcd2Dec((byte)(readBuffer[0] & (byte)TimekeepingSecondRegister.SecondMask));
            int minute = NumberHelper.Bcd2Dec((byte)(readBuffer[1] & (byte)TimekeepingMinuteRegister.MinuteMask));
            int hour = NumberHelper.Bcd2Dec((byte)(readBuffer[2] & (byte)TimekeepingHourRegister.HourMask));
            int day = NumberHelper.Bcd2Dec((byte)(readBuffer[4] & (byte)TimekeepingDayRegister.DayMask));
            int month = NumberHelper.Bcd2Dec((byte)(readBuffer[5] & (byte)TimekeepingMonthRegister.MonthMask));
            int year = NumberHelper.Bcd2Dec(readBuffer[6]) + 2000;

            return new DateTime(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Sets the time.
        /// </summary>
        /// <param name="time">The new time.</param>
        public void SetTime(DateTime time)
        {
            bool clockIsRunning = !IsHalted();

            if (clockIsRunning)
            {
                // To avoid rollover issues when loading new time and date values, the clocks oscillator must be disabled if it is currently running.
                Halt(true);
            }

            SpanByte writeBuffer = new byte[8];

            writeBuffer[0] = (byte)Register.TimekeepingSecond;
            writeBuffer[1] = NumberHelper.Dec2Bcd(time.Second);
            writeBuffer[2] = NumberHelper.Dec2Bcd(time.Minute);
            writeBuffer[3] = NumberHelper.Dec2Bcd(time.Hour);
            writeBuffer[4] = NumberHelper.Dec2Bcd((int)time.DayOfWeek);
            writeBuffer[5] = NumberHelper.Dec2Bcd(time.Day);
            writeBuffer[6] = NumberHelper.Dec2Bcd(time.Month);
            writeBuffer[7] = NumberHelper.Dec2Bcd(time.Year % 100);

            _I2cDevice.Write(writeBuffer);

            // Re-enable the clocks oscillator if it was running at the start of this function.
            if (clockIsRunning)
            {
                StartClock();
            }
        }

        #endregion

        #region Alarm Interrupt

        /// <summary>
        /// Returns the output polarity of the MFP pin when an alarm interrupt is asserted.
        /// </summary>
        /// <returns>Returns <c>true</c> if the output state of the MFP pin is logic high when an alarm is asserted, <c>false</c> if it is logic low.</returns>
        public bool GetAlarmInterruptPolarity()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity);
        }

        /// <summary>
        /// Sets the output polarity of the MFP pin when an alarm interrupt is asserted.
        /// </summary>
        /// <param name="polarity">
        /// <list type="table">
        /// <item><c>true</c> = the output state of the MFP pin is logic high when an alarm is asserted.</item>
        /// <item><c>false</c> = the output state of the MFP pin is logic low when an alarm is asserted.</item>
        /// </list>
        /// </param>
        public void SetAlarmInterruptPolarity(bool polarity)
        {
            if (polarity)
            {
                RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity);
            }
            else
            {
                RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterruptPolarity);
            }
        }

        #endregion

        #region Alarm1

        /// <summary>
        /// Reads the settings of Alarm 1 from the device.
        /// </summary>
        /// <returns>An <see cref = "Alarm" /> object whose value is the current settings of Alarm 1.</returns>
        public Alarm GetAlarm1()
        {
            return new Alarm(_I2cDevice, Register.Alarm1Second);
        }

        /// <summary>
        /// Sets Alarm 1 on the device.
        /// </summary>
        /// <param name="alarm">The new settings for Alarm 1.</param>
        public void SetAlarm1(Alarm alarm)
        {
            alarm.WriteToDevice(_I2cDevice, Register.Alarm1Second);
        }

        /// <summary>
        /// Enables Alarm 1.
        /// </summary>
        /// <remarks>
        /// Alarm interrupt output can only be used when general purpose output and square wave output are both disabled.
        /// </remarks>
        public void EnableAlarm1()
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled);
        }

        /// <summary>
        /// Disables Alarm 1.
        /// </summary>
        public void DisableAlarm1()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled);
        }

        /// <summary>
        /// Checks if Alarm 1 is enabled.
        /// </summary>
        /// <returns>Returns <c>true</c> if Alarm 1 is enabled, <c>false</c> if not.</returns>
        public bool IsEnabledAlarm1()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm1InterruptEnabled);
        }

        /// <summary>
        /// Checks if Alarm 1 is triggered.
        /// </summary>
        /// <returns>Returns <c>true</c> if Alarm 1 is triggered, <c>false</c> if not.</returns>
        public bool IsTriggeredAlarm1()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterrupt);
        }

        /// <summary>
        /// Clears the triggered state of Alarm 1.
        /// </summary>
        public void ResetAlarm1()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Alarm1Weekday, (byte)AlarmWeekdayRegister.AlarmInterrupt);
        }

        #endregion

        #region Alarm2

        /// <summary>
        /// Reads the settings of Alarm 2 from the device.
        /// </summary>
        /// <returns>An <see cref = "Alarm" /> object whose value is the current settings of Alarm 2.</returns>
        public Alarm GetAlarm2()
        {
            return new Alarm(_I2cDevice, Register.Alarm2Second);
        }

        /// <summary>
        /// Sets Alarm 2 on the device.
        /// </summary>
        /// <param name="alarm">The new settings for Alarm 2.</param>
        public void SetAlarm2(Alarm alarm)
        {
            alarm.WriteToDevice(_I2cDevice, Register.Alarm2Second);
        }

        /// <summary>
        /// Enables Alarm 2.
        /// </summary>
        /// <remarks>
        /// Alarm interrupt output can only be used when general purpose output and square wave output are both disabled.
        /// </remarks>
        public void EnableAlarm2()
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled);
        }

        /// <summary>
        /// Disables Alarm 2.
        /// </summary>
        public void DisableAlarm2()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled);
        }

        /// <summary>
        /// Checks if Alarm 2 is enabled.
        /// </summary>
        /// <returns>Returns <c>true</c> if Alarm 2 is enabled, <c>false</c> if not.</returns>
        public bool IsEnabledAlarm2()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.Alarm2InterruptEnabled);
        }

        /// <summary>
        /// Checks if Alarm 2 is triggered.
        /// </summary>
        /// <returns>Returns <c>true</c> if Alarm 2 is triggered, <c>false</c> if not.</returns>
        public bool IsTriggeredAlarm2()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Alarm2Weekday, (byte)AlarmWeekdayRegister.AlarmInterrupt);
        }

        /// <summary>
        /// Clears the triggered state of Alarm 2.
        /// </summary>
        public void ResetAlarm2()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Alarm2Weekday, (byte)AlarmWeekdayRegister.AlarmInterrupt);
        }

        #endregion

        #region  General Purpose Output

        /// <summary>
        /// Sets the MFP pin level to logic high.
        /// </summary>
        /// <remarks>
        /// General purpose output can only be used when Alarm1, Alarm2 and square wave output are all disabled.
        /// </remarks>
        public void SetGeneralPurposeOutputHigh()
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput);
        }

        /// <summary>
        /// Sets the MFP pin level to logic low.
        /// </summary>
        /// <remarks>
        /// General purpose output can only be used when Alarm1, Alarm2 and square wave output are all disabled.
        /// </remarks>
        public void SetGeneralPurposeOutputLow()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput);
        }

        /// <summary>
        /// Checks if general purpose output is enabled.
        /// </summary>
        /// <returns>Returns <c>true</c> if general purpose output is enabled, <c>false</c> if not.</returns>
        public bool IsEnabledGeneralPurposeOutput()
        {
            if (IsEnabledAlarm1() || IsEnabledAlarm2() || IsEnabledSquareWaveOutput())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if general purpose output is setting the state of the MFP pin to high.
        /// </summary>
        /// <returns>Returns <c>true</c> if MFP pin is high, <c>false</c> if not.</returns>
        public bool GeneralPurposeOutputIsHigh()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.GeneralPurposeOutput);
        }

        #endregion

        #region Square Wave

        /// <summary>
        /// Enables square wave output from the MFP pin.
        /// </summary>
        public void EnableSquareWaveOutput()
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput);
        }

        /// <summary>
        /// Disables square wave output from the MFP pin.
        /// </summary>
        public void DisableSquareWaveOutput()
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput);
        }

        /// <summary>
        /// Checks if square wave output is enabled.
        /// </summary>
        /// <returns>Returns <c>true</c> if square wave is enabled, <c>false</c> if not.</returns>
        public bool IsEnabledSquareWaveOutput()
        {
            return RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveOutput);
        }

        /// <summary>
        /// Gets the frequency of the square wave output on the MFP pin.
        /// </summary>
        /// <returns>A <see cref = "SquareWaveFrequency" /> object whose value is the current frequency of the square wave.</returns>
        public SquareWaveFrequency GetSquareWaveFrequency()
        {
            byte control = RegisterHelper.ReadRegister(_I2cDevice, (byte)Register.Control);

            return (SquareWaveFrequency)(control & (byte)ControlRegister.SquareWaveFrequencyMask);
        }

        /// <summary>
        /// Sets the frequency of the square wave output on the MFP pin.
        /// </summary>
        /// <param name="frequency">The frequency of the square wave.</param>
        public void SetSquareWaveFrequency(SquareWaveFrequency frequency)
        {
            RegisterHelper.MaskedSetRegisterBits(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.SquareWaveFrequencyMask, (byte)frequency);
        }

        #endregion

        #region Clock

        /// <summary>
        /// Starts the clocks oscillator.
        /// </summary>
        /// <param name="blockUntilOscillatorActive">Determins if the function should block execution until the clocks oscillator is fully active.</param>
        public void StartClock(bool blockUntilOscillatorActive = false)
        {
            RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled);

            if (blockUntilOscillatorActive)
            {
                // Wait for the clock to signal that the oscillator has successfully started.
                while (IsHalted())
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Stops the clocks oscillator.
        /// </summary>
        /// <param name="blockUntilOscillatorInactive">Determins if the function should block execution until the clocks oscillator is fully stopped.</param>
        public void Halt(bool blockUntilOscillatorInactive = false)
        {
            RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.TimekeepingSecond, (byte)TimekeepingSecondRegister.OscillatorInputEnabled);

            if (blockUntilOscillatorInactive)
            {
                // Wait for the clock to signal that the oscillator has successfully been stopped.
                while (!IsHalted())
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Checks if the clocks oscillator has been halted.
        /// </summary>
        /// <returns>Returns <c>false</c> if oscillator is currently halted, <c>true</c> if not.</returns>
        public bool IsHalted()
        {
            return !RegisterHelper.RegisterBitIsSet(_I2cDevice, (byte)Register.TimekeepingWeekday, (byte)TimekeepingWeekdayRegister.OscillatorRunning);
        }

        /// <summary>
        /// Sets the time trimming correction to compensate for a clock frequency mismatch.
        /// </summary>
        /// <param name="measuredFrequency">The measured frequency of the clocks external crystal oscillator from the MFP pin.</param>
        /// <param name="idealFrequency">The desired clock frequency.</param>
        /// <param name="mode">The rate that oscillator trimming adjustments are applied.</param>
        public void SetTimeAdjustment(int measuredFrequency, SquareWaveFrequency idealFrequency, TrimmingMode mode = TrimmingMode.NormalTrimMode)
        {
            int idealFrequencyTicks;

            switch (idealFrequency)
            {
                case SquareWaveFrequency.Frequency4kHz:

                    idealFrequencyTicks = 4096;
                    break;

                case SquareWaveFrequency.Frequency8kHz:

                    idealFrequencyTicks = 8192;
                    break;

                case SquareWaveFrequency.Frequency32kHz:

                    idealFrequencyTicks = 32768;
                    break;

                case SquareWaveFrequency.Frequency1Hz:
                default:

                    idealFrequencyTicks = 1;
                    break;
            }

            int adjustment = ((idealFrequencyTicks - measuredFrequency) * (32768 / idealFrequencyTicks) * 60) / 2;
            byte registerValue;

            if (adjustment < 0)
            {
                registerValue = (byte)(0b1000_0000 | (adjustment & 0b0111_1111));
            }
            else
            {
                registerValue = (byte)(adjustment & 0b0111_1111);
            }

            RegisterHelper.WriteRegister(_I2cDevice, (byte)Register.OscillatorTrimming, registerValue);

            // Set trimming mode.
            if (mode == TrimmingMode.NormalTrimMode)
            {
                RegisterHelper.ClearRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.CoarseTrimMode);
            }
            else
            {
                RegisterHelper.SetRegisterBit(_I2cDevice, (byte)Register.Control, (byte)ControlRegister.CoarseTrimMode);
            }
        }

        /// <summary>
        /// Requests time trimming correction not be performed.
        /// </summary>
        public void ClearTimeAdjustment()
        {
            RegisterHelper.WriteRegister(_I2cDevice, (byte)Register.OscillatorTrimming, 0);
        }

        #endregion

        #region SRAM

        /// <summary>
        /// Converts an address to the SRAM address space.
        /// </summary>
        /// <param name="address">The address to convert.</param>
        /// <returns>The address after it has been converted to the SRAM address space.</returns>
        /// <remarks>
        /// Parameter <paramref name="address"/> must be in the range 0 to 63.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        internal byte ConvertAddressToSRAM(byte address)
        {
            // Convert address to SRAM memory address.
            byte adjustedAddress = (byte)(LowerAddressBoundSRAM + address);

            // Verify memory address will not fall outside of SRAM addressable range.
            if (adjustedAddress < LowerAddressBoundSRAM || adjustedAddress > UpperAddressBoundSRAM)
            {
                throw new ArgumentOutOfRangeException(nameof(address));
            }

            return adjustedAddress;
        }

        /// <summary>
        /// Reads a single byte from the devices SRAM at the given address.
        /// </summary>
        /// <param name="address">The address to read from.</param>
        /// <returns>The byte read from the device.</returns>
        /// <remarks>
        /// Parameter <paramref name="address"/> must be in the range 0 to 63.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        public byte ReadByteFromSRAM(byte address)
        {
            byte adjustedAddress = ConvertAddressToSRAM(address);

            return RegisterHelper.ReadRegister(_I2cDevice, adjustedAddress);
        }

        /// <summary>
        /// Writes a single byte to the devices SRAM at the given address.
        /// </summary>
        /// <param name="address">The address to write to.</param>
        /// <param name="value">The byte to be written into the device.</param>
        /// <remarks>
        /// Parameter <paramref name="address"/> must be in the range 0 to 63.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when address falls outside of the addressable range for this device.</exception>
        public void WriteByteToSRAM(byte address, byte value)
        {
            byte adjustedAddress = ConvertAddressToSRAM(address);

            RegisterHelper.WriteRegister(_I2cDevice, adjustedAddress, value);
        }

        #endregion

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
