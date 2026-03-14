// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Gc0308;

Debug.WriteLine("Hello GC0308 Camera!");

//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
// On the M5Stack CoreS3, the GC0308 is on the internal system I2C bus:
//   SDA = GPIO 12, SCL = GPIO 11
// Uncomment the lines below for ESP32:
// Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

using Gc0308 camera = new(I2cDevice.Create(new I2cConnectionSettings(1, Gc0308.DefaultI2cAddress)));

// Verify chip identity
byte chipId = camera.ChipId;
Debug.WriteLine($"GC0308 Chip ID: 0x{chipId:X2} (expected 0x{Gc0308.ExpectedChipId:X2})");

if (chipId != Gc0308.ExpectedChipId)
{
    Debug.WriteLine("WARNING: Unexpected chip ID. Check wiring and I2C address.");
}

// ── Configure the camera ──

// Set output format (default is YCbCr 4:2:2)
camera.OutputFormat = OutputFormat.Rgb565;
Debug.WriteLine("Output format set to RGB565");

// Set resolution to QVGA (320x240) for lower memory usage
camera.SetResolution(Resolution.Qvga320x240);
Debug.WriteLine("Resolution set to QVGA (320x240)");

// Adjust image quality settings
camera.Contrast = 0x40;     // 1.0x contrast (default)
camera.Saturation = 0x40;   // 1.0x saturation (default)
camera.AecTarget = 0x48;    // Default AEC target (also controls brightness)
Debug.WriteLine($"Contrast: 0x{camera.Contrast:X2}, Saturation: 0x{camera.Saturation:X2}, AEC Target: 0x{camera.AecTarget:X2}");

// Configure white balance
camera.SetWhiteBalance(WhiteBalanceMode.Auto);
Debug.WriteLine("White balance: Auto");

// Configure exposure
Debug.WriteLine($"Exposure mode: {camera.ExposureMode}");
Debug.WriteLine($"AEC target: 0x{camera.AecTarget:X2}");

// ── Demonstrate special effects ──
Debug.WriteLine("\n--- Special Effects Demo ---");
SpecialEffect[] effects = new SpecialEffect[]
{
    SpecialEffect.Normal,
    SpecialEffect.Grayscale,
    SpecialEffect.Sepia,
    SpecialEffect.Negative,
};

foreach (SpecialEffect effect in effects)
{
    camera.SetSpecialEffect(effect);
    Debug.WriteLine($"Effect: {effect}");
    Thread.Sleep(500);
}

// Reset to normal
camera.SetSpecialEffect(SpecialEffect.Normal);

// ── Demonstrate mirror/flip ──
Debug.WriteLine("\n--- Orientation Demo ---");
camera.Orientation = MirrorFlip.HorizontalMirror;
Debug.WriteLine($"Orientation: {camera.Orientation}");
Thread.Sleep(500);

camera.Orientation = MirrorFlip.None;
Debug.WriteLine($"Orientation: {camera.Orientation}");

// ── Test pattern mode ──
Debug.WriteLine("\n--- Test Pattern ---");
camera.SetTestPattern(true);
Debug.WriteLine("Test pattern enabled (color bars)");
Thread.Sleep(1000);

camera.SetTestPattern(false);
Debug.WriteLine("Test pattern disabled (normal camera)");

// ── Register dump for debugging ──
Debug.WriteLine("\n--- Key Register Values ---");
Debug.WriteLine($"  AAAA_EN (0x22):       0x{camera.ReadRegister(0x22):X2}");
Debug.WriteLine($"  Special effect (0x23):0x{camera.ReadRegister(0x23):X2}");
Debug.WriteLine($"  Output format (0x24): 0x{camera.ReadRegister(0x24):X2}");
Debug.WriteLine($"  Analog mode 1 (0x14): 0x{camera.ReadRegister(0x14):X2}");
Debug.WriteLine($"  Global gain (0x50):   0x{camera.ReadRegister(0x50):X2}");
Debug.WriteLine($"  AWB R/G/B gains:      0x{camera.ReadRegister(0x5A):X2} / 0x{camera.ReadRegister(0x5B):X2} / 0x{camera.ReadRegister(0x5C):X2}");
Debug.WriteLine($"  Saturation Cb (0xB1): 0x{camera.ReadRegister(0xB1):X2}");
Debug.WriteLine($"  Contrast (0xB3):      0x{camera.ReadRegister(0xB3):X2}");
Debug.WriteLine($"  AEC target Y (0xD3):  0x{camera.ReadRegister(0xD3):X2}");

Debug.WriteLine("\nGC0308 configuration complete.");
Debug.WriteLine("Note: Frame capture requires platform-specific DVP/camera controller support.");
Debug.WriteLine("On ESP32-S3, use the native camera driver to capture frames,");
Debug.WriteLine("then use CameraFrame class to process the pixel data.");

Thread.Sleep(Timeout.Infinite);
