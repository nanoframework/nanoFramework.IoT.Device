// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm;
using Iot.Device.ServoMotor;

Debug.WriteLine("Hello Servo Motor!");

using PwmChannel pwmChannel = PwmChannel.Create(0, 0, 50);
using ServoMotor servoMotor = new ServoMotor(
    pwmChannel,
    160,
    700,
    2200);

// Samples.
WritePulseWidth(pwmChannel, servoMotor);
// WriteAngle(pwmChannel, servoMotor);
// Methods
void WritePulseWidth(PwmChannel pwmChannel, ServoMotor servoMotor)
{
    servoMotor.Start();

    while (true)
    {
        Debug.WriteLine("Enter a pulse width in microseconds ('Q' to quit). ");
        string? pulseWidth = Console.ReadLine();

        if (pulseWidth?.ToUpper() is "Q" or null)
        {
            break;
        }

        if (!int.TryParse(pulseWidth, out int pulseWidthValue))
        {
            Debug.WriteLine($"Can not parse {pulseWidth}.  Try again.");
        }

        servoMotor.WritePulseWidth(pulseWidthValue);
        Debug.WriteLine($"Duty Cycle: {pwmChannel.DutyCycle * 100.0}%");
    }

    servoMotor.Stop();
}

void WriteAngle(PwmChannel pwmChannel, ServoMotor servoMotor)
{
    servoMotor.Start();

    while (true)
    {
        Debug.WriteLine("Enter an angle ('Q' to quit). ");
        string? angle = Console.ReadLine();

        if (angle?.ToUpper() is "Q" or null)
        {
            break;
        }

        if (!int.TryParse(angle, out int angleValue))
        {
            Debug.WriteLine($"Can not parse {angle}.  Try again.");
        }

        servoMotor.WriteAngle(angleValue);
        Debug.WriteLine($"Duty Cycle: {pwmChannel.DutyCycle * 100.0}%");
    }

    servoMotor.Stop();
}
