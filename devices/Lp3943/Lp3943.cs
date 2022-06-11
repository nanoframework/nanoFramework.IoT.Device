// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Threading;

namespace Iot.Device.Lp3943
{
    /// <summary>
    /// 16-Channel LED-Driver Lp3943
    /// </summary>
    public class Lp3943 : IDisposable
    {
        /// <summary>
        /// Lp3943 I2C Address
        /// </summary>
        public const byte DefaultI2cAddress = 0x60;

        private readonly int _pinReset;
        private readonly GpioController _controller;
        private readonly I2cDevice _i2cDevice;
        private readonly bool _shouldDispose;

        private readonly LedState[] _ledStates;

        /// <summary>
        /// Creates a new instance of the Lp3943
        /// </summary>
        /// <param name="i2CDevice">The I2C device used for communication</param>
        /// <param name="pinReset">the reset pin for the hardware reset</param>
        /// <param name="gpioController">A GpioController for the hardware reset</param>
        /// <param name="shouldDispose">True to dispose the GpioController</param>
        public Lp3943(I2cDevice i2CDevice, int pinReset = -1, GpioController gpioController = null, bool shouldDispose = true)
        {
            _pinReset = pinReset;
            _shouldDispose = shouldDispose || gpioController == null;
            _controller = gpioController ?? new GpioController();
            _i2cDevice = i2CDevice ?? throw new ArgumentException(nameof(i2CDevice));
            _ledStates = new LedState[16];

            if (pinReset >= 0)
            {
                _controller.OpenPin(_pinReset, PinMode.Output);
            }

            Reset();
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Dispose()
        {
            if (_pinReset >= 0)
            {
                _controller?.ClosePin(_pinReset);
            }

            if (_shouldDispose)
            {
                _controller?.Dispose();
            }
        }

        private void SetFrequency(DimRegister dimRegister, float period)
        {
            // the frequency gets calculated with the following formula:
            // period = (PSCx + 1) / 160
            // in reverse this results in:
            var toWrite = (byte)(period * 160 - 1);
            var register = dimRegister switch
            {
                Device.Lp3943.DimRegister.Dim0 => Register.Psc0,
                Device.Lp3943.DimRegister.Dim1 => Register.Psc1,
            };

            WriteToRegister(register, toWrite);
        }

        private void SetDimPercentage(DimRegister dimRegister, int percentage)
        {
            // converts the percentage into a 8 bit number
            var toWrite = (byte)((float)percentage / 100 * 256);
            var register = dimRegister switch
            {
                Device.Lp3943.DimRegister.Dim0 => Register.Pwm0,
                Device.Lp3943.DimRegister.Dim1 => Register.Pwm1,
            };

            WriteToRegister(register, toWrite);
        }

        private void WriteToRegister(Register register, byte data)
        {
            var message = new byte[2];
            message[0] = (byte)register;
            message[1] = data;
            _i2cDevice.Write(message);
        }

        private byte FillSelectorRegister(LedState led0, LedState led1, LedState led2, LedState led3)
        {
            byte result = 0x00;
            result |= (byte)(((byte)led3 & 0x03) << 6);
            result |= (byte)(((byte)led2 & 0x03) << 4);
            result |= (byte)(((byte)led1 & 0x03) << 2);
            result |= (byte)((byte)led0 & 0x03);
            return result;
        }

        /// <summary>
        /// Sets the dimming on a register
        /// </summary>
        /// <param name="register">register to dim</param>
        /// <param name="frequency">frequency in Hz</param>
        /// <param name="dimPercentage">percentage</param>
        /// <exception cref="ArgumentOutOfRangeException">thrown when either frequency or dimPercentage is out of range</exception>
        public void DimRegister(DimRegister register, int frequency, int dimPercentage)
        {
            if (frequency is > 160 or < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(frequency), "The frequency has to be between 1 and 160 Hz.");
            }

            if (dimPercentage is > 100 or < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimPercentage));
            }

            SetFrequency(register, 1.0f / frequency);
            SetDimPercentage(register, dimPercentage);
        }

        private void Update()
        {
            var ls0 = FillSelectorRegister(_ledStates[0], _ledStates[1], _ledStates[2], _ledStates[3]);
            var ls1 = FillSelectorRegister(_ledStates[4], _ledStates[5], _ledStates[6], _ledStates[7]);
            var ls2 = FillSelectorRegister(_ledStates[8], _ledStates[9], _ledStates[10], _ledStates[11]);
            var ls3 = FillSelectorRegister(_ledStates[12], _ledStates[13], _ledStates[14], _ledStates[15]);

            WriteToRegister(Register.Ls0, ls0);
            WriteToRegister(Register.Ls1, ls1);
            WriteToRegister(Register.Ls2, ls2);
            WriteToRegister(Register.Ls3, ls3);
        }

        /// <summary>
        /// Resets the Lp3943 chip. This only has effect if you set a proper Reset pin.
        /// </summary>
        public void Reset()
        {
            if (_pinReset >= 0)
            {
                _controller.Write(_pinReset, PinValue.Low);
                Thread.Sleep(5);
                _controller.Write(_pinReset, PinValue.High);
            }
        }

        /// <summary>
        /// Sets led to assigned mode
        /// </summary>
        /// <param name="led">led to assign</param>
        /// <param name="ledState">state to give the led</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the value for led is not between 0 and 16</exception>
        public void SetLed(int led, LedState ledState)
        {
            if (led is > 16 or < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(led));
            }

            _ledStates[led] = ledState;
            Update();
        }

        /// <summary>
        /// Sets the LEDs in the array to the assigned mode
        /// </summary>
        /// <param name="leds">LEDs to assign</param>
        /// <param name="ledState">state to give the LEDs</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the values in the array is not between 0 and 16</exception>
        /// <exception cref="ArgumentNullException">Thrown when leds is not initialized</exception>
        public void SetLed(int[] leds, LedState ledState)
        {
            if (leds is null)
            {
                throw new ArgumentNullException(nameof(leds));
            }

            foreach (var led in leds)
            {
                if (led is <= 15 and >= 0)
                {
                    _ledStates[led] = ledState;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(leds), $"{led} is out of range");
                }
            }

            Update();
        }
    }
}
