// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device;
using System.Device.Gpio;
using System.Threading;
using UnitsNet;

namespace Iot.Device.Drv8825
{
    /// <summary>
    /// Class for controlling Drv8825 stepper motor driver.
    /// </summary>
    public class Drv8825 : IDisposable
    {
        private readonly ushort _fullStepsPerRotation;
        private readonly GpioPin _stepPin;
        private readonly GpioPin _dirPin;
        private readonly GpioPin? _sleepPin;
        private readonly bool _shouldDisposeGpioController;
        private readonly MicrostepsController _microstepsController;

        private GpioController? _gpioController;

        /// <summary>
        /// Gets or sets the delay that allows the driver to recognize a step. By default, it is 5 microseconds.
        /// It is not recommended to set this delay less than 2 microseconds, 
        /// otherwise the motor will "miss"(skip) some steps.
        /// And it is not recommended to set this delay more than 50 microseconds,
        /// otherwise the morot will stop recognize steps.
        /// </summary>
        public TimeSpan StepDelay { get; set; } = TimeSpan.FromTicks(5 * 10L);

        /// <summary>
        /// Gets or sets delay between each step (or microstep).
        /// </summary>
        public TimeSpan DelayBetweenSteps { get; set; } = TimeSpan.Zero;

        /// <summary>
        /// Initializes a new instance of the <see cref="Drv8825" /> class.
        /// </summary>
        /// <param name="stepPin">Microcontroller pin connected to STEP driver pin. Used to set steps count.</param>
        /// <param name="dirPin">Microcontroller pin connected to DIR driver pin. Used to set rotation direction.</param>
        /// <param name="sleepPin">Microcontroller pin connected to SLP driver pin. Used to wake up and put the driver to sleep. Usually SLP need to connect with RST pin.</param>
        /// <param name="fullStepsPerRotation">How many steps your motor need to make full rotation. For example, Nema 17 takes 200 full steps to complete full rotation.</param>
        /// <param name="gpioController">GPIO controller. If it not passed, then it be created here.</param>
        /// <param name="shouldDisposeGpioController">True to dispose the Gpio Controller when this class wiill be disposed.</param>
        /// <param name="m0Pin">Microcontroller pin connected to M0 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        /// <param name="m1Pin">Microcontroller pin connected to M1 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        /// <param name="m2Pin">Microcontroller pin connected to M2 driver pin. Can be used to microsteps control. 0 if not connected.</param>
        public Drv8825(
            byte stepPin,
            byte dirPin,
            byte sleepPin = 0,
            ushort fullStepsPerRotation = 200,
            GpioController? gpioController = null,
            bool shouldDisposeGpioController = true,
            byte m0Pin = 0,
            byte m1Pin = 0,
            byte m2Pin = 0)
        {
            _fullStepsPerRotation = fullStepsPerRotation;
            _gpioController = gpioController ?? new GpioController();
            _shouldDisposeGpioController = shouldDisposeGpioController || gpioController is null;
            _stepPin = _gpioController.OpenPin(stepPin, PinMode.Output);
            _dirPin = _gpioController.OpenPin(dirPin, PinMode.Output);

            if (sleepPin > 0)
            {
                _sleepPin = _gpioController.OpenPin(sleepPin, PinMode.Output);
            }

            _microstepsController = new MicrostepsController(_gpioController, m0Pin, m1Pin, m2Pin);
        }

        /// <summary>
        /// Switch driver to working mode.
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws when sleep pin doesn't passed.</exception>
        public void WakeUp()
        {
            if (_sleepPin == null)
            {
                throw new InvalidOperationException("Sleep pin does not passed. Try to create driver class with sleepPin param");
            }

            _sleepPin.Write(PinValue.High);
        }

        /// <summary>
        /// Switch driver to sleep mode.
        /// </summary>
        /// <param name="millisecondsDelay">The number of milliseconds to wait before going to sleep. Use if you want give to driver time to process all previously sent steps.</param>
        /// <exception cref="InvalidOperationException">Throws when sleep pin doesn't passed.</exception>
        public void Sleep(int millisecondsDelay = 0)
        {
            if (_sleepPin == null)
            {
                throw new InvalidOperationException("Sleep pin does not passed. Try to create driver class with sleepPin param");
            }

            if (millisecondsDelay > 0)
            {
                Thread.Sleep(millisecondsDelay);
            }

            _sleepPin.Write(PinValue.Low);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="angle">Angle to rotate.</param>
        /// <param name="stepSize">Step size.</param>
        public void Rotate(Angle angle, StepSize stepSize = StepSize.FullStep)
        {
            if (angle.Degrees == 0)
            {
                return;
            }

            _dirPin.Write(angle.Degrees > 0 ? PinValue.High : PinValue.Low);
            var degreeForStepsCalculation = angle.Degrees < 0 ? -angle.Degrees : angle.Degrees;
            var pulses = degreeForStepsCalculation / 360 * _fullStepsPerRotation * (byte)stepSize;

            _microstepsController.Set(stepSize);
            Rotate((int)pulses);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="steps">Steps count.</param>
        /// <param name="direction">True to go forward, false to go back(or vice versa if you connect the motor in the opposite direction).</param>
        /// <param name="size">Step size.</param>
        public virtual void Rotate(
            int steps,
            Direction direction = Direction.Clockwise,
            StepSize size = StepSize.FullStep)
        {
            if (steps == 0)
            {
                return;
            }

            _microstepsController.Set(size);
            _dirPin.Write(direction == Direction.Clockwise ? PinValue.High : PinValue.Low);

            Rotate(steps);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (_shouldDisposeGpioController)
            {
                _gpioController?.Dispose();
            }

            _gpioController = null;
        }

        /// <summary>
        /// Controls the speed of rotation.
        /// </summary>
        private void SleepBetweenSteps()
        {
            if (DelayBetweenSteps == TimeSpan.Zero)
            {
                return;
            }

            Thread.Sleep(DelayBetweenSteps);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="steps">Steps count.</param>
        private void Rotate(int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                _stepPin.Write(PinValue.High);
                Thread.Sleep(StepDelay);
                _stepPin.Write(PinValue.Low);
                SleepBetweenSteps();
            }
        }
    }
}