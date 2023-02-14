// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Buffers.Binary;
using System.Device.I2c;
using System.Numerics;
using System.Threading;
using Iot.Device.ADXL343Lib;
using nanoFramework.Hardware.Esp32;

namespace Iot.Device.ADXL343
{
    /// <summary>
    /// Library for the ADXL343 sensor using I2C.
    /// </summary>
    public class ADXL343
    {
        private const int Resolution = 1024;

        private int range = 4;
        private byte gravityRangeByte = 0;
        private I2cDevice i2c;

        /// <summary>
        /// Initializes a new instance of the <see cref="ADXL343" /> class.
        /// </summary>
        /// <param name="i2cDevice">I2C Device.</param>
        /// <param name="gravityRange">Gravity Range.</param>
        public ADXL343(I2cDevice i2cDevice, GravityRange gravityRange)
        {
            range = ConvertGravityRangeToInt(gravityRange);
            gravityRangeByte = (byte)gravityRange;
            i2c = i2cDevice;
            Initialize();
        }

        private static int ConvertGravityRangeToInt(GravityRange gravityRange)
        {
            switch (gravityRange)
            {
                case GravityRange.Range02:
                    return 0;
                case GravityRange.Range04:
                    return 1;
                case GravityRange.Range08:
                    return 2;
                case GravityRange.Range16:
                    return 3;
                default:
                    return 0;
            }
        }

        private void Initialize()
        {
            SetDataFormat(false, false, false, true, false, gravityRangeByte);
            SetPowerControl(false, false, true, false, 0);
        }

        #region Device Id
        /// <summary>
        /// The DEVID register holds a fixed device ID code of 0xE5.
        /// </summary>
        /// <param name="deviceId">Device id returned by ADXL 343.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetDeviceId(ref int deviceId)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.DevId);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                deviceId = readBuf[0];
                return true;
            }

