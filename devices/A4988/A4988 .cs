// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using UnitsNet.Units;

namespace Iot.Device.A4988
{
    /// <summary>
    /// Class for controlling A4988 stepper motor driver.
    /// </summary>
    public class A4988 : IDisposable
    {
        private readonly Microsteps _microsteps;
        private readonly ushort _fullStepsPerRotation;
        private readonly TimeSpan _sleepBetweenSteps;
        private readonly GpioPin _stepPin;
        private readonly GpioPin _dirPin;
        private readonly bool _shouldDispose;

        private GpioController _gpioController;

        /// <summary>
        /// Initializes a new instance of the <see cref="A4988" /> class.
        /// </summary>
        /// <param name="stepPin">Pin connected to STEP driver pin.</param>
        /// <param name="dirPin">Pin connected to DIR driver pin.</param>
        /// <param name="microsteps">Microsteps mode.</param>
        /// <param name="fullStepsPerRotation">Full steps per rotation.</param>
        /// <param name="sleepBetweenSteps">By changing this parameter you can set delay between steps and control the rotation speed (less time equals faster rotation).</param>
        /// <param name="gpioController">GPIO controller.</param>
        /// <param name="shouldDispose">True to dispose the Gpio Controller.</param>
        public A4988(
            byte stepPin, 
            byte dirPin, 
            Microsteps microsteps, 
            ushort fullStepsPerRotation, 
            TimeSpan sleepBetweenSteps,
            GpioController? gpioController = null, 
            bool shouldDispose = true)
        {
            _microsteps = microsteps;
            _fullStepsPerRotation = fullStepsPerRotation;
            _sleepBetweenSteps = sleepBetweenSteps;
            _gpioController = gpioController ?? new GpioController();
            _shouldDispose = shouldDispose || gpioController is null;
            _stepPin = _gpioController.OpenPin(stepPin, PinMode.Output);
            _dirPin = _gpioController.OpenPin(dirPin, PinMode.Output);
        }

        /// <summary>
        /// Controls the speed of rotation.
        /// </summary>
        protected virtual void SleepBetweenSteps()
        {
            if (_sleepBetweenSteps == TimeSpan.Zero)
            {
                return;
            }

            System.Threading.Thread.Sleep(_sleepBetweenSteps);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="angle">Angle to rotate.</param>
        public virtual void Rotate(UnitsNet.Angle angle)
        {
            if (angle.Degrees == 0)
            {
                return;
            }

            _dirPin.Write(angle.Degrees > 0 ? PinValue.High : PinValue.Low);
            var degreeForStepsCalculation = angle.Degrees < 0 ? -angle.Degrees : angle.Degrees;
            var steps = degreeForStepsCalculation / 360 * _fullStepsPerRotation * (byte)_microsteps;
            for (int x = 0; x < steps; x++)
            {
                _stepPin.Write(PinValue.High);
                SleepBetweenSteps();
                _stepPin.Write(PinValue.Low);
                SleepBetweenSteps();
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (_shouldDispose)
            {
                _gpioController?.Dispose();
                _gpioController = null;
            }
        }
    }
}