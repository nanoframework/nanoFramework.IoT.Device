// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
//using System.Diagnostics;
//using Iot.Device.Common;

namespace Iot.Device.Rtc
{

    /// <summary>
    /// Driver for RTC PCF85263 with key functions implemented for time
    /// </summary>
    /// <remarks>
    /// Partial work on support for alarm and some other system functions (commented out).
    /// See datasheet for details: https://www.nxp.com/docs/en/data-sheet/PCF85263A.pdf
    /// </remarks>
    public class Pcf85263 : RtcBase
    {
        /// <summary>
        /// Pcf8563 Default I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x51;
        private I2cDevice _i2cDevice;

        /// <summary>
        /// Creates a new instance of the Pcf85263
        /// </summary>
        /// <param name="i2cDevice">The I2C device used for communication.</param>
        public Pcf85263(I2cDevice i2cDevice) // TODO: ensure default parameters!
        {
            _i2cDevice = i2cDevice ?? throw new ArgumentNullException(nameof(i2cDevice));

        }

        protected override DateTime ReadTime()
        {
            // Sec, Min, Hour, Date, Day, Month & Century, Year
            SpanByte readBuffer = new byte[7];

            _i2cDevice.WriteByte((byte)Rtc.PCF85263RtcRegisters.RTC_SECOND_ADDR);
            _i2cDevice.Read(readBuffer);

            return new DateTime(1900 + (readBuffer[5] >> 7) * 100 + NumberHelper.Bcd2Dec(readBuffer[6]),
                                           NumberHelper.Bcd2Dec((byte)(readBuffer[5] & 0b_0001_1111)),
                                           NumberHelper.Bcd2Dec((byte)(readBuffer[3] & 0b_0011_1111)),
                                           NumberHelper.Bcd2Dec((byte)(readBuffer[2] & 0b_0011_1111)),
                                           NumberHelper.Bcd2Dec((byte)(readBuffer[1] & 0b_0111_1111)),
                                           NumberHelper.Bcd2Dec((byte)(readBuffer[0] & 0b_0111_1111)));
        }