            return false;
        }

        #endregion

        #region Thresh Tap

        /// <summary>
        /// The THRESH_TAP register is eight bits and holds the threshold
        /// value for tap interrupts.The data format is unsigned, therefore,
        /// the magnitude of the tap event is compared with the value
        /// in THRESH_TAP for normal tap detection. The scale factor is
        /// 62.5 mg/LSB (that is, 0xFF = 16 g). A value of 0 may result in
        /// undesirable behavior if single tap/double tap interrupts are enabled.
        /// </summary>
        /// <param name="threshTap">Referenced scale factor value 0 to 255.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetThreshTap(ref int threshTap)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ThreshTap);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                threshTap = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The THRESH_TAP register is eight bits and holds the threshold
        /// value for tap interrupts.The data format is unsigned, therefore,
        /// the magnitude of the tap event is compared with the value
        /// in THRESH_TAP for normal tap detection. The scale factor is
        /// 62.5 mg/LSB (that is, 0xFF = 16 g). A value of 0 may result in
        /// undesirable behavior if single tap/double tap interrupts are enabled.
        /// </summary>
        /// <param name="threshTap">Scale factor value 0 to 255.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetThreshTap(int threshTap)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.ThreshTap;
            writeBuf[1] = (byte)threshTap;
            var res = i2c.Write(writeBuf); 

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Offset Adjustments

        /// <summary>
        /// The OFSX, OFSY, and OFSZ registers are each eight bits and offer
        /// user-set offset adjustments in twos complement format with a scale
        /// factor of 15.6 mg/LSB(that is, 0x7F = 2 g). The value stored in
        /// the offset registers is automatically added to the acceleration data,
        /// and the resulting value is stored in the output data registers.
        /// undesirable behavior if single tap/double tap interrupts are enabled.
        /// </summary>
        /// <param name="point">Referenced point containing offset values.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetOffsetAdjustments(ref Vector3 point)
        {
            SpanByte readBuf = new byte[3];
            var res = i2c.WriteByte((byte)Register.OfsX);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                point.X = readBuf[0];
                point.Y = readBuf[1];
                point.Z = readBuf[2];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The OFSX, OFSY, and OFSZ registers are each eight bits and offer
        /// user-set offset adjustments in twos complement format with a scale
        /// factor of 15.6 mg/LSB(that is, 0x7F = 2 g). The value stored in
        /// the offset registers is automatically added to the acceleration data,
        /// and the resulting value is stored in the output data registers.
        /// undesirable behavior if single tap/double tap interrupts are enabled.
        /// </summary>
        /// <param name="point">Point containing X, Y, and Z offset values.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetOffsetAdjustments(Vector3 point)
        {
            SpanByte writeBuf = new byte[4];
            writeBuf[0] = (byte)Register.OfsX;
            writeBuf[1] = (byte)point.X;
            writeBuf[2] = (byte)point.Y;
            writeBuf[3] = (byte)point.Z;
            var res = i2c.Write(writeBuf); 

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region DUR

        /// <summary>
        /// The DUR register is eight bits and contains an unsigned time value
        /// representing the maximum time that an event must be above the
        /// THRESH_TAP threshold to qualify as a tap event. The scale factor
        /// is 625 µs/LSB. A value of 0 disables the single tap/ double tap
        /// functions.
        /// </summary>
        /// <param name="dur">Reference to maximum duration above thresh tap value.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetDUR(ref int dur)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.Dur);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                dur = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The DUR register is eight bits and contains an unsigned time value
        /// representing the maximum time that an event must be above the
        /// THRESH_TAP threshold to qualify as a tap event. The scale factor
        /// is 625 µs/LSB. A value of 0 disables the single tap/ double tap
        /// functions.
        /// </summary>
        /// <param name="dur">Maximum duration above thresh tap value.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetDUR(int dur)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.Dur;
            writeBuf[1] = (byte)dur;
            var res = i2c.Write(writeBuf); 

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Latent

        /// <summary>
        /// The latent register is eight bits and contains an unsigned time value
        /// representing the wait time from the detection of a tap event to the
        /// start of the time window(defined by the window register) during
        /// which a possible second tap event can be detected.The scale
        /// factor is 1.25 ms/LSB.A value of 0 disables the double tap function.
        /// </summary>
        /// <param name="latent">Reference to Wait time between a tap even and a double-tap.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetLatent(ref int latent)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.Latent);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                latent = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The latent register is eight bits and contains an unsigned time value
        /// representing the wait time from the detection of a tap event to the
        /// start of the time window(defined by the window register) during
        /// which a possible second tap event can be detected.The scale
        /// factor is 1.25 ms/LSB.A value of 0 disables the double tap function. 
        /// </summary>
        /// <param name="latent">Wait time between a tap even and a double-tap.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetLatent(int latent)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.Latent;
            writeBuf[1] = (byte)latent;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Window

        /// <summary>
        /// The window register is eight bits and contains an unsigned time
        /// value representing the amount of time after the expiration of the
        /// latency time(determined by the latent register) during which a
        /// second valid tap can begin.The scale factor is 1.25 ms/LSB.A
        /// value of 0 disables the double tap function.
        /// </summary>
        /// <param name="win">Reference to amount of time after expiration of latency time.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetWindow(ref int win)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.Window);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                win = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The window register is eight bits and contains an unsigned time
        /// value representing the amount of time after the expiration of the
        /// latency time(determined by the latent register) during which a
        /// second valid tap can begin.The scale factor is 1.25 ms/LSB.A
        /// value of 0 disables the double tap function.
        /// </summary>
        /// <param name="win">Amount of time after expiration of latency time.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetWindow(int win)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.Window;
            writeBuf[1] = (byte)win;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Threshold Activity

        /// <summary>
        /// The THRESH_ACT register is eight bits and holds the threshold
        /// value for detecting activity.The data format is unsigned, therefore,
        /// the magnitude of the activity event is compared with the value in the
        /// THRESH_ACT register. The scale factor is 62.5 mg/LSB. A value
        /// of 0 may result in undesirable behavior if the activity interrupt is
        /// enabled.
        /// </summary>
        /// <param name="thresholdDetect">Reference to threshold value for detecting activity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetThreshAct(ref int thresholdDetect)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ThreshAct);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                thresholdDetect = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The THRESH_ACT register is eight bits and holds the threshold
        /// value for detecting activity.The data format is unsigned, therefore,
        /// the magnitude of the activity event is compared with the value in the
        /// THRESH_ACT register. The scale factor is 62.5 mg/LSB. A value
        /// of 0 may result in undesirable behavior if the activity interrupt is
        /// enabled.
        /// </summary>
        /// <param name="thresholdDetect">Threshold value for detecting activity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetThreshAct(int thresholdDetect)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.ThreshAct;
            writeBuf[1] = (byte)thresholdDetect;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Threshold Inactivity

        /// <summary>
        /// The THRESH_INACT register is eight bits and holds the threshold
        /// value for detecting inactivity.The data format is unsigned, therefore,
        /// the magnitude of the inactivity event is compared with the value
        /// in the THRESH_INACT register. The scale factor is 62.5 mg/LSB.
        /// A value of 0 may result in undesirable behavior if the inactivity
        /// interrupt is enabled.
        /// </summary>
        /// <param name="thresholdDetect">Reference to threshold value for detecting inactivity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetThreshInact(ref int thresholdDetect)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ThreshInact);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                thresholdDetect = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The THRESH_INACT register is eight bits and holds the threshold
        /// value for detecting inactivity.The data format is unsigned, therefore,
        /// the magnitude of the inactivity event is compared with the value
        /// in the THRESH_INACT register. The scale factor is 62.5 mg/LSB.
        /// A value of 0 may result in undesirable behavior if the inactivity
        /// interrupt is enabled.
        /// </summary>
        /// <param name="thresholdDetect">Reference to threshold value for detecting inactivity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetThreshInact(int thresholdDetect)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.ThreshInact;
            writeBuf[1] = (byte)thresholdDetect;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Time Inactivity

        /// <summary>
        /// The TIME_INACT register is eight bits and contains an unsigned
        /// time value representing the amount of time that acceleration must
        /// be less than the value in the THRESH_INACT register for inactivity
        /// to be declared.The scale factor is 1 sec/LSB.Unlike the other
        /// interrupt functions, which use unfiltered data,
        /// the inactivity function uses filtered output data.At least
        /// one output sample must be generated for the inactivity interrupt to
        /// be triggered.This results in the function appearing unresponsive
        /// if the TIME_INACT register is set to a value less than the time
        /// constant of the output data rate. A value of 0 results in an interrupt
        /// when the output data is less than the value in the THRESH_INACT
        /// register.
        /// </summary>
        /// <param name="time">Reference to the amount of time that acceleration must be less than Threshhold Inactivity to register for inactivity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetTimeInact(ref int time)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.TimeInact);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                time = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The TIME_INACT register is eight bits and contains an unsigned
        /// time value representing the amount of time that acceleration must
        /// be less than the value in the THRESH_INACT register for inactivity
        /// to be declared.The scale factor is 1 sec/LSB.Unlike the other
        /// interrupt functions, which use unfiltered data,
        /// the inactivity function uses filtered output data.At least
        /// one output sample must be generated for the inactivity interrupt to
        /// be triggered.This results in the function appearing unresponsive
        /// if the TIME_INACT register is set to a value less than the time
        /// constant of the output data rate. A value of 0 results in an interrupt
        /// when the output data is less than the value in the THRESH_INACT
        /// register.
        /// </summary>
        /// <param name="time">Amount of time that acceleration must be less than Threshhold Inactivity to register for inactivity.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetTimeInact(int time)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.TimeInact;
            writeBuf[1] = (byte)time;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Act/Inact Control

        /// <summary>
        ///     Act and Inact of AC-DC Bits
        ///     A setting of 0 selects dc-coupled operation, and a setting of 1
        ///     enables ac-coupled operation.In dc-coupled operation, the current
        ///     acceleration magnitude is compared directly with THRESH_ACT
        ///     and THRESH_INACT to determine whether activity or inactivity is
        ///     detected.
        ///     In ac-coupled operation for activity detection, the acceleration value
        ///     at the start of activity detection is taken as a reference value.New
        ///     samples of acceleration are then compared to this reference value,
        ///     and if the magnitude of the difference exceeds the THRESH_ACT
        ///     value, the device triggers an activity interrupt.
        ///     Similarly, in ac-coupled operation for inactivity detection, a reference value is used for comparison and is updated whenever
        ///     the device exceeds the inactivity threshold.After the reference
        ///     value is selected, the device compares the magnitude of the difference between the reference value and the current acceleration
        ///     with THRESH_INACT.If the difference is less than the value in
        ///     THRESH_INACT for the time in TIME_INACT, the device is considered inactive and the inactivity interrupt is triggered.
        ///     Act and Inact of X, Y, and Z
        ///     A setting of 1 enables x-, y-, or z-axis participation in detecting
        ///     activity or inactivity.A setting of 0 excludes the selected axis from
        ///     participation.If all axes are excluded, the function is disabled.For
        ///     activity detection, all participating axes are logically OR’ed, causing
        ///     the activity function to trigger when any of the participating axes
        ///     exceeds the threshold.For inactivity detection, all participating axes
        ///     are logically AND’ed, causing the inactivity function to trigger only if
        ///     all participating axes are below the threshold for the specified time.
        /// </summary>
        /// <param name="actAcDc">Active AC/DC.</param>
        /// <param name="actXEnable">Active X Enable.</param>
        /// <param name="actYEnable">Active Y Enable.</param>
        /// <param name="actZEnable">Active Z Enable.</param>
        /// <param name="inactAcDc">Inactive AC/DC.</param>
        /// <param name="inactXEnable">Inactive X Enable.</param>
        /// <param name="inactYEnable">Inactive Y Enable.</param>
        /// <param name="inactZEnable">Inactive Z Enable.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetActiveInactiveControl(ref bool actAcDc, ref bool actXEnable, ref bool actYEnable, ref bool actZEnable, ref bool inactAcDc, ref bool inactXEnable, ref bool inactYEnable, ref bool inactZEnable)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ActInactCtl);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                actAcDc = (map & (byte)ActInactCtlMap.ActAcDc) == (byte)ActInactCtlMap.ActAcDc;
                actXEnable = (map & (byte)ActInactCtlMap.ActXEnable) == (byte)ActInactCtlMap.ActXEnable;
                actYEnable = (map & (byte)ActInactCtlMap.ActYEnable) == (byte)ActInactCtlMap.ActYEnable;
                actZEnable = (map & (byte)ActInactCtlMap.ActZEnable) == (byte)ActInactCtlMap.ActZEnable;
                inactAcDc = (map & (byte)ActInactCtlMap.InactAcDc) == (byte)ActInactCtlMap.InactAcDc;
                inactXEnable = (map & (byte)ActInactCtlMap.InactXEnable) == (byte)ActInactCtlMap.InactXEnable;
                inactYEnable = (map & (byte)ActInactCtlMap.InactYEnable) == (byte)ActInactCtlMap.InactYEnable;
                inactZEnable = (map & (byte)ActInactCtlMap.InactZEnable) == (byte)ActInactCtlMap.InactZEnable;

                return true;
            }

            return false;
        }

        /// <summary>
        ///     Act and Inact of AC-DC Bits
        ///     A setting of 0 selects dc-coupled operation, and a setting of 1
        ///     enables ac-coupled operation.In dc-coupled operation, the current
        ///     acceleration magnitude is compared directly with THRESH_ACT
        ///     and THRESH_INACT to determine whether activity or inactivity is
        ///     detected.
        ///     In ac-coupled operation for activity detection, the acceleration value
        ///     at the start of activity detection is taken as a reference value.New
        ///     samples of acceleration are then compared to this reference value,
        ///     and if the magnitude of the difference exceeds the THRESH_ACT
        ///     value, the device triggers an activity interrupt.
        ///     Similarly, in ac-coupled operation for inactivity detection, a reference value is used for comparison and is updated whenever
        ///     the device exceeds the inactivity threshold.After the reference
        ///     value is selected, the device compares the magnitude of the difference between the reference value and the current acceleration
        ///     with THRESH_INACT.If the difference is less than the value in
        ///     THRESH_INACT for the time in TIME_INACT, the device is considered inactive and the inactivity interrupt is triggered.
        ///     Act and Inact of X, Y, and Z
        ///     A setting of 1 enables x-, y-, or z-axis participation in detecting
        ///     activity or inactivity.A setting of 0 excludes the selected axis from
        ///     participation.If all axes are excluded, the function is disabled.For
        ///     activity detection, all participating axes are logically OR’ed, causing
        ///     the activity function to trigger when any of the participating axes
        ///     exceeds the threshold.For inactivity detection, all participating axes
        ///     are logically AND’ed, causing the inactivity function to trigger only if
        ///     all participating axes are below the threshold for the specified time.
        /// </summary>
        /// <param name="actAcDc">Active AC/DC.</param>
        /// <param name="actXEnable">Active X Enable.</param>
        /// <param name="actYEnable">Active Y Enable.</param>
        /// <param name="actZEnable">Active Z Enable.</param>
        /// <param name="inactAcDc">Inactive AC/DC.</param>
        /// <param name="inactXEnable">Inactive X Enable.</param>
        /// <param name="inactYEnable">Inactive Y Enable.</param>
        /// <param name="inactZEnable">Inactive Z Enable.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetActiveInactiveControl(bool actAcDc, bool actXEnable, bool actYEnable, bool actZEnable, bool inactAcDc, bool inactXEnable, bool inactYEnable, bool inactZEnable)
        {
            byte map = 0;

            if (actAcDc)
            {
                map |= (byte)ActInactCtlMap.ActAcDc;
            }

            if (actXEnable)
            {
                map |= (byte)ActInactCtlMap.ActXEnable;
            }

            if (actYEnable)
            {
                map |= (byte)ActInactCtlMap.ActYEnable;
            }

            if (actZEnable)
            {
                map |= (byte)ActInactCtlMap.ActZEnable;
            }

            if (inactAcDc)
            {
                map |= (byte)ActInactCtlMap.InactAcDc;
            }

            if (inactXEnable)
            {
                map |= (byte)ActInactCtlMap.InactXEnable;
            }

            if (inactYEnable)
            {
                map |= (byte)ActInactCtlMap.InactYEnable;
            }

            if (inactZEnable)
            {
                map |= (byte)ActInactCtlMap.InactZEnable;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.TapAxes;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Thresh FF

        /// <summary>
        /// The THRESH_FF register is eight bits and holds the threshold
        /// value, in unsigned format, for free-fall detection.The acceleration
        /// on all axes is compared with the value in THRESH_FF to determine
        /// if a free-fall event occurred.The scale factor is 62.5 mg/LSB.Note
        /// that a value of 0 mg may result in undesirable behavior if the
        /// free-fall interrupt is enabled.Values between 300 mg and 600 mg
        /// (0x05 to 0x09) are recommended.
        /// </summary>
        /// <param name="threshFF">Free-fall detection value.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetThreshFF(ref int threshFF)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ThreshFF);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                threshFF = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The THRESH_FF register is eight bits and holds the threshold
        /// value, in unsigned format, for free-fall detection.The acceleration
        /// on all axes is compared with the value in THRESH_FF to determine
        /// if a free-fall event occurred.The scale factor is 62.5 mg/LSB.Note
        /// that a value of 0 mg may result in undesirable behavior if the
        /// free-fall interrupt is enabled.Values between 300 mg and 600 mg
        /// (0x05 to 0x09) are recommended.
        /// </summary>
        /// <param name="threshFF">Free-fall detection value.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetThreshFF(int threshFF)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.ThreshFF;
            writeBuf[1] = (byte)threshFF;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Time FF

        /// <summary>
        /// The TIME_FF register is eight bits and stores an unsigned time
        /// value representing the minimum time that the value of all axes
        /// must be less than THRESH_FF to generate a free-fall interrupt.The
        /// scale factor is 5 ms/LSB.A value of 0 may result in undesirable
        /// behavior if the free-fall interrupt is enabled.Values between 100 ms
        /// and 350 ms (0x14 to 0x46) are recommended.
        /// </summary>
        /// <param name="timeFF">Minimum time all axes less than Thresh FF.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetTimeFF(ref int timeFF)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.TimeFF);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                timeFF = readBuf[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// The TIME_FF register is eight bits and stores an unsigned time
        /// value representing the minimum time that the value of all axes
        /// must be less than THRESH_FF to generate a free-fall interrupt.The
        /// scale factor is 5 ms/LSB.A value of 0 may result in undesirable
        /// behavior if the free-fall interrupt is enabled.Values between 100 ms
        /// and 350 ms (0x14 to 0x46) are recommended.
        /// </summary>
        /// <param name="timeFF">Minimum time all axes less than Thresh FF.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetTimeFF(int timeFF)
        {
            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.TimeFF;
            writeBuf[1] = (byte)timeFF;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Act/Inact Control

        /// <summary>
        /// Suppress Bit
        /// Setting the suppress bit suppresses double tap detection if acceleration greater than the value in THRESH_TAP is present between
        /// taps.
        /// TAP_x Enable Bits
        /// A setting of 1 in the TAP_X enable, TAP_Y enable, or TAP_Z
        /// enable bit enables x-, y-, or z-axis participation in tap detection.
        /// A setting of 0 excludes the selected axis from participation in tap
        /// detection.
        /// </summary>
        /// <param name="suppress">Suppress.</param>
        /// <param name="tapXEnable">Tap X Enable.</param>
        /// <param name="tapYEnable">Tap Y Enable.</param>
        /// <param name="tapZEnable">Tap Z Enable.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetTapAxes(ref bool suppress, ref bool tapXEnable, ref bool tapYEnable, ref bool tapZEnable)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.TapAxes);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                suppress = (map & (byte)TapAxesMap.Suppress) == (byte)TapAxesMap.Suppress;
                tapXEnable = (map & (byte)TapAxesMap.TapXEnable) == (byte)TapAxesMap.TapXEnable;
                tapYEnable = (map & (byte)TapAxesMap.TapYEnable) == (byte)TapAxesMap.TapYEnable;
                tapZEnable = (map & (byte)TapAxesMap.TapZEnable) == (byte)TapAxesMap.TapZEnable;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Suppress Bit
        /// Setting the suppress bit suppresses double tap detection if acceleration greater than the value in THRESH_TAP is present between
        /// taps.
        /// TAP_x Enable Bits
        /// A setting of 1 in the TAP_X enable, TAP_Y enable, or TAP_Z
        /// enable bit enables x-, y-, or z-axis participation in tap detection.
        /// A setting of 0 excludes the selected axis from participation in tap
        /// detection.
        /// </summary>
        /// <param name="suppress">Suppress.</param>
        /// <param name="tapXEnable">Tap X Enable.</param>
        /// <param name="tapYEnable">Tap Y Enable.</param>
        /// <param name="tapZEnable">Tap Z Enable.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetTapAxes(bool suppress, bool tapXEnable, bool tapYEnable, bool tapZEnable)
        {
            byte map = 0;

            if (suppress)
            {
                map |= (byte)TapAxesMap.Suppress;
            }

            if (tapXEnable)
            {
                map |= (byte)TapAxesMap.TapXEnable;
            }

            if (tapYEnable)
            {
                map |= (byte)TapAxesMap.TapYEnable;
            }

            if (tapZEnable)
            {
                map |= (byte)TapAxesMap.TapZEnable;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.ActInactCtl;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Act Tap Status

        /// <summary>
        /// <para>
        /// ACT_x Source and TAP_x Source Bits
        /// These bits indicate the first axis involved in a tap or activity event.
        /// A setting of 1 corresponds to involvement in the event, and a
        /// setting of 0 corresponds to no involvement. When new data is
        /// available, these bits are not cleared but are overwritten by the
        /// new data.The ACT_TAP_STATUS register should be read before
        /// clearing the interrupt.Disabling an axis from participation clears the
        /// corresponding source bit when the next activity or single tap/double
        /// tap event occurs.
        /// </para>
        /// <para>
        /// Asleep Bit
        /// A setting of 1 in the asleep bit indicates that the part is asleep,
        /// and a setting of 0 indicates that the part is not asleep.This bit
        /// toggles only if the device is configured for auto sleep. See the
        /// AUTO_SLEEP Bit section for more information on autosleep mode.
        /// </para>
        /// </summary>
        /// <param name="actXSource">Active X Source.</param>
        /// <param name="actYSource">Active Y Source.</param>
        /// <param name="actZSource">Active Z Source.</param>
        /// <param name="asleep">Asleep.</param>
        /// <param name="tapXSource">Tap X Source.</param>
        /// <param name="tapYSource">Tap Y Source.</param>
        /// <param name="tapZSource">Tap Z Source.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetActTapStatus(ref bool actXSource, ref bool actYSource, ref bool actZSource, ref bool asleep, ref bool tapXSource, ref bool tapYSource, ref bool tapZSource)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.ActTapStatus);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                actXSource = (map & (byte)ActTapStatusMap.ActXSource) == (byte)ActTapStatusMap.ActXSource;
                actYSource = (map & (byte)ActTapStatusMap.ActYSource) == (byte)ActTapStatusMap.ActYSource;
                actZSource = (map & (byte)ActTapStatusMap.ActZSource) == (byte)ActTapStatusMap.ActZSource;
                asleep = (map & (byte)ActTapStatusMap.Asleep) == (byte)ActTapStatusMap.Asleep;
                tapXSource = (map & (byte)ActTapStatusMap.TapXSource) == (byte)ActTapStatusMap.TapXSource;
                tapYSource = (map & (byte)ActTapStatusMap.TapYSource) == (byte)ActTapStatusMap.TapYSource;
                tapZSource = (map & (byte)ActTapStatusMap.TapZSource) == (byte)ActTapStatusMap.TapZSource;

                return true;
            }

            return false;
        }

        #endregion

        #region BW Rate

        /// <summary>
        /// LOW_POWER Bit
        /// A setting of 0 in the LOW_POWER bit selects normal operation,
        /// and a setting of 1 selects reduced power operation, which has
        /// somewhat higher noise(see the Power Modes section for details).
        /// Rate Bits
        /// These bits select the device bandwidth and output data rate(see
        /// Table 7 and Table 8 for details). The default value is 0x0A, which
        /// translates to a 100 Hz output data rate.An output data rate should
        /// be selected that is appropriate for the communication protocol and
        /// frequency selected. Selecting too high of an output data rate with a
        /// low communication speed results in samples being discarded.
        /// </summary>
        /// <param name="lowPower">Low Power.</param>
        /// <param name="rate">Rate.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetBWRate(ref bool lowPower, ref int rate)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.BwRate);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                lowPower = (map & (byte)BwRateMap.LowPower) == (byte)BwRateMap.LowPower;
                rate = (byte)(map & (byte)BwRateMap.Rate);

                return true;
            }

            return false;
        }

        /// <summary>
        /// LOW_POWER Bit
        /// A setting of 0 in the LOW_POWER bit selects normal operation,
        /// and a setting of 1 selects reduced power operation, which has
        /// somewhat higher noise(see the Power Modes section for details).
        /// Rate Bits
        /// These bits select the device bandwidth and output data rate(see
        /// Table 7 and Table 8 for details). The default value is 0x0A, which
        /// translates to a 100 Hz output data rate.An output data rate should
        /// be selected that is appropriate for the communication protocol and
        /// frequency selected. Selecting too high of an output data rate with a
        /// low communication speed results in samples being discarded.
        /// </summary>
        /// <param name="lowPower">Low Power.</param>
        /// <param name="rate">Rate.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetBWRates(bool lowPower, int rate)
        {
            byte map = 0;

            if (rate != 0)
            {
                map = (byte)(rate & (byte)BwRateMap.Rate);
            }

            if (lowPower)
            {
                map |= (byte)BwRateMap.LowPower;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.BwRate;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Power Control

        /// <summary>
        /// Link Bit
        /// A setting of 1 in the link bit with both the activity and inactivity functions enabled delays the start of the activity function until inactivity
        /// is detected.After activity is detected, inactivity detection begins,
        /// preventing the detection of activity.This bit serially links the activity
        /// and inactivity functions.When this bit is set to 0, the inactivity
        /// and activity functions are concurrent.Additional information can be
        /// found in the Link Mode section.
        /// When clearing the link bit, it is recommended that the part be
        /// placed into standby mode and then set back to measurement mode
        /// with a subsequent write. This is done to ensure that the device
        /// is properly biased if sleep mode is manually disabled; otherwise,
        /// the first few samples of data after the link bit is cleared may have
        /// additional noise, especially if the device was asleep when the bit
        /// was cleared.
        /// AUTO_SLEEP Bit
        /// If the link bit is set, a setting of 1 in the AUTO_SLEEP bit enables
        /// the auto-sleep functionality.In this mode, the ADXL343 automatically switches to sleep mode if the inactivity function is enabled
        /// and inactivity is detected(that is, when acceleration is below the
        /// THRESH_INACT value for at least the time indicated by TIME_INACT).If activity is also enabled, the ADXL343 automatically wakes
        /// up from sleep after detecting activity and returns to operation at the
        /// output data rate set in the BW_RATE register.A setting of 0 in the
        /// AUTO_SLEEP bit disables automatic switching to sleep mode.See
        /// the Sleep Bit section for more information on sleep mode.
        /// If the link bit is not set, the AUTO_SLEEP feature is disabled and
        /// setting the AUTO_SLEEP bit does not have an impact on device
        /// operation.Refer to the Link Bit section or the Link Mode section for
        /// more information on utilization of the link feature.
        /// When clearing the AUTO_SLEEP bit, it is recommended that the
        /// part be placed into standby mode and then set back to measurement mode with a subsequent write.This is done to ensure that
        /// the device is properly biased if sleep mode is manually disabled;
        /// otherwise, the first few samples of data after the AUTO_SLEEP bit
        /// is cleared may have additional noise, especially if the device was
        /// asleep when the bit was cleared.
        /// Measure Bit
        /// A setting of 0 in the measure bit places the part into standby
        /// mode, and a setting of 1 places the part into measurement mode.
        /// The ADXL343 powers up in standby mode with minimum power
        /// consumption.
        /// Sleep Bit
        /// A setting of 0 in the sleep bit puts the part into the normal mode
        /// of operation, and a setting of 1 places the part into sleep mode.
        /// Sleep mode suppresses DATA_READY, stops transmission of data
        /// to FIFO, and switches the sampling rate to one specified by the
        /// wakeup bits. In sleep mode, only the activity function can be used.
        /// When the DATA_READY interrupt is suppressed, the output data
        /// registers(Register 0x32 to Register 0x37) are still updated at the
        /// sampling rate set by the wakeup bits(D1:D0).
        /// When clearing the sleep bit, it is recommended that the part be
        /// placed into standby mode and then set back to measurement mode
        /// with a subsequent write. This is done to ensure that the device is
        /// properly biased if sleep mode is manually disabled; otherwise, the
        /// first few samples of data after the sleep bit is cleared may have
        /// additional noise, especially if the device was asleep when the bit
        /// was cleared.
        /// Wakeup Bits
        /// These bits control the frequency of readings in sleep mode as
        /// described here:  
        /// D1 D0 Frequency (Hz)
        ///     D1      D0      Setting
        ///     0       0       8
        ///     0       1       4
        ///     1       0       2
        ///     1       1       1
        ///     .
        /// </summary>
        /// <param name="link">Link.</param>
        /// <param name="autoSleep">Autosleep.</param>
        /// <param name="measure">Measure.</param>
        /// <param name="sleep">Sleep.</param>
        /// <param name="wakeUp">Wake Up.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetPowerControl(ref bool link, ref bool autoSleep, ref bool measure, ref bool sleep, ref byte wakeUp)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.PowerCtl);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                link = (map & (byte)PowerControlMap.Link) == (byte)PowerControlMap.Link;
                autoSleep = (map & (byte)PowerControlMap.AutoSleep) == (byte)PowerControlMap.AutoSleep;
                measure = (map & (byte)PowerControlMap.Measure) == (byte)PowerControlMap.Measure;
                sleep = (map & (byte)PowerControlMap.Sleep) == (byte)PowerControlMap.Sleep;
                wakeUp = (byte)(map & (byte)PowerControlMap.Wakeup);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Link Bit
        /// A setting of 1 in the link bit with both the activity and inactivity functions enabled delays the start of the activity function until inactivity
        /// is detected.After activity is detected, inactivity detection begins,
        /// preventing the detection of activity.This bit serially links the activity
        /// and inactivity functions.When this bit is set to 0, the inactivity
        /// and activity functions are concurrent.Additional information can be
        /// found in the Link Mode section.
        /// When clearing the link bit, it is recommended that the part be
        /// placed into standby mode and then set back to measurement mode
        /// with a subsequent write. This is done to ensure that the device
        /// is properly biased if sleep mode is manually disabled; otherwise,
        /// the first few samples of data after the link bit is cleared may have
        /// additional noise, especially if the device was asleep when the bit
        /// was cleared.
        /// AUTO_SLEEP Bit
        /// If the link bit is set, a setting of 1 in the AUTO_SLEEP bit enables
        /// the auto-sleep functionality.In this mode, the ADXL343 automatically switches to sleep mode if the inactivity function is enabled
        /// and inactivity is detected(that is, when acceleration is below the
        /// THRESH_INACT value for at least the time indicated by TIME_INACT).If activity is also enabled, the ADXL343 automatically wakes
        /// up from sleep after detecting activity and returns to operation at the
        /// output data rate set in the BW_RATE register.A setting of 0 in the
        /// AUTO_SLEEP bit disables automatic switching to sleep mode.See
        /// the Sleep Bit section for more information on sleep mode.
        /// If the link bit is not set, the AUTO_SLEEP feature is disabled and
        /// setting the AUTO_SLEEP bit does not have an impact on device
        /// operation.Refer to the Link Bit section or the Link Mode section for
        /// more information on utilization of the link feature.
        /// When clearing the AUTO_SLEEP bit, it is recommended that the
        /// part be placed into standby mode and then set back to measurement mode with a subsequent write.This is done to ensure that
        /// the device is properly biased if sleep mode is manually disabled;
        /// otherwise, the first few samples of data after the AUTO_SLEEP bit
        /// is cleared may have additional noise, especially if the device was
        /// asleep when the bit was cleared.
        /// Measure Bit
        /// A setting of 0 in the measure bit places the part into standby
        /// mode, and a setting of 1 places the part into measurement mode.
        /// The ADXL343 powers up in standby mode with minimum power
        /// consumption.
        /// Sleep Bit
        /// A setting of 0 in the sleep bit puts the part into the normal mode
        /// of operation, and a setting of 1 places the part into sleep mode.
        /// Sleep mode suppresses DATA_READY, stops transmission of data
        /// to FIFO, and switches the sampling rate to one specified by the
        /// wakeup bits. In sleep mode, only the activity function can be used.
        /// When the DATA_READY interrupt is suppressed, the output data
        /// registers(Register 0x32 to Register 0x37) are still updated at the
        /// sampling rate set by the wakeup bits(D1:D0).
        /// When clearing the sleep bit, it is recommended that the part be
        /// placed into standby mode and then set back to measurement mode
        /// with a subsequent write. This is done to ensure that the device is
        /// properly biased if sleep mode is manually disabled; otherwise, the
        /// first few samples of data after the sleep bit is cleared may have
        /// additional noise, especially if the device was asleep when the bit
        /// was cleared.
        /// Wakeup Bits
        /// These bits control the frequency of readings in sleep mode as
        /// described here:  
        /// D1 D0 Frequency (Hz)
        ///     D1      D0      Setting
        ///     0       0       8
        ///     0       1       4
        ///     1       0       2
        ///     1       1       1
        ///     .
        /// </summary>
        /// <param name="link">Link.</param>
        /// <param name="autoSleep">Autosleep.</param>
        /// <param name="measure">Measure.</param>
        /// <param name="sleep">Sleep.</param>
        /// <param name="wakeUp">Wakeup.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetPowerControl(bool link, bool autoSleep, bool measure, bool sleep, byte wakeUp)
        {
            byte map = 0;

            if (wakeUp != 0)
            {
                map = (byte)(wakeUp & (byte)PowerControlMap.Wakeup);
            }

            if (link)
            {
                map |= (byte)PowerControlMap.Link;
            }

            if (autoSleep)
            {
                map |= (byte)PowerControlMap.AutoSleep;
            }

            if (measure)
            {
                map |= (byte)PowerControlMap.Measure;
            }

            if (sleep)
            {
                map |= (byte)PowerControlMap.Sleep;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.PowerCtl;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region  Interrupt Enable

        /// <summary>
        /// Setting bits in this register to a value of 1 enables their respective
        /// functions to generate interrupts, whereas a value of 0 prevents
        /// the functions from generating interrupts.The DATA_READY, watermark, and overrun bits enable only the interrupt output; the
        /// functions are always enabled.It is recommended that interrupts be
        /// configured before enabling their outputs.
        /// </summary>
        /// <param name="dataReady">Data Ready.</param>
        /// <param name="singleTap">Single Tap.</param>
        /// <param name="doubleTap">Double Tap.</param>
        /// <param name="activity">Activity.</param>
        /// <param name="inactivity">Inactivity.</param>
        /// <param name="freeFall">Free Fall.</param>
        /// <param name="watermark">Watermark.</param>
        /// <param name="overrun">Overrun.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetInterruptEnable(ref bool dataReady, ref bool singleTap, ref bool doubleTap, ref bool activity, ref bool inactivity, ref bool freeFall, ref bool watermark, ref bool overrun)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.IntEnable);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                dataReady = (map & (byte)IntEnableMap.DataReady) == (byte)IntEnableMap.DataReady;
                singleTap = (map & (byte)IntEnableMap.SingleTap) == (byte)IntEnableMap.SingleTap;
                doubleTap = (map & (byte)IntEnableMap.DoubleTap) == (byte)IntEnableMap.DoubleTap;
                activity = (map & (byte)IntEnableMap.Activity) == (byte)IntEnableMap.Activity;
                inactivity = (map & (byte)IntEnableMap.Inactivity) == (byte)IntEnableMap.Inactivity;
                freeFall = (map & (byte)IntEnableMap.Freefall) == (byte)IntEnableMap.Freefall;
                watermark = (map & (byte)IntEnableMap.Watermark) == (byte)IntEnableMap.Watermark;
                overrun = (map & (byte)IntEnableMap.Overrun) == (byte)IntEnableMap.Overrun;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Setting bits in this register to a value of 1 enables their respective
        /// functions to generate interrupts, whereas a value of 0 prevents
        /// the functions from generating interrupts.The DATA_READY, watermark, and overrun bits enable only the interrupt output; the
        /// functions are always enabled.It is recommended that interrupts be
        /// configured before enabling their outputs.
        /// </summary>
        /// <param name="dataReady">Data Ready.</param>
        /// <param name="singleTap">Single Tap.</param>
        /// <param name="doubleTap">Double Tap.</param>
        /// <param name="activity">Activity.</param>
        /// <param name="inactivity">Inactivity.</param>
        /// <param name="freeFall">Free Fall.</param>
        /// <param name="watermark">Watermark.</param>
        /// <param name="overrun">Overrun.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetInterruptEnable(ref bool dataReady, ref bool singleTap, ref bool doubleTap, ref bool activity, ref bool inactivity, ref bool freeFall, ref bool watermark, ref bool overrun)
        {
            byte map = 0;

            if (dataReady)
            {
                map |= (byte)IntEnableMap.DataReady;
            }

            if (singleTap)
            {
                map |= (byte)IntEnableMap.SingleTap;
            }

            if (doubleTap)
            {
                map |= (byte)IntEnableMap.DoubleTap;
            }

            if (activity)
            {
                map |= (byte)IntEnableMap.Activity;
            }

            if (inactivity)
            {
                map |= (byte)IntEnableMap.Inactivity;
            }

            if (freeFall)
            {
                map |= (byte)IntEnableMap.Freefall;
            }

            if (watermark)
            {
                map |= (byte)IntEnableMap.Watermark;
            }

            if (overrun)
            {
                map |= (byte)IntEnableMap.Overrun;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.IntMap;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Interrupt Map

        /// <summary>
        /// Any bits set to 0 in this register send their respective interrupts to
        /// the INT1 pin, whereas bits set to 1 send their respective interrupts
        /// to the INT2 pin.All selected interrupts for a given pin are OR’ed.
        /// </summary>
        /// <param name="dataReady">Data Ready.</param>
        /// <param name="singleTap">Single Tap.</param>
        /// <param name="doubleTap">Double Tap.</param>
        /// <param name="activity">Activity.</param>
        /// <param name="inactivity">Inactivity.</param>
        /// <param name="freeFall">Free Fall.</param>
        /// <param name="watermark">Watermark.</param>
        /// <param name="overrun">Overrun.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetInterruptMap(ref bool dataReady, ref bool singleTap, ref bool doubleTap, ref bool activity, ref bool inactivity, ref bool freeFall, ref bool watermark, ref bool overrun)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.IntMap);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                dataReady = (map & (byte)IntEnableMap.DataReady) == (byte)IntEnableMap.DataReady;
                singleTap = (map & (byte)IntEnableMap.SingleTap) == (byte)IntEnableMap.SingleTap;
                doubleTap = (map & (byte)IntEnableMap.DoubleTap) == (byte)IntEnableMap.DoubleTap;
                activity = (map & (byte)IntEnableMap.Activity) == (byte)IntEnableMap.Activity;
                inactivity = (map & (byte)IntEnableMap.Inactivity) == (byte)IntEnableMap.Inactivity;
                freeFall = (map & (byte)IntEnableMap.Freefall) == (byte)IntEnableMap.Freefall;
                watermark = (map & (byte)IntEnableMap.Watermark) == (byte)IntEnableMap.Watermark;
                overrun = (map & (byte)IntEnableMap.Overrun) == (byte)IntEnableMap.Overrun;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Any bits set to 0 in this register send their respective interrupts to
        /// the INT1 pin, whereas bits set to 1 send their respective interrupts
        /// to the INT2 pin.All selected interrupts for a given pin are OR’ed.
        /// </summary>
        /// <param name="dataReady">Data Ready.</param>
        /// <param name="singleTap">Single Tap.</param>
        /// <param name="doubleTap">Double Tap.</param>
        /// <param name="activity">Activity.</param>
        /// <param name="inactivity">Inactivity.</param>
        /// <param name="freeFall">Free Fall.</param>
        /// <param name="watermark">Watermark.</param>
        /// <param name="overrun">Overrun.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetInterruptMap(ref bool dataReady, ref bool singleTap, ref bool doubleTap, ref bool activity, ref bool inactivity, ref bool freeFall, ref bool watermark, ref bool overrun)
        {
            byte map = 0;

            if (dataReady)
            {
                map |= (byte)IntEnableMap.DataReady;
            }

            if (singleTap)
            {
                map |= (byte)IntEnableMap.SingleTap;
            }

            if (doubleTap)
            {
                map |= (byte)IntEnableMap.DoubleTap;
            }

            if (activity)
            {
                map |= (byte)IntEnableMap.Activity;
            }

            if (inactivity)
            {
                map |= (byte)IntEnableMap.Inactivity;
            }

            if (freeFall)
            {
                map |= (byte)IntEnableMap.Freefall;
            }

            if (watermark)
            {
                map |= (byte)IntEnableMap.Watermark;
            }

            if (overrun)
            {
                map |= (byte)IntEnableMap.Overrun;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.IntEnable;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Data Format

        /// <summary>
        /// SELF_TEST Bit
        /// A setting of 1 in the SELF_TEST bit applies a self-test force to the
        /// sensor, causing a shift in the output data.A value of 0 disables the
        /// self-test force.
        /// SPI Bit
        /// A value of 1 in the SPI bit sets the device to 3-wire SPI mode, and a
        /// value of 0 sets the device to 4-wire SPI mode.
        /// INT_INVERT Bit
        /// A value of 0 in the INT_INVERT bit sets the interrupts to active
        /// high, and a value of 1 sets the interrupts to active low.
        /// FULL_RES Bit
        /// When this bit is set to a value of 1, the device is in full resolution
        /// mode, where the output resolution increases with the g range set
        /// by the range bits to maintain a 4 mg/LSB scale factor.When the
        /// FULL_RES bit is set to 0, the device is in 10-bit mode, and the
        /// range bits determine the maximum g range and scale factor.
        /// Justify Bit
        /// A setting of 1 in the justify bit selects left-justified (MSB) mode, and
        /// a setting of 0 selects right-justified mode with sign extension.
        /// Range Bits
        /// These bits set the g range as described in the table.
        /// D1      D0      g Range
        /// 0       0       ±2 g
        /// 0       1       ±4 g
        /// 1       0       ±8 g
        /// 1       1       ±16 g
        /// .
        /// </summary>
        /// <param name="selfTest">Self Test.</param>
        /// <param name="spi">SPI.</param>
        /// <param name="intInvert">Interrupt Invert.</param>
        /// <param name="fullRes">Full Resolution.</param>
        /// <param name="justify">Justify.</param>
        /// <param name="range">Range.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetDataFormat(ref bool selfTest, ref bool spi, ref bool intInvert, ref bool fullRes, ref bool justify, ref byte range)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.DataFormat);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                selfTest = (map & (byte)DataFormatMap.SelfTest) == (byte)DataFormatMap.SelfTest;
                spi = (map & (byte)DataFormatMap.Spi) == (byte)DataFormatMap.Spi;
                intInvert = (map & (byte)DataFormatMap.IntInvert) == (byte)DataFormatMap.IntInvert;
                fullRes = (map & (byte)DataFormatMap.FullRes) == (byte)DataFormatMap.FullRes;
                justify = (map & (byte)DataFormatMap.Justify) == (byte)DataFormatMap.Justify;
                range = (byte)(map & (byte)DataFormatMap.Range);

                return true;
            }

            return false;
        }

        /// <summary>
        /// SELF_TEST Bit
        /// A setting of 1 in the SELF_TEST bit applies a self-test force to the
        /// sensor, causing a shift in the output data.A value of 0 disables the
        /// self-test force.
        /// SPI Bit
        /// A value of 1 in the SPI bit sets the device to 3-wire SPI mode, and a
        /// value of 0 sets the device to 4-wire SPI mode.
        /// INT_INVERT Bit
        /// A value of 0 in the INT_INVERT bit sets the interrupts to active
        /// high, and a value of 1 sets the interrupts to active low.
        /// FULL_RES Bit
        /// When this bit is set to a value of 1, the device is in full resolution
        /// mode, where the output resolution increases with the g range set
        /// by the range bits to maintain a 4 mg/LSB scale factor.When the
        /// FULL_RES bit is set to 0, the device is in 10-bit mode, and the
        /// range bits determine the maximum g range and scale factor.
        /// Justify Bit
        /// A setting of 1 in the justify bit selects left-justified (MSB) mode, and
        /// a setting of 0 selects right-justified mode with sign extension.
        /// Range Bits
        /// These bits set the g range as described in the table.
        /// D1      D0      g Range
        /// 0       0       ±2 g
        /// 0       1       ±4 g
        /// 1       0       ±8 g
        /// 1       1       ±16 g
        /// .
        /// </summary>
        /// <param name="selfTest">Self Test.</param>
        /// <param name="spi">SPI.</param>
        /// <param name="intInvert">Interrupt Invert.</param>
        /// <param name="fullRes">Full Resolution.</param>
        /// <param name="justify">Justify.</param>
        /// <param name="range">Range.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetDataFormat(bool selfTest, bool spi, bool intInvert, bool fullRes, bool justify, byte range)
        {
            byte map = 0;

            if (range != 0)
            {
                map = (byte)(range & (byte)DataFormatMap.Range);
            }

            if (selfTest)
            {
                map |= (byte)DataFormatMap.SelfTest;
            }

            if (spi)
            {
                map |= (byte)DataFormatMap.Spi;
            }

            if (intInvert)
            {
                map |= (byte)DataFormatMap.IntInvert;
            }

            if (fullRes)
            {
                map |= (byte)DataFormatMap.FullRes;
            }

            if (justify)
            {
                map |= (byte)DataFormatMap.Justify;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.DataFormat;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Acceleration

        /// <summary>
        /// These six bytes (Register 0x32 to Register 0x37) are eight bits
        /// each and hold the output data for each axis.Register 0x32 and
        /// Register 0x33 hold the output data for the x-axis, Register 0x34 and
        /// Register 0x35 hold the output data for the y-axis, and Register 0x36
        /// and Register 0x37 hold the output data for the z-axis.The output
        /// data is twos complement, with DATAx0 as the least significant byte
        /// and DATAx1 as the most significant byte, where x represent X,
        /// Y, or Z. The DATA_FORMAT register (Address 0x31) controls the
        /// format of the data.It is recommended that a multiple-byte read of all
        /// registers be performed to prevent a change in data between reads
        /// of sequential registers.
        /// </summary>
        /// <param name="accel">Acceleration.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetAcceleration(ref Vector3 accel)
        {
            SpanByte readBuf = new byte[7];
            var res = i2c.WriteByte((byte)Register.X0);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                short accelerationX = BinaryPrimitives.ReadInt16LittleEndian(readBuf.Slice(0, 2));
                short accelerationY = BinaryPrimitives.ReadInt16LittleEndian(readBuf.Slice(2, 2));
                short accelerationZ = BinaryPrimitives.ReadInt16LittleEndian(readBuf.Slice(4, 2));

                accel.X = (float)accelerationX;
                accel.Y = (float)accelerationY;
                accel.Z = (float)accelerationZ;
               
                return true;
            }

            return false;
        }

        #endregion

        #region FIFO Control

        /// <summary>
        /// These bits set the FIFO mode, as described in the Table
        /// FIFO Modes
        ///     D7      D6      Mode        Function
        ///     0       0       Bypass      FIFO is bypassed.
        ///     0       1       FIFO        FIFO collects up to 32 values and then stops collecting data, collecting new data only when FIFO is not
        ///                                 full.
        ///     1       0       Stream      FIFO holds the last 32 data values. When FIFO is full,
        ///                                 the oldest data is overwritten with newer data.
        ///     1       1       Trigger     When triggered by the trigger bit, FIFO holds the last
        ///                                 data samples before the trigger event and then contin-
        ///                                 ues to collect data until full. New data is collected only
        ///                                 when FIFO is not full.
        /// Trigger Bit
        /// A value of 0 in the trigger bit links the trigger event of trigger mode
        /// to INT1, and a value of 1 links the trigger event to INT2.
        /// Samples Bits
        /// The function of these bits depends on the FIFO mode selected(see
        /// Table). Entering a value of 0 in the samples bits immediately
        /// sets the watermark status bit in the INT_SOURCE register, regardless of which FIFO mode is selected.Undesirable operation may
        /// occur if a value of 0 is used for the samples bits when trigger mode
        /// is used.
        ///     FIFO Mode       Samples Bits Function
        ///     Bypass          None.
        ///     FIFO            Specifies how many FIFO entries are needed to trigger a
        ///                     watermark interrupt.
        ///     Stream          Specifies how many FIFO entries are needed to trigger a
        ///                     watermark interrupt.
        ///     Trigger         Specifies how many FIFO samples are retained in the FIFO
        ///                     buffer before a trigger event.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <param name="trigger">Trigger.</param>
        /// <param name="samples">Samples.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetFIFOControl(ref FifoMode mode, ref bool trigger, ref int samples)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.BwRate);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                mode = (FifoMode)(byte)(map & (byte)FifoControlMap.Mode);
                trigger = (map & (byte)FifoControlMap.Trigger) == (byte)FifoControlMap.Trigger;
                samples = (byte)(map & (byte)FifoControlMap.Samples);

                return true;
            }

            return false;
        }

        /// <summary>
        /// These bits set the FIFO mode, as described in the Table
        /// FIFO Modes
        ///     D7      D6      Mode        Function
        ///     0       0       Bypass      FIFO is bypassed.
        ///     0       1       FIFO        FIFO collects up to 32 values and then stops collecting data, collecting new data only when FIFO is not
        ///                                 full.
        ///     1       0       Stream      FIFO holds the last 32 data values. When FIFO is full,
        ///                                 the oldest data is overwritten with newer data.
        ///     1       1       Trigger     When triggered by the trigger bit, FIFO holds the last
        ///                                 data samples before the trigger event and then contin-
        ///                                 ues to collect data until full. New data is collected only
        ///                                 when FIFO is not full.
        /// Trigger Bit
        /// A value of 0 in the trigger bit links the trigger event of trigger mode
        /// to INT1, and a value of 1 links the trigger event to INT2.
        /// Samples Bits
        /// The function of these bits depends on the FIFO mode selected(see
        /// Table). Entering a value of 0 in the samples bits immediately
        /// sets the watermark status bit in the INT_SOURCE register, regardless of which FIFO mode is selected.Undesirable operation may
        /// occur if a value of 0 is used for the samples bits when trigger mode
        /// is used.
        ///     FIFO Mode       Samples Bits Function
        ///     Bypass          None.
        ///     FIFO            Specifies how many FIFO entries are needed to trigger a
        ///                     watermark interrupt.
        ///     Stream          Specifies how many FIFO entries are needed to trigger a
        ///                     watermark interrupt.
        ///     Trigger         Specifies how many FIFO samples are retained in the FIFO
        ///                     buffer before a trigger event.
        /// </summary>
        /// <param name="mode">Mode.</param>
        /// <param name="trigger">Trigger.</param>
        /// <param name="samples">Samples.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool SetFIFOControl(FifoMode mode, bool trigger, int samples)
        {
            byte map = 0;

            if (mode != FifoMode.Bypass)
            {
                map = (byte)((byte)mode << 7);
            }

            if (samples != 0)
            {
                map |= (byte)((byte)samples & (byte)FifoControlMap.Samples);
            }

            if (trigger)
            {
                map |= (byte)FifoControlMap.Trigger;
            }

            SpanByte writeBuf = new byte[2];
            writeBuf[0] = (byte)Register.BwRate;
            writeBuf[1] = map;
            var res = i2c.Write(writeBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region FIFO Status

        /// <summary>
        /// FIFO_TRIG Bit
        /// A 1 in the FIFO_TRIG bit corresponds to a trigger event occurring,
        /// and a 0 means that a FIFO trigger event has not occurred.
        /// Entries Bits
        /// These bits report how many data values are stored in FIFO.Access
        /// to collect the data from FIFO is provided through the DATAX,
        /// DATAY, and DATAZ registers.FIFO reads must be done in burst or
        /// multiple-byte mode because each FIFO level is cleared after any
        /// read (single- or multiple-byte) of FIFO.FIFO stores a maximum of
        /// 32 entries, which equates to a maximum of 33 entries available at
        /// any given time because an additional entry is available at the output
        /// filter of the device.
        /// </summary>
        /// <param name="fifoTrigger">FIFO Trigger.</param>
        /// <param name="entries">Entries.</param>
        /// <returns>True if Full Transfer result, false if any other result.</returns>
        public bool GetFIFOStatus(ref bool fifoTrigger, ref int entries)
        {
            SpanByte readBuf = new byte[1];
            var res = i2c.WriteByte((byte)Register.FifoStatus);
            res = i2c.Read(readBuf);

            if (res.Status == I2cTransferStatus.FullTransfer)
            {
                byte map = readBuf[0];

                fifoTrigger = (map & (byte)FifoStatusMap.FifoTrig) == (byte)FifoStatusMap.FifoTrig;
                entries = (byte)(map & (byte)FifoStatusMap.Entries);

                return true;
            }

            return false;
        }

        #endregion
    }
}