//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Paj7620.Samples
{
    /// <summary>
    /// PAJ7620 sample entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Starts the sample loop and prints detected gestures.
        /// </summary>
        public static void Main()
        {
            // Configure I2C pins for your board before creating the device.
            Configuration.SetPinFunction(Gpio.IO05, DeviceFunction.I2C1_DATA);
            Configuration.SetPinFunction(Gpio.IO06, DeviceFunction.I2C1_CLOCK);

            I2cConnectionSettings settings = new(1, Paj7620.DefaultI2CAddress);

            using (Paj7620 sensor = new(I2cDevice.Create(settings)))
            {
                sensor.GestureDebounceMilliseconds = 500;

                sensor.Initialize();
                Debug.WriteLine("PAJ7620 initialized.");

                for (int i = 0; i < int.MaxValue; i++)
                {
                    if (sensor.TryReadGesture(out Gesture gesture))
                    {
                        Debug.WriteLine($"Gesture: {GetGestureName(gesture)}");
                    }

                    Thread.Sleep(100);
                }
            }
        }

        private static string GetGestureName(Gesture gesture)
        {
            switch (gesture)
            {
                case Gesture.Up:
                    return "Up";
                case Gesture.Down:
                    return "Down";
                case Gesture.Left:
                    return "Left";
                case Gesture.Right:
                    return "Right";
                case Gesture.Forward:
                    return "Forward";
                case Gesture.Backward:
                    return "Backward";
                case Gesture.Clockwise:
                    return "Clockwise";
                case Gesture.CounterClockwise:
                    return "CounterClockwise";
                case Gesture.Wave:
                    return "Wave";
                default:
                    return "None";
            }
        }
    }
}