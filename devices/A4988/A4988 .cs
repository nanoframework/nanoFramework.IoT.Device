using System;
using System.Device.Gpio;

namespace Iot.Device.A4988
{
    /// <summary>
    /// Class for controlling A4988 stepper motor driver
    /// </summary>
    public class A4988 : IDisposable
    {
        private readonly Microsteps microsteps;
        private readonly ushort fullStepsPerRotation;
        private readonly TimeSpan sleepBetweenSteps;
        private readonly GpioPin stepPin;
        private readonly GpioPin dirPin;
        private readonly bool shouldDispose;

        private GpioController gpioController;


        /// <summary>
        /// Initialize a A4988 class.
        /// </summary>
        /// <param name="stepPin">Pin connected to STEP driver pin</param>
        /// <param name="dirPin">Pin connected to DIR driver pin</param>
        /// <param name="microsteps">Microsteps mode</param>
        /// <param name="fullStepsPerRotation">Full steps per rotation</param>
        /// <param name="sleepBetweenSteps">By changing this parameter you can set delay between steps and controll the rotation speed (less time equals faster rotation)</param>
        /// <param name="gpioController">GPIO controller</param>
        public A4988(byte stepPin, byte dirPin, Microsteps microsteps, ushort fullStepsPerRotation, TimeSpan sleepBetweenSteps, 
            GpioController gpioController)
        {
            this.microsteps = microsteps;
            this.fullStepsPerRotation = fullStepsPerRotation;
            this.sleepBetweenSteps = sleepBetweenSteps;
            this.gpioController = gpioController;
            this.stepPin = this.gpioController.OpenPin(stepPin, PinMode.Output);
            this.dirPin = this.gpioController.OpenPin(dirPin, PinMode.Output);
        }

        /// <summary>
        /// Initialize a A4988 class.
        /// </summary>
        /// <param name="stepPin">Pin connected to STEP driver pin</param>
        /// <param name="dirPin">Pin connected to DIR driver pin</param>
        /// <param name="microsteps">Microsteps mode</param>
        /// <param name="fullStepsPerRotation">Full steps per rotation</param>
        /// <param name="sleepBetweenSteps">By changing this parameter you can set delay between steps and controll the rotation speed (less time equals faster rotation)</param>
        public A4988(byte stepPin, byte dirPin, Microsteps microsteps, ushort fullStepsPerRotation, TimeSpan sleepBetweenSteps) 
            : this(stepPin, dirPin, microsteps, fullStepsPerRotation, sleepBetweenSteps, new GpioController())
        {
            shouldDispose = true;
        }

        /// <summary>
        /// Controls the speed of rotation.
        /// </summary>
        protected virtual void SleepBetweenSteps()
        {
            if (sleepBetweenSteps == TimeSpan.Zero)
                return;

            System.Threading.Thread.Sleep(sleepBetweenSteps);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="degree">Rotation degree</param>
        /// <param name="rotation">Direction of rotation</param>
        public virtual void Rotate(ushort degree, bool rotation)
        {
            dirPin.Write(rotation ? PinValue.High : PinValue.Low);
            var steps = (double)degree / 360 * fullStepsPerRotation * ConvertMicrostepsToValue();
            for (int x = 0; x < steps; x++)
            {
                stepPin.Write(PinValue.High);
                SleepBetweenSteps();
                stepPin.Write(PinValue.Low);
                SleepBetweenSteps();
            }
        }

        private byte ConvertMicrostepsToValue()
        {
            return (byte)microsteps;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            if (shouldDispose)
            {
                gpioController?.Dispose();
                gpioController = null!;
            }
        }
    }
}