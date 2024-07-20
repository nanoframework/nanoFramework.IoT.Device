// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Atgm336h;
using System;
using System.Threading;
using nanoFramework.Hardware.Esp32;

var rxPin = 21;
var txPin = 19;
var comPort = "COM2";
Configuration.SetPinFunction(rxPin, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(txPin, DeviceFunction.COM2_TX);
var gpsModule = new Atgm336h(comPort);
gpsModule.LocationChanged += GpsModuleOnLocationChanged;
gpsModule.FixChanged += GpsModuleOnFixChanged;
gpsModule.ModeChanged += GpsModuleOnModeChanged;
gpsModule.Start();

Thread.Sleep(Timeout.Infinite);

void GpsModuleOnFixChanged(Fix fix)
{
    Console.WriteLine($"Fix changed to: {fix}");
}

void GpsModuleOnModeChanged(Mode mode)
{
    Console.WriteLine($"Mode changed to: {mode}");
}

void GpsModuleOnLocationChanged(GeoPosition position)
{
    Console.WriteLine($"Position: {position.Latitude},{position.Longitude}");
}