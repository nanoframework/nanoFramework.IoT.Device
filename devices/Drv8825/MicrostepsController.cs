// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;

namespace Iot.Device.Drv8825
{
    /// <summary>
    /// Class for microstepping control.
    /// </summary>
    internal class MicrostepsController
    {
        private readonly GpioPin? _m0Pin;
        private readonly GpioPin? _m1Pin;
        private readonly GpioPin? _m2Pin;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrostepsController" /> class.
        /// </summary>
        /// <param name="gpio">GPIO controller. If it not passed, then it be created here.</param>
        /// <param name="m0Pin">Microcontroller pin connected to M0 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        /// <param name="m1Pin">Microcontroller pin connected to M1 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        /// <param name="m2Pin">Microcontroller pin connected to M2 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        public MicrostepsController(
            GpioController gpio,
            byte m0Pin,
            byte m1Pin,
            byte m2Pin)
        {
            if (m0Pin > 0)
            {
                _m0Pin = gpio.OpenPin(m0Pin, PinMode.Output);
            }

            if (m1Pin > 0 && m1Pin != m0Pin)
            {
                _m1Pin = gpio.OpenPin(m1Pin, PinMode.Output);
            }

            if (m2Pin > 0 && m2Pin != m1Pin && m2Pin != m0Pin)
            {
                _m2Pin = gpio.OpenPin(m2Pin, PinMode.Output);
            }
        }

        /// <summary>
        /// Sets the values of M0, M1, M2 to go to the desired step size.
        /// </summary>
        /// <param name="size">Step size.</param>
        public void Set(StepSize size)
        {
            switch (size)
            {
                case StepSize.FullStep:
                    UpdateM0(PinValue.Low);
                    UpdateM1(PinValue.Low);
                    UpdateM2(PinValue.Low);
                    break;

                case StepSize.HalfStep:
                    UpdateM0(PinValue.High);
                    UpdateM1(PinValue.Low);
                    UpdateM2(PinValue.Low);
                    break;

                case StepSize.QuaterStep:
                    UpdateM0(PinValue.Low);
                    UpdateM1(PinValue.High);
                    UpdateM2(PinValue.Low);
                    break;

                case StepSize.EightStep:
                    UpdateM0(PinValue.High);
                    UpdateM1(PinValue.High);
                    UpdateM2(PinValue.Low);
                    break;

                case StepSize.SixteenStep:
                    UpdateM0(PinValue.Low);
                    UpdateM1(PinValue.Low);
                    UpdateM2(PinValue.High);
                    break;

                case StepSize.ThirtyTwoStep:
                    UpdateM0(PinValue.High);
                    UpdateM1(PinValue.Low);
                    UpdateM2(PinValue.High);
                    break;
            }
        }

        /// <summary>
        /// Sets the value of M0.
        /// </summary>
        /// <param name="value">Pin value(Low or High).</param>
        private void UpdateM0(PinValue value)
        {
            if (_m0Pin == null)
            {
                return;
            }

            _m0Pin.Write(value);
        }

        /// <summary>
        /// Sets the value of M1.
        /// </summary>
        /// <param name="value">Pin value(Low or High).</param>
        private void UpdateM1(PinValue value)
        {
            if (_m1Pin == null)
            {
                return;
            }
            
            _m1Pin.Write(value);
        }

        /// <summary>
        /// Sets the value of M2.
        /// </summary>
        /// <param name="value">Pin value(Low or High).</param>
        private void UpdateM2(PinValue value)
        {
            if (_m2Pin == null)
            {
                return;
            }

            _m2Pin.Write(value);
        }
    }
}
