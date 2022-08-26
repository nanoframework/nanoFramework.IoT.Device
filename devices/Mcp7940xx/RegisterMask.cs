// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp7940xx
{
    /// <summary>
    /// Register mask constants for the Mcp7940xx family.
    /// </summary>
    internal enum RegisterMask : byte
    {
        #region Alarm Day Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm day.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// </remarks>
        AlarmDayMask = 0b0011_1111,

        #endregion

        #region Alarm Hour Register

        /// <summary>
        /// Determines if alarm is in 12 or 24 hour time format.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// <list type="table">
        /// <item>1 = 12 hour format.</item>
        /// <item>0 = 24 hour format.</item>
        /// </list> 
        /// </remarks>
        AlarmTimeFormatMask = 0b0100_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm hour.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// </remarks>
        AlarmHourMask = 0b0011_1111,

        #endregion

        #region Alarm Minute Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm minute.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// </remarks>
        AlarmMinuteMask = 0b0111_1111,

        #endregion

        #region Alarm Month Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm alarm month.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// </remarks>
        AlarmMonthMask = 0b0001_1111,

        #endregion

        #region Alarm Second Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm second.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// </remarks>
        AlarmSecondMask = 0b0111_1111,

        #endregion

        #region Alarm Weekday Register

        /// <summary>
        /// Determines polarity of the MFP pin when in Alarm Interrupt Output mode.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// <list type="table">
        /// <item>1 = MFP is a logic high when alarm is asserted.</item>
        /// <item>0 = MFP is a logic low when alarm is asserted.</item>
        /// </list>
        /// </remarks>
        AlarmInterruptPolarityMask = 0b1000_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm match mode.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// Must contain a value defined in <see cref="AlarmMatchMode" />.
        /// </remarks>
        AlarmMatchModeMask = 0b0111_0000,

        /// <summary>
        /// Indicates if the alarm interrupt has been triggered.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// <list type="table">
        /// <item>1 = Alarm has been triggered. (This flag must be cleared by software.)</item>
        /// <item>0 = Alarm has not been triggered.</item>
        /// </list>
        /// </remarks>
        AlarmInterruptMask = 0b0000_1000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the alarm day-of-the-week.
        /// </summary>
        /// <remarks>
        /// Used for both Alarm1 and Alarm2 registers.
        /// Contains a value from 1 to 7. The representation is user-defined.
        /// </remarks>
        AlarmDayOfWeekMask = 0b0000_0111,

        #endregion

        #region Control Register

        /// <summary>
        /// Determines polarity of the MFP pin when in General Purpose Output mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>If <see cref="RegisterMask.SquareWaveOutputMask" /> = 1, this bit is ignored.</item>
        /// <item>If <see cref="RegisterMask.Alarm1InterruptEnabledMask" /> = 1 or <see cref="RegisterMask.Alarm2InterruptEnabledMask" /> = 1, this bit is ignored.</item>
        /// <item>Otherwise :</item>
        /// <item>1 = MFP is a logic high.</item>
        /// <item>0 = MFP is a logic low.</item>
        /// </list>
        /// </remarks>
        GeneralPurposeOutputMask = 0b1000_0000,

        /// <summary>
        /// Determines if the MFP pin is in Square Wave Output mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable square wave output on MFP pin.</item>
        /// <item>0 = Disable square wave output on MFP pin.</item>
        /// </list>
        /// </remarks>
        SquareWaveOutputMask = 0b0100_0000,

        /// <summary>
        /// Determines if Alarm 2 interrupt mode is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable alarm interrupt output on MFP pin for Alarm 2.</item>
        /// <item>0 = Disable alarm interrupt output on MFP pin for Alarm 2.</item>
        /// </list>
        /// </remarks>
        Alarm2InterruptEnabledMask = 0b0010_0000,

        /// <summary>
        /// Determines if Alarm 1 interrupt mode is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Enable alarm interrupt output on MFP pin for Alarm 1.</item>
        /// <item>0 = Disable alarm interrupt output on MFP pin for Alarm 1.</item>
        /// </list>
        /// </remarks>
        Alarm1InterruptEnabledMask = 0b0001_0000,

        /// <summary>
        /// Determines if the clock is being driven by an external clock source.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = X1 pin is being driven by a 32.768 kHz external clock source.</item>
        /// <item>0 = No external clock source is present on the X1 pin.</item>
        /// </list>
        /// </remarks>
        ExternalClockInputMask = 0b000_01000,

        /// <summary>
        /// Determines if the clock is in Coarse Trim mode.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>Coarse Trim mode results in the MCP7940 applying digital trimming 128 times per second.</item>
        /// <item>If <see cref="RegisterMask.SquareWaveOutputMask" /> = 1, the MFP pin will output a trimmed signal.</item>
        /// <item>1 = Enable Coarse Trim mode.</item>
        /// <item>0 = Disable Coarse Trim mode.</item>
        /// </list>
        /// </remarks>
        CoarseTrimModeMask = 0b0000_0100,

        /// <summary>
        /// Determines the frequency of the clock output on MFP the pin if <see cref="RegisterMask.SquareWaveOutputMask" /> = 1.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>If <see cref="RegisterMask.CoarseTrimModeMask" /> = 1, the frequency selected will be subject to coarse trimming.</item>
        /// <item>Must contain a value defined in <see cref="SquareWaveFrequency" />.</item>
        /// </list>
        /// </remarks>
        SquareWaveFrequencyMask = 0b0000_0011,

        #endregion

        #region PowerEvent Day Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the day.
        /// </summary>
        /// <remarks>
        /// Used for both power up and power down registers.
        /// </remarks>
        PowerEventDayMask = 0b0011_1111,

        #endregion

        #region PowerEvent Hour Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the hour.
        /// </summary>
        /// <remarks>
        /// Used for both power up and power down registers.
        /// </remarks>
        PowerEventHourMask = 0b0011_1111,

        #endregion

        #region PowerEvent Minute Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the minute.
        /// </summary>
        /// <remarks>
        /// Used for both power up and power down registers.
        /// </remarks>
        PowerEventMinuteMask = 0b0111_1111,

        #endregion

        #region PowerEvent Month Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the month.
        /// </summary>
        /// <remarks>
        /// Used for both power up and power down registers.
        /// </remarks>
        PowerEventMonthMask = 0b0001_1111,

        /// <summary>
        /// Mask to seperate the bits pertaining to the weekday.
        /// </summary>
        /// <remarks>
        /// Used for both power up and power down registers.
        /// </remarks>
        PowerEventWeekdayMask = 0b1110_0000,

        #endregion

        #region Timekeeping Day Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the day.
        /// </summary>
        ClockDayMask = 0b0111_1111,

        #endregion

        #region Timekeeping Hour Register

        /// <summary>
        /// Determines 12 or 24 hour time format.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = 12 hour format.</item>
        /// <item>0 = 24 hour format.</item>
        /// </list> 
        /// </remarks>
        ClockTimeFormatMask = 0b0100_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the hour.
        /// </summary>
        ClockHourMask = 0b0011_1111,

        #endregion

        #region Timekeeping Minute Register

        /// <summary>
        /// Mask to seperate the bits pertaining to the minute.
        /// </summary>
        ClockMinuteMask = 0b0111_1111,

        #endregion

        #region Timekeeping Month Register

        /// <summary>
        /// Determines if the current year is a leap year.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Year is a leap year.</item>
        /// <item>0 = Year is not a leap year.</item>
        /// </list> 
        /// </remarks>
        IsLeapYearMask = 0b0010_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the month.
        /// </summary>
        ClockMonthMask = 0b0001_1111,

        #endregion

        #region Timekeeping Second Register

        /// <summary>
        /// Determines if the external oscillator input is active.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Oscillator input enabled.</item>
        /// <item>0 = Oscillator input disabled.</item>
        /// </list> 
        /// </remarks>
        OscillatorInputEnabledMask = 0b1000_0000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the second.
        /// </summary>
        ClockSecondMask = 0b0111_1111,

        #endregion

        #region Timekeeping Weekday Register

        /// <summary>
        /// Shows if the oscillator is currently running.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Oscillator is enabled and running.</item>
        /// <item>0 = Oscillator has stopped or has been disabled.</item>
        /// <item>This bit is read only.</item>
        /// </list>
        /// </remarks>
        OscillatorRunningMask = 0b0010_0000,

        /// <summary>
        /// Shows if the device has experienced a power failure.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Primary power was lost. (This flag must be cleared by software.)</item>
        /// <item>0 = Primary power has not been lost.</item>
        /// </list>
        /// </remarks>
        PowerFailureStatusMask = 0b0001_0000,

        /// <summary>
        /// Shows if the device has experienced a power failure.
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <item>1 = Battery backup input is enabled.</item>
        /// <item>0 = Battery backup input is disabled.</item>
        /// </list>
        /// </remarks>
        ExternalBatteryBackupEnabledMask = 0b0000_1000,

        /// <summary>
        /// Mask to seperate the bits pertaining to the weekday.
        /// </summary>
        ClockWeekdayMask = 0b0000_0111

        #endregion
    }
}