        /// <summary>
        /// Set Pcf8563 Time
        /// </summary>
        /// <param name="time">Time</param>
        protected override void SetTime(DateTime time)
        {

            SpanByte writeBuffer = new byte[8];

            writeBuffer[0] = (byte)Rtc.PCF85263RtcRegisters.RTC_SECOND_ADDR;
            // Set bit8 as 0 to guarantee clock integrity
            writeBuffer[1] = (byte)(NumberHelper.Dec2Bcd(time.Second) & 0b_0111_1111);
            writeBuffer[2] = NumberHelper.Dec2Bcd(time.Minute);
            writeBuffer[3] = NumberHelper.Dec2Bcd(time.Hour);
            writeBuffer[4] = NumberHelper.Dec2Bcd(time.Day);
            writeBuffer[5] = NumberHelper.Dec2Bcd((int)time.DayOfWeek);
            if (time.Year >= 2000)
            {
                writeBuffer[6] = (byte)(NumberHelper.Dec2Bcd(time.Month) | 0b_1000_0000);
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
        /// Cleanup
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            _i2cDevice?.Dispose();
            _i2cDevice = null!;

            base.Dispose(disposing);
        }


        //public void DisableClockOutEnableInterruptA()
        //{
        //    const byte CLKPM_BIT_7 = 0x07;
        //    const byte INTA_OUT_BIT_1 = 0x01;
        //    // Disable CLK and enable interrupts on CLK/INTA pin
        //    SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_PIN_IO_ADDR, CLKPM_BIT_7);
        //    SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_PIN_IO_ADDR, INTA_OUT_BIT_1);
        //}


        /// <summary>
        /// Set alarm for seconds/min/hour/day/month, a full calandar like alarm
        /// </summary>
        /// <param name="alarmDateTime">Date and time to set the alarm, must be in teh future from present RTC time.</param>
        /// <param name="alarm">The alarm to set.</param>
        /// <returns>True if alarm set, False if the alarm date is in the past from the RTC time.</returns>
        public bool SetAlarm(DateTime alarmDateTime, int alarm)
        {
            //    if (alarmDateTime < ReadTime())
            //        return false;

            //    ClearAlarmInterrupt();

            //    Debug.WriteLine("Alarm set: " + alarmDateTime.ToString());

            //    byte[] sb = new byte[6] { (byte)Rtc.PCF85263RtcRegisters.RTC_ALARM1_SECOND_ADDR,  // start at location 1 for alarm seconds
            //                           NumberHelper.Dec2Bcd(alarmDateTime.Second),
            //                           NumberHelper.Dec2Bcd(alarmDateTime.Minute),
            //                           NumberHelper.Dec2Bcd(alarmDateTime.Hour),
            //                           NumberHelper.Dec2Bcd(alarmDateTime.Day),
            //                           NumberHelper.Dec2Bcd(alarmDateTime.Month)
            //                           };

            //    _i2cDevice.Write(sb);

            //    EnableAlarm();

            //    return true;
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the alarm if any, for month, day, hour, min seconds. The year part is just picked to be current year but
        /// alarm will fire every time for that month and the date/time.
        /// </summary>
        /// <param name="alarm">The alarm to set.</param>
        /// <returns>Date and time of alarm if set, or DateTime.MinValue if alarm is not set.</returns>
        public DateTime GetAlarm(int alarm)
        {
            //    _i2cDevice.WriteByte((byte)Rtc.PCF85263RtcRegisters.RTC_ALARM1_SECOND_ADDR);

            //    byte[] dtNow = new byte[6];
            //    _i2cDevice.Read(dtNow);
            //    byte ss = NumberHelper.Bcd2Bin(dtNow[0]);
            //    byte mm = NumberHelper.Bcd2Bin(dtNow[1]);
            //    byte hh = NumberHelper.Bcd2Bin(dtNow[2]);
            //    byte d = NumberHelper.Bcd2Bin(dtNow[3]);
            //    byte m = NumberHelper.Bcd2Bin(dtNow[4]);
            //    int y = NumberHelper.Bcd2Bin(dtNow[5]) + 2000;

            //    try
            //    {
            //        if (m == 0 || d == 0)
            //            return DateTime.MinValue;
            //        else
            //            return new DateTime(DateTime.UtcNow.Year, m, d, hh, mm, ss);
            //    }
            //    catch
            //    {
            //        return DateTime.MinValue;
            //    }
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Enables the alarm, and the interrupts for the Alarm 1, for month/day/hour/min/sec always.
        ///// </summary>
        //void EnableAlarm()
        //{
        //    DisableClockOutEnableInterruptA();
        //    const byte ALARM1_SECONDS_BIT = 0x00;
        //    const byte ALARM1_MINUTES_BIT = 0x00;
        //    const byte ALARM1_HOURS_BIT = 0x00;
        //    const byte ALARM1_DAYS_BIT = 0x00;
        //    const byte ALARM1_MONTHS_BIT = 0x00;

        //    // mode = (1 << ALARM1_MINUTES_BIT) | (1 << ALARM1_HOURS_BIT) enables hour/minute of alarm1
        //    byte mode = (byte)((1 << ALARM1_SECONDS_BIT) | (1 << ALARM1_MINUTES_BIT) | (1 << ALARM1_HOURS_BIT) |
        //        (1 << ALARM1_DAYS_BIT) | (1 << ALARM1_MONTHS_BIT));

        //    WriteRegister((byte)Rtc.PCF85263RtcRegisters.RTC_ALARM_ENABLES_ADDR, mode);

        //    const byte INTA_A1IEA_BIT_4 = 0x04;
        //    const byte INTA_ILPA_BIT_7 = 0x07;
        //    SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_INTA_ENABLE_ADDR, INTA_A1IEA_BIT_4);
        //    //set interrupt to permanent low
        //    SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_INTA_ENABLE_ADDR, INTA_ILPA_BIT_7);
        //}


        ///// <summary>
        ///// Clears the alarm flag bit 5 and also disables clockout and enables interrupt for Alarm.
        ///// </summary>
        //public void ClearAlarmInterrupt()
        //{
        //    const byte ALARM1_AF1_FLAG_BIT5 = 0x05;
        //    ClearRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_FLAGS_ADDR, ALARM1_AF1_FLAG_BIT5);
        //    DisableClockOutEnableInterruptA();
        //}

        ///// <summary>
        ///// Checks if the alarm flag bit 5 is set  indicating there is active alarm.
        ///// </summary>
        ///// <returns>True if bit 5 of flags redister is set otherwise false.</returns>
        //public bool IsAlarmActive()
        //{
        //    const byte ALARM1_AF1_FLAG_BIT5 = 0x05;
        //    byte tmp = ReadRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FLAGS_ADDR);
        //    return tmp.IsBitSet(ALARM1_AF1_FLAG_BIT5);
        //}


        //void SetPeriodicInterrupt(bool forSeconds = true)
        //{
        //    if (forSeconds)
        //    {
        //        const byte INTA_PI_SECOND_BIT = 0x01;
        //        SetRegisterBit((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR, INTA_PI_SECOND_BIT);
        //    }
        //    else
        //    {
        //        const byte INTA_PI_MINUTE_BIT = 0x02;
        //        SetRegisterBit((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR, INTA_PI_MINUTE_BIT);
        //        Debug.WriteLine("Periodit INT per minute set: ");
        //    }
        //}

        ///// <summary>
        ///// Enables period interrupt on the interrupt pin
        ///// </summary>
        //void EnablePeriodicInterrupt()
        //{
        //    DisableClockOutEnableInterruptA();
        //    const byte INTA_PIEA_BIT = 0x01;
        //    SetRegisterBit((byte)Rtc.PCF85263ControlRegisters.CTRL_INTA_ENABLE_ADDR, INTA_PIEA_BIT);
        //}

        //void DisablePeriodicInterrupt()
        //{
        //    const byte INTA_PIEA_BIT = 0x00;
        //    ClearRegisterBit((byte)Rtc.PCF85263ControlRegisters.CTRL_INTA_ENABLE_ADDR, INTA_PIEA_BIT);
        //}

        ////void SetHourMode(byte hour)
        ////{
        ////    //DateTime time = getTime();
        ////    //DateTime then = Date.now();
        ////    StopClock();
        ////    // bit 5 = 12/24 hour mode, 1 = 12 hour mode 0 = 24 hour mode
        ////    byte mode = ReadRegister(REG_OSC_25);
        ////    switch (hour)
        ////    {
        ////        case 12:
        ////            mode |= 0x20;
        ////            break;

        ////        case 24:
        ////            mode &= 0xDF;
        ////            break;

        ////        default:
        ////            return;
        ////    }
        ////    WriteRegister(REG_OSC_25, mode);
        ////    //time += Date.now() - then; // adjust for lost time
        ////    //SetTime(time);
        ////}


        //public byte GetHourMode()
        //{
        //    byte mode = ReadRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR);
        //    mode &= 0x20;

        //    if (mode != 0)
        //        return 12;
        //    else
        //        return 24;
        //}


        //public void EnableHundredths()
        //{
        //    byte temp = ReadRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR);
        //    // bit 7 on
        //    temp |= 0x80;
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR, temp);
        //}

        //public void DisableHundredths()
        //{
        //    byte temp = ReadRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR);
        //    // bit 7 off
        //    temp &= 0x7F;
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FUNCTION_ADDR, temp);
        //}

        //// set drive control for quartz series resistance, ESR, motional resistance, or Rs
        //// 100k ohms = 'normal', 60k ohms = 'low', 500k ohms = 'high'
        //void SetDriveControl(string drive)
        //{
        //    // bit 3 - 2 = drive control, 00 = normal Rs 100k, 01 = low drive Rs 60k, 10 and 11 = high drive Rs 500k
        //    byte bits = 0x00;
        //    switch (drive)
        //    {
        //        case "low":
        //            bits = 0x04;
        //            break;

        //        case "normal":
        //            bits = 0x00;
        //            break;

        //        case "high":
        //            bits = 0x08;
        //            break;

        //        default:
        //            throw new Exception("Invalid drive mode " + drive);
        //    }
        //    byte tmp = ReadRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR);
        //    tmp |= bits;
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, tmp);
        //}

