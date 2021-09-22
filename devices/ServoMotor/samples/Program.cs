// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Pwm;
using System.Diagnostics;
using System.Threading;
using Iot.Device.ServoMotor;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("Hello Servo Motor!");

// When using an ESP32, you have to setup the pin function then create the PWM channel
Configuration.SetPinFunction(5, DeviceFunction.PWM1);

using PwmChannel pwmChannel = PwmChannel.CreateFromPin(5, 50);
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
    pulseWidthValue = 2200;
    servoMotor.Start();

    while (true)
    {
        pulseWidthValue = pulseWidthValue == 700 ? 2200 : 700;

        servoMotor.WritePulseWidth(pulseWidthValue);
        Debug.WriteLine($"Duty Cycle: {pwmChannel.DutyCycle * 100.0}%");
        Thread.Sleep(2000);
    }

    servoMotor.Stop();
}

void WriteAngle(PwmChannel pwmChannel, ServoMotor servoMotor)
{
    servoMotor.Start();

    while (true)
    {
        angleValue = angleValue == 0 ? 160 : 0;

        servoMotor.WriteAngle(angleValue);
        Debug.WriteLine($"Duty Cycle: {pwmChannel.DutyCycle * 100.0}%");
        Thread.Sleep(2000);
    }

    servoMotor.Stop();
}
