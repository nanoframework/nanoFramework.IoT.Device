// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
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
        protected readonly I2cDevice _i2cDevice;

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
                throw new ArgumentNullException();
            }

            _i2cDevice = i2cDevice;

            // Set the clock to 24 hour mode.
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.TimekeepingHour, (byte)RegisterMask.ClockTimeFormatMask);

            // Set clock source.
            if (clockSource == ClockSource.ExternalClockInput)
            {
                RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.ExternalClockInputMask);
            }
            else
            {
                RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.ExternalClockInputMask);
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
            Span<byte> readBuffer = new byte[7];

            _i2cDevice.WriteByte((byte)Register.TimekeepingSecond);
            _i2cDevice.Read(readBuffer);

            int second = NumberHelper.Bcd2Dec((byte)(readBuffer[0] & (byte)RegisterMask.ClockSecondMask));
            int minute = NumberHelper.Bcd2Dec((byte)(readBuffer[1] & (byte)RegisterMask.ClockMinuteMask));
            int hour = NumberHelper.Bcd2Dec((byte)(readBuffer[2] & (byte)RegisterMask.ClockHourMask));
            int day = NumberHelper.Bcd2Dec((byte)(readBuffer[4] & (byte)RegisterMask.ClockDayMask));
            int month = NumberHelper.Bcd2Dec((byte)(readBuffer[5] & (byte)RegisterMask.ClockMonthMask));
            int year = NumberHelper.Bcd2Dec(readBuffer[6]) + 2000;

            return new DateTime(year, month, day, hour, minute, second);
        }

        /// <summary>
        /// Sets the time.
        /// </summary>
        /// <param name="time">The new time.</param>
        public void SetTime(DateTime time)
        {
            bool clockIsRunning = !IsHalted;

            if (clockIsRunning)
            {
                // To avoid rollover issues when loading new time and date values, the clocks oscillator must be disabled if it is currently running.
                Halt(true);
            }

            Span<byte> writeBuffer = new byte[8];

            writeBuffer[0] = (byte)Register.TimekeepingSecond;
            writeBuffer[1] = NumberHelper.Dec2Bcd(time.Second);
            writeBuffer[2] = NumberHelper.Dec2Bcd(time.Minute);
            writeBuffer[3] = NumberHelper.Dec2Bcd(time.Hour);
            writeBuffer[4] = NumberHelper.Dec2Bcd((int)time.DayOfWeek);
            writeBuffer[5] = NumberHelper.Dec2Bcd(time.Day);
            writeBuffer[6] = NumberHelper.Dec2Bcd(time.Month);
            writeBuffer[7] = NumberHelper.Dec2Bcd(time.Year % 100);

            _i2cDevice.Write(writeBuffer);

            // Re-enable the clocks oscillator if it was running at the start of this function.
            if (clockIsRunning)
            {
                StartClock();
            }
        }

        #endregion

        #region Alarm Interrupt

        /// <summary>
        /// Gets or sets the output polarity of the MFP pin when an alarm interrupt is asserted.
        /// </summary>
        public PinValue AlarmInterruptPolarity
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask);
            }

            set
            {
                if (value == PinValue.High)
                {
                    RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask);
                }
                else
                {
                    RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptPolarityMask);
                }
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
            return new Alarm(_i2cDevice, Register.Alarm1Second);
        }

        /// <summary>
        /// Sets Alarm 1 on the device.
        /// </summary>
        /// <param name="alarm">The new settings for Alarm 1.</param>
        public void SetAlarm1(Alarm alarm)
        {
            alarm.WriteToDevice(_i2cDevice, Register.Alarm1Second);
        }

        /// <summary>
        /// Enables Alarm 1.
        /// </summary>
        /// <remarks>
        /// Alarm interrupt output can only be used when general purpose output and square wave output are both disabled.
        /// </remarks>
        public void EnableAlarm1()
        {
            RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask);
        }

        /// <summary>
        /// Disables Alarm 1.
        /// </summary>
        public void DisableAlarm1()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask);
        }

        /// <summary>
        /// Clears the triggered state of Alarm 1.
        /// </summary>
        public void ResetAlarm1()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptMask);
        }

        /// <summary>
        /// Gets a value indicating whether Alarm 1 is enabled.
        /// </summary>
        public bool IsEnabledAlarm1
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm1InterruptEnabledMask);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Alarm 1 is triggered.
        /// </summary>
        public bool IsTriggeredAlarm1
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm1Weekday, (byte)RegisterMask.AlarmInterruptMask);
            }
        }

        #endregion

        #region Alarm2

        /// <summary>
        /// Reads the settings of Alarm 2 from the device.
        /// </summary>
        /// <returns>An <see cref = "Alarm" /> object whose value is the current settings of Alarm 2.</returns>
        public Alarm GetAlarm2()
        {
            return new Alarm(_i2cDevice, Register.Alarm2Second);
        }

        /// <summary>
        /// Sets Alarm 2 on the device.
        /// </summary>
        /// <param name="alarm">The new settings for Alarm 2.</param>
        public void SetAlarm2(Alarm alarm)
        {
            alarm.WriteToDevice(_i2cDevice, Register.Alarm2Second);
        }

        /// <summary>
        /// Enables Alarm 2.
        /// </summary>
        /// <remarks>
        /// Alarm interrupt output can only be used when general purpose output and square wave output are both disabled.
        /// </remarks>
        public void EnableAlarm2()
        {
            RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask);
        }

        /// <summary>
        /// Disables Alarm 2.
        /// </summary>
        public void DisableAlarm2()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask);
        }

        /// <summary>
        /// Clears the triggered state of Alarm 2.
        /// </summary>
        public void ResetAlarm2()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Alarm2Weekday, (byte)RegisterMask.AlarmInterruptMask);
        }

        /// <summary>
        /// Gets a value indicating whether Alarm 2 is enabled.
        /// </summary>
        public bool IsEnabledAlarm2
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.Alarm2InterruptEnabledMask);
            }
        }

        /// <summary>
        /// Gets a value indicating whether Alarm 2 is triggered.
        /// </summary>
        public bool IsTriggeredAlarm2
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Alarm2Weekday, (byte)RegisterMask.AlarmInterruptMask);
            }
        }

        #endregion

        #region  General Purpose Output

        /// <summary>
        /// Gets or sets the MFP pin level when in general purpose output mode.
        /// </summary>
        /// <remarks>
        /// General purpose output can only be used when Alarm1, Alarm2 and square wave output are all disabled.
        /// </remarks>
        public PinValue GeneralPurposeOutput
        {
            get
            {
                if (RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask))
                {
                    return PinValue.High;
                }
                else
                {
                    return PinValue.Low;
                }
            }

            set
            {
                if (value == PinValue.High)
                {
                    RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask);
                }
                else
                {
                    RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.GeneralPurposeOutputMask);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the general purpose output is enabled.
        /// </summary>
        public bool IsEnabledGeneralPurposeOutput
        {
            get
            {
                if (IsEnabledAlarm1 || IsEnabledAlarm2 || IsEnabledSquareWaveOutput)
                {
                    return false;
                }

                return true;
            }
        }

        #endregion

        #region Square Wave

        /// <summary>
        /// Enables square wave output from the MFP pin.
        /// </summary>
        public void EnableSquareWaveOutput()
        {
            RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask);
        }

        /// <summary>
        /// Disables square wave output from the MFP pin.
        /// </summary>
        public void DisableSquareWaveOutput()
        {
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask);
        }

        /// <summary>
        /// Gets a value indicating whether the square wave output is enabled.
        /// </summary>
        public bool IsEnabledSquareWaveOutput
        {
            get
            {
                return RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveOutputMask);
            }
        }

        /// <summary>
        /// Gets or sets the frequency of the square wave output on the MFP pin.
        /// </summary>
        public SquareWaveFrequency SquareWaveOutputFrequency
        {
            get
            {
                byte control = RegisterHelper.ReadRegister(_i2cDevice, (byte)Register.Control);

                return (SquareWaveFrequency)(control & (byte)RegisterMask.SquareWaveFrequencyMask);
            }

            set
            {
                RegisterHelper.MaskedSetRegisterBits(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.SquareWaveFrequencyMask, (byte)value);
            }
        }

        #endregion

        #region Clock

        /// <summary>
        /// Starts the clocks oscillator.
        /// </summary>
        /// <param name="blockUntilOscillatorActive">Determins if the function should block execution until the clocks oscillator is fully active.</param>
        public void StartClock(bool blockUntilOscillatorActive = false)
        {
            RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask);

            if (blockUntilOscillatorActive)
            {
                // Wait for the clock to signal that the oscillator has successfully started.
                while (IsHalted)
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
            RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.TimekeepingSecond, (byte)RegisterMask.OscillatorInputEnabledMask);

            if (blockUntilOscillatorInactive)
            {
                // Wait for the clock to signal that the oscillator has successfully been stopped.
                while (!IsHalted)
                {
                    Thread.Sleep(10);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clocks oscillator has been halted.
        /// </summary>
        public bool IsHalted
        {
            get
            {
                return !RegisterHelper.RegisterBitIsSet(_i2cDevice, (byte)Register.TimekeepingWeekday, (byte)RegisterMask.OscillatorRunningMask);
            }
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

            RegisterHelper.WriteRegister(_i2cDevice, (byte)Register.OscillatorTrimming, registerValue);

            // Set trimming mode.
            if (mode == TrimmingMode.NormalTrimMode)
            {
                RegisterHelper.ClearRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.CoarseTrimModeMask);
            }
            else
            {
                RegisterHelper.SetRegisterBit(_i2cDevice, (byte)Register.Control, (byte)RegisterMask.CoarseTrimModeMask);
            }
        }

        /// <summary>
        /// Requests time trimming correction not be performed.
        /// </summary>
        public void ClearTimeAdjustment()
        {
            RegisterHelper.WriteRegister(_i2cDevice, (byte)Register.OscillatorTrimming, 0);
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
                throw new ArgumentOutOfRangeException();
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

            return RegisterHelper.ReadRegister(_i2cDevice, adjustedAddress);
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

            RegisterHelper.WriteRegister(_i2cDevice, adjustedAddress, value);
        }

        #endregion

        /// <inheritdoc/>
        public virtual void Dispose()
        {
        }
    }
}