        //// set load capacitance for quartz crystal in pF, valid values are 6, 7, or 12.5
        //void SetLoadCapacitance(double capacitance)
        //{
        //    // bit 1 - 0 = load capacitance, 00 = 7.0pF, 01 = 6.0pF, 10 and 11 = 12.5pF
        //    byte bits = 0x00;
        //    switch (capacitance)
        //    {
        //        case 7:
        //            bits = 0x00;
        //            break;

        //        case 6:
        //            bits = 0x01;
        //            break;

        //        case 12.5:
        //            bits = 0x02;
        //            break;

        //        default:
        //            throw new Exception("Invalid load capacitance.");
        //    }
        //    byte tmp = ReadRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR);
        //    tmp |= bits;
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, tmp);

        //}


        //// enable the battery switch circuitry
        //void EnableBatterySwitch()
        //{
        //    byte tmp = ReadRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_BATTERY_SWITCH_ADDR);
        //    tmp &= 0x0F;
        //    WriteRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_BATTERY_SWITCH_ADDR, tmp);
        //}

        //// disable the battery switch circuitry
        //void DisableBatterySwitch()
        //{
        //    byte tmp = ReadRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_BATTERY_SWITCH_ADDR);
        //    tmp |= 0x10;
        //    WriteRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_BATTERY_SWITCH_ADDR, tmp);
        //}

