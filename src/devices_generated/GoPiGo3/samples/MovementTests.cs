// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Threading;
using Iot.Device.GoPiGo3.Models;
using Iot.Device.GoPiGo3.Movements;

namespace GoPiGo3.Samples
{
    public partial class Program
    {
        private static void TestMotorTacho()
        {
            Motor motor = new Motor(_goPiGo3, MotorPort.MotorLeft);
            Debug.WriteLine($"Test on Motor class with motor on {motor.Port}.");
            motor.SetSpeed(10);
            motor.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();
            long initialTick = stopwatch.ElapsedTicks;
            double desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            Debug.WriteLine("Increase speed on the motor during 10 seconds.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.Write($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            motor.SetPolarity(Polarity.OppositeDirection);
            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            Debug.WriteLine("Decrease speed on the motor during 10 seconds.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Console.Write($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(200);
                motor.SetSpeed(motor.GetSpeed() + 10);
            }

            desiredTicks = 10000.0 / 1000.0 * Stopwatch.Frequency;
            finalTick = stopwatch.ElapsedTicks + desiredTicks;
            int pos = 0;
            Debug.WriteLine("Set the motor to the 0 position.");
            while (stopwatch.ElapsedTicks < finalTick)
            {
                Debug.WriteLine($"Encoder: {motor.GetTachoCount()}");
                Console.CursorLeft = 0;
                Thread.Sleep(2000);
                motor.SetTachoCount(pos);
            }

            motor.Stop();
        }

        private static void Testvehicle()
        {
            Debug.WriteLine("vehicle drive test using Motor left, Motor right, not inverted direction.");
            Vehicle veh = new Vehicle(_goPiGo3);
            veh.DirectionOpposite = true;
            Debug.WriteLine("Driving backward");
            veh.Backward(30, 5000);
            Debug.WriteLine("Driving forward");
            veh.Forward(30, 5000);
            Debug.WriteLine("Turning left");
            veh.TrunLeftTime(30, 5000);
            Debug.WriteLine("Turning right");
            veh.TrunRightTime(30, 5000);
            Debug.WriteLine("Turning left");
            veh.TurnLeft(30, 180);
            Debug.WriteLine("Turning right");
            veh.TurnRight(30, 180);
        }
    }
}
