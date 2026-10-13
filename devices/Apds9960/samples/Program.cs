// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Device.I2c;
using System.Drawing;
using System.Threading;
using Iot.Device.Apds9960;
using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(40, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(41, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Apds9960.DefaultI2cAddress);
using I2cDevice i2cDevice = new I2cDevice(settings);
using Apds9960 sensor = new Apds9960(i2cDevice);

sensor.ColorSensorEnabled = true;
sensor.ProximitySensorEnabled = true;
sensor.GestureSensorEnabled = true;

int measurementCycle = 4;
while (true)
{
    if (sensor.IsGestureAvailable)
    {
        Debug.WriteLine("Gesture mode active; color and proximity output paused.");
        Gesture gesture = sensor.ReadGesture();
        Debug.WriteLine("Gesture mode ended.");

        switch (gesture)
        {
            case Gesture.Up:
                Debug.WriteLine("Gesture=Up");
                break;
            case Gesture.Down:
                Debug.WriteLine("Gesture=Down");
                break;
            case Gesture.Left:
                Debug.WriteLine("Gesture=Left");
                break;
            case Gesture.Right:
                Debug.WriteLine("Gesture=Right");
                break;
        }

        continue;
    }

    measurementCycle++;
    if (measurementCycle >= 4)
    {
        measurementCycle = 0;
        if (sensor.IsColorDataReady)
        {
            Color color = sensor.ReadColorData();
            Debug.WriteLine($"Clear={color.A}, R={color.R}, G={color.G}, B={color.B}");
        }

        Debug.WriteLine($"Proximity={sensor.ReadProximity()}");
    }

    Thread.Sleep(50);
}
