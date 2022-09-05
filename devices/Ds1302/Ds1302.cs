// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Device.Model;
using Iot.Device.Common;

namespace Iot.Device.Rtc
{
    /// <summary>
    /// Realtime Clock Ds1302.
    /// </summary>
    [Interface("Realtime Clock Ds1302")]
    public class Ds1302 : RtcBase
    {
        private readonly GpioPin _clockPin;
        private readonly GpioPin _communicationEnabledPin;
        private readonly GpioPin _dataPin;
        private readonly byte enableWrite = 0b1000_0000;
        private readonly byte disableWrite = 0b0000_0000;
        private readonly byte enableWriteOrRead = 0b1000_0001;
        private readonly bool _shouldDispose;
        private PinMode _currentDataPinDirection;
        private GpioController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="Ds1302"/> class.
        /// </summary>
        /// <param name="clockPin">Clock pin number.</param>
        /// <param name="dataPin">Data pin number.</param>
        /// <param name="communicationEnabledPin">Communication enabled pin number.</param>
        /// <param name="controller">Gpio controller.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        /// <exception cref="ArgumentNullException">When pin numbers are not valid.</exception>
        public Ds1302(int clockPin, int dataPin, int communicationEnabledPin, GpioController? controller = null, bool shouldDispose = true)
        {
            _controller = controller ?? new GpioController();
            _shouldDispose = shouldDispose || controller is null;

            if (clockPin < 0 || dataPin < 0 || communicationEnabledPin < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            _clockPin = _controller.OpenPin(clockPin, PinMode.Output);
            _communicationEnabledPin = _controller.OpenPin(communicationEnabledPin, PinMode.Output);
            _dataPin = _controller.OpenPin(dataPin, PinMode.Input);

            _communicationEnabledPin.Write(PinValue.Low);
            _clockPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Checks if the clock is halted.
        /// </summary>
        /// <returns><b>true</b> when clock is halted, else <b>false</b></returns>
        public bool IsHalted()
        {
            PrepareRead(Ds1302Registers.REG_SECONDS);
            byte seconds = ReadByte();
            EndTransmission();
            return (byte)(seconds & enableWrite) > 0;
        }

        /// <summary>
        /// Halts the clock.
        /// </summary>
        public void Halt()
        {
            PrepareWrite(Ds1302Registers.REG_SECONDS);
            WriteByte(enableWrite);
            EndTransmission();
        }

        /// <summary>
        /// Read time from the device.
        /// </summary>
        /// <returns>Time from the device.</returns>
        protected override DateTime ReadTime()
        {
            PrepareRead(Ds1302Registers.REG_BURST);
            var seconds = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0111_1111));
            var minutes = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0111_1111));
            var hours = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0011_1111));
            var days = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0011_1111));
            var months = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0001_1111));
            var dayOfWeek = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0000_0111));
            var years = NumberHelper.Bcd2Dec((byte)(ReadByte() & 0b0111_1111));

            EndTransmission();

            return new DateTime(2000 + years, months, days, hours, minutes, seconds, 0);
        }

        /// <summary>
        /// Sets date and time.
        /// </summary>
        /// <param name="time">Date and time.</param>
        protected override void SetTime(DateTime time)
        {
            PrepareWrite(Ds1302Registers.REG_WP);
            WriteByte(disableWrite);
            EndTransmission();

            PrepareWrite(Ds1302Registers.REG_BURST);
            WriteByte(NumberHelper.Dec2Bcd(time.Second));
            WriteByte(NumberHelper.Dec2Bcd(time.Minute));
            WriteByte(NumberHelper.Dec2Bcd(time.Hour));
            WriteByte(NumberHelper.Dec2Bcd(time.Day));
            WriteByte(NumberHelper.Dec2Bcd(time.Month));
            WriteByte(NumberHelper.Dec2Bcd((int)time.DayOfWeek));
            WriteByte(NumberHelper.Dec2Bcd(time.Year - 2000));

            WriteByte(enableWrite);
            EndTransmission();
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _dataPin?.Dispose();
            _clockPin?.Dispose();
            _communicationEnabledPin?.Dispose();
            if (_shouldDispose)
            {
                _controller?.Dispose();
                _controller = null;
            }
        }

        private byte ReadByte()
        {
            byte readValue = 0;
            for (byte i = 0; i < 8; i++)
            {
                if (_dataPin.Read() == PinValue.High)
                {
                    readValue |= (byte)(0x01 << i);
                }

                MoveToNextBit();
            }

            return readValue;
        }

        private void PrepareRead(Ds1302Registers ds1302Register)
        {
            SetDirectionOfDataPin(PinMode.Output);
            _communicationEnabledPin.Write(PinValue.High);
            WriteByte((byte)(enableWriteOrRead | (byte)ds1302Register));
            SetDirectionOfDataPin(PinMode.Input);
        }

        private void PrepareWrite(Ds1302Registers ds1302Register)
        {
            SetDirectionOfDataPin(PinMode.Output);
            _communicationEnabledPin.Write(PinValue.High);
            WriteByte((byte)(enableWrite | (byte)ds1302Register));
        }

        private void EndTransmission()
        {
            _communicationEnabledPin.Write(PinValue.Low);
        }

        private void SetDirectionOfDataPin(PinMode direction)
        {
            if (_currentDataPinDirection != direction)
            {
                _currentDataPinDirection = direction;
                _dataPin.SetPinMode(_currentDataPinDirection);
            }
        }

        private void WriteByte(byte register)
        {
            for (int i = 0; i < 8; i++)
            {
                _dataPin.Write((register & 0x01) > 0 ? PinValue.High : PinValue.Low);
                MoveToNextBit();
                register >>= 1;
            }
        }

        private void MoveToNextBit()
        {
            _clockPin.Write(PinValue.High);
            DelayHelper.DelayMicroseconds(1, true);
            _clockPin.Write(PinValue.Low);
            DelayHelper.DelayMicroseconds(1, true);
        }
    }
}