        //void StartClock()
        //{
        //    WriteRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_STOP_ADDR, 0);
        //}

        //void ResetClock()
        //{
        //    WriteRegister((byte) Rtc.PCF85263ControlRegisters.CTRL_RESET_ADDR, 0x2c);
        //}

        //void SetRegisterBit(byte reg, byte bitNum)
        //{
        //    //byte ss = ReadRegister(reg);
        //    //ss = ss.SetBit(bitNum);
        //    //WriteRegister(reg, ss);
        //    WriteRegister(reg, bitNum);
        //}

        //void ClearRegisterBit(byte reg, byte bitNum)
        //{
        //    byte ss = ReadRegister(reg);
        //    ss = ss.ClearBit(bitNum);
        //    WriteRegister(reg, ss);
        //}

        //// mode = 0 for normal 7pF, 1 for low 6pF, 2 for high 12.5 pF
        //void SetOscillatorCapacitance(byte capVal = 0)
        //{
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, 0x2B);

        //    //switch (capVal)
        //    //{
        //    //    case 0:// 7 pF
        //    //    default:
        //    //        ClearRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_HIGH_CAPACITANCE_BIT);
        //    //        ClearRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_LOW_CAPACITANCE_BIT);
        //    //        break;
        //    //    case 1: //low 6 pF
        //    //        ClearRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_HIGH_CAPACITANCE_BIT);
        //    //        SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_LOW_CAPACITANCE_BIT);
        //    //        break;
        //    //    case 2: //high 12.5 pF
        //    //        SetRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_HIGH_CAPACITANCE_BIT);
        //    //        ClearRegisterBit((byte) Rtc.PCF85263ControlRegisters.CTRL_OSCILLATOR_ADDR, OSC_LOW_CAPACITANCE_BIT);
        //    //        break;
        //    //}
        //}

        //void ClearAllFlags()
        //{
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_FLAGS_ADDR, 0x00);
        //}

        //public void StopClock()
        //{
        //    WriteRegister((byte)Rtc.PCF85263ControlRegisters.CTRL_STOP_ADDR, 1);
        //}

        //byte ReadRegister(byte addrByte)
        //{
        //    _i2cDevice.WriteByte(addrByte);
        //    return _i2cDevice.ReadByte();
        //}

        //void WriteRegister(byte reg, byte cmd)
        //{
        //    _i2cDevice.Write(new byte[] { reg, cmd });
        //}


        ////public void Debug_PrintAllRegisters()
        ////{
        ////    for (byte i = 0; i < 48; i++)
        ////    {
        ////        Debug_PrintRegister(i);
        ////    }
        ////}

        ////public void Debug_PrintRegister(byte regAdd)
        ////{
        ////    byte reg = ReadRegister(regAdd);
        ////    Debug.WriteLine("REG " + regAdd.ToString("X") + " " + RtcHelper.ByteToBitsString(reg));
        ////}

    }

}
