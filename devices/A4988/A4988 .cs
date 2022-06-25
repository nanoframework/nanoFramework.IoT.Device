using System;
using System.Device.Gpio;

namespace A4988
{
    public class A4988
    {
        public Microsteps Microsteps { get; }
        public ushort FullStepsPerRotation { get; }

        private readonly TimeSpan sleepBetweenSteps;
        private readonly GpioPin stepPin;
        private readonly GpioPin dirPin;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stepPin"></param>
        /// <param name="dirPin"></param>
        /// <param name="microsteps"></param>
        /// <param name="fullStepsPerRotation"></param>
        /// <param name="sleepBetweenSteps">By changing this parameter you can set delay between steps and controll the rotation speed (less time equals faster rotation)</param>
        /// <param name="gpioController"></param>
        public A4988(byte stepPin, byte dirPin, Microsteps microsteps, ushort fullStepsPerRotation, TimeSpan sleepBetweenSteps, GpioController gpioController)
        {
            Microsteps = microsteps;
            FullStepsPerRotation = fullStepsPerRotation;
            this.sleepBetweenSteps = sleepBetweenSteps;
            this.stepPin = gpioController.OpenPin(stepPin, PinMode.Output);
            this.dirPin = gpioController.OpenPin(dirPin, PinMode.Output);
        }

        protected virtual void SleepBetweenSteps()
        {
            if (sleepBetweenSteps == TimeSpan.Zero)
                return;

            System.Threading.Thread.Sleep(sleepBetweenSteps);
        }

        /// <summary>
        /// Rotates a stepper motor.
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="rotation"></param>
        public virtual void Rotate(ushort degree, bool rotation)
        {
            dirPin.Write(rotation ? PinValue.High : PinValue.Low);
            var steps = (double)degree / 360 * FullStepsPerRotation * ConvertMicrostepsToValue();
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
            return (byte)Microsteps;
        }
    }
}