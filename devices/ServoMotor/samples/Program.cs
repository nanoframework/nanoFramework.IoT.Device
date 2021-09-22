// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm;
using System.Diagnostics;
using Iot.Device.ServoMotor;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("Hello Servo Motor!");

// When using an ESP32, you have to setup the pin function then create the PWM channel
Configuration.SetPinFunction(21, DeviceFunction.PWM1);

using PwmChannel pwmChannel = PwmChannel.CreateFromPin(21, 50);
using ServoMotor servoMotor = new ServoMotor(
    pwmChannel,
    160,
    700,
    2200);

// Samples.
WritePulseWidth(pwmChannel, servoMotor);
// WriteAngle(pwmChannel, servoMotor);
// Methods
int pulseWidthValue;
int angleValue;
void WritePulseWidth(PwmChannel pwmChannel, ServoMotor servoMotor)
{
    servoMotor.Start();

    while (true)
    {
        Debug.WriteLine("Enter a pulse width in microseconds ('Q' to quit). ");
        pulseWidthValue = 500;

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
        angleValue = 70;

        servoMotor.WriteAngle(angleValue);
        Debug.WriteLine($"Duty Cycle: {pwmChannel.DutyCycle * 100.0}%");
    }

    servoMotor.Stop();
}
