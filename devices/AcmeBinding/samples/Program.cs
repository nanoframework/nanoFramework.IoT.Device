// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AcmeBinding;
using System.Diagnostics;
using System.Numerics;

// AcmeDevice is fully synthetic - no wiring, no bus setup, it just runs.
AcmeDevice device = new AcmeDevice();

Debug.WriteLine($"Firmware version: {device.FirmwareVersion}");
Debug.WriteLine($"Temperature: {device.Temperature}");
Debug.WriteLine($"Uptime: {device.GetUptimeSeconds()}s");

device.TryReadOrientation(out Vector3 orientation);
Debug.WriteLine($"Orientation: ({orientation.X}, {orientation.Y}, {orientation.Z})");

device.SamplingRateHz = 25;
device.SetThreshold(75.5);
Debug.WriteLine($"Sampling rate: {device.SamplingRateHz}Hz, threshold: {device.GetThreshold()}");

device.SetMode(AcmeMode.Active);
Debug.WriteLine($"Mode: {device.CurrentMode}");

device.Calibrate(offset: 0.5, scale: 1.02, iterations: 3);
Debug.WriteLine($"Calibrated with offset={device.LastCalibrationOffset}, scale={device.LastCalibrationScale}, iterations={device.LastCalibrationIterations}");

device.Beeper.Beep(200);
Debug.WriteLine($"Beeper is beeping: {device.Beeper.IsBeeping}");

device.Reset();
Debug.WriteLine($"Mode after reset: {device.CurrentMode}");
