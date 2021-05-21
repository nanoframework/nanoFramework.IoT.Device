// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    internal class DCMotor2PinNoEnable : DCMotor
    {
        private PwmChannel _pwm;
        private int _pin1;
        private double _speed;

        public DCMotor2PinNoEnable(
            PwmChannel pwmChannel,
            int pin1,
            GpioController? controller,
            bool shouldDispose)
            : base(controller ?? ((pin1 == -1) ? null : new GpioController()), controller is null ? true : shouldDispose)
        {
            _pwm = pwmChannel;

            _pin1 = pin1;

            _speed = 0;

            _pwm.Start();

            if (_pin1 != -1)
            {
                Controller.OpenPin(_pin1, PinMode.Output);
                Controller.Write(_pin1, PinValue.Low);
            }
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from 0 to 1 or -1 to 1 if direction pin has been provided.
        /// 1 means maximum speed, signed value changes the direction.
        /// </summary>
        public override double Speed
        {
            get => _speed;
            set
            {
                double val = MathExtensions.Clamp(value, _pin1 != -1 ? -1.0 : 0.0, 1.0);

                if (_speed == val)
                {
                    return;
                }

                if (val >= 0.0)
                {
                    if (_pin1 != -1)
                    {
                        Controller.Write(_pin1, PinValue.Low);
                    }

                    _pwm.DutyCycle = val;
                }
                else
                {
                    if (_pin1 != -1)
                    {
                        Controller.Write(_pin1, PinValue.High);
                    }

                    _pwm.DutyCycle = 1.0 + val;
                }

                _speed = val;
            }
        }

        public override void Dispose()
        {
            _pwm?.Dispose();
            _pwm = null!;
            base.Dispose();
        }
    }
}
