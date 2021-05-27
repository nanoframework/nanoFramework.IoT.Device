// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Pwm.Drivers;

namespace Iot.Device.DCMotor
{
    internal class DCMotor3Pin : DCMotor
    {
        private PwmChannel _pwm;
        private int _pin0;
        private int _pin1;
        private double _speed;

        public DCMotor3Pin(
            PwmChannel pwmChannel,
            int pin0,
            int pin1,
            GpioController? controller,
            bool shouldDispose)
            : base(controller ?? new GpioController(), controller is null ? true : shouldDispose)
        {
            if (pwmChannel is null)
            {
                throw new ArgumentNullException(nameof(pwmChannel));
            }

            _pwm = pwmChannel;

            _pin0 = pin0;
            _pin1 = pin1;

            _speed = 0;

            _pwm.Start();

            Controller.OpenPin(_pin0, PinMode.Output);
            Controller.Write(_pin0, PinValue.Low);

            Controller.OpenPin(_pin1, PinMode.Output);
            Controller.Write(_pin1, PinValue.Low);
        }

        /// <summary>
        /// Gets or sets the speed of the motor.
        /// Speed is a value from -1 to 1.
        /// 1 means maximum speed, signed value changes the direction.
        /// </summary>
        public override double Speed
        {
            get
            {
                return _speed;
            }
            set
            {
                double val = Math.Clamp(value, -1.0, 1.0);

                if (_speed == val)
                {
                    return;
                }

                if (val == 0.0)
                {
                    Controller.Write(_pin0, PinValue.Low);
                    Controller.Write(_pin1, PinValue.Low);
                }
                else if (val > 0.0)
                {
                    Controller.Write(_pin0, PinValue.Low);
                    Controller.Write(_pin1, PinValue.High);
                }
                else
                {
                    Controller.Write(_pin0, PinValue.High);
                    Controller.Write(_pin1, PinValue.Low);
                }

                _pwm.DutyCycle = Math.Abs(val);

                _speed = val;
            }
        }

        public override void Dispose()
        {
            _speed = 0.0;
            _pwm?.Dispose();
            _pwm = null!;
            base.Dispose();
        }
    }
}
