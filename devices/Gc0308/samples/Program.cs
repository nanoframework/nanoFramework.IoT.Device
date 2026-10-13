// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Axp2101;
using Iot.Device.Gc0308;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

Debug.WriteLine("GC0308 sample adjusted for the M5Stack CoreS3.");

// CoreS3 internal I2C bus: official M5Stack pin map uses G12 SDA / G11 SCL.
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

EnableCoreS3InternalBusPower();
EnableCoreS3SensorPower();

if (!TryProbeI2cAddress(1, Gc0308.DefaultI2cAddress, out I2cTransferStatus gc0308ProbeStatus))
{
    Debug.WriteLine($"GC0308 probe failed at 0x{Gc0308.DefaultI2cAddress:X2}. I2C status: {gc0308ProbeStatus}");
    DumpPowerState();
    ScanI2cRange(1, 0x08, 0x77);
    // If all power rails are ON and the LTR-553 at 0x23 (same ribbon) responds but
    // 0x21 does not, the GC0308 chip itself is defective or not populated on this unit.
    // There is nothing further to configure — this is a hardware fault.
    Debug.WriteLine("DIAGNOSIS: power rails OK, ribbon OK (LTR-553 responded), bus OK.");
    Debug.WriteLine("GC0308 chip does not respond. Hardware defect or not populated on this unit.");
    while (true) { Thread.Sleep(1000); }
}

Debug.WriteLine($"GC0308 detected on I2C address 0x{Gc0308.DefaultI2cAddress:X2}.");

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

while (true)
{
    Thread.Sleep(1000);
}

static void EnableCoreS3SensorPower()
{
    using I2cDevice i2cAxp2101 = I2cDevice.Create(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
    using Axp2101 power = new(i2cAxp2101);

    byte chipId = power.GetChipId();
    Debug.WriteLine($"AXP2101 Chip ID: 0x{chipId:X2} (expected 0x{Axp2101.ChipId:X2})");
    if (chipId != Axp2101.ChipId)
    {
        throw new InvalidOperationException("AXP2101 chip ID mismatch. CoreS3 internal sensor rails were not configured.");
    }

    // Match M5Stack's CoreS3 power profile used by the official source.
    power.Aldo1Voltage = ElectricPotential.FromVolts(1.8);
    power.EnableAldo1();

    power.Aldo2Voltage = ElectricPotential.FromVolts(3.3);
    power.EnableAldo2();

    power.Aldo3Voltage = ElectricPotential.FromVolts(3.3);
    power.EnableAldo3();

    power.Aldo4Voltage = ElectricPotential.FromVolts(3.3);
    power.EnableAldo4();

    power.Bldo1Voltage = ElectricPotential.FromVolts(3.3);
    power.EnableBldo1();

    power.Bldo2Voltage = ElectricPotential.FromVolts(3.3);
    power.EnableBldo2();

    Debug.WriteLine("CoreS3 power rails enabled via AXP2101: ALDO1/2/3/4 + BLDO1/2.");
    Thread.Sleep(20);
}

static void EnableCoreS3InternalBusPower()
{
    const int aw9523Address = 0x58;
    const byte regPort0Output = 0x02;
    const byte regPort1Output = 0x03;
    // Direction registers: 0 = output, 1 = input. AW9523B defaults all pins to INPUT on
    // cold boot (registers 0x04/0x05 = 0xFF). BUS_EN and BOOST_EN must be configured as
    // outputs before writing the output registers, otherwise the pins are never driven.
    const byte regPort0Direction = 0x04;
    const byte regPort1Direction = 0x05;
    const byte busEnableMask = 0x02;
    const byte boostEnableMask = 0x80;

    using I2cDevice aw9523 = I2cDevice.Create(new I2cConnectionSettings(1, aw9523Address));

    // Configure BUS_EN (Port0 bit1) and BOOST_EN (Port1 bit7) as outputs.
    if (!TryClearBits(aw9523, regPort0Direction, busEnableMask, out I2cTransferStatus dir0Status))
    {
        throw new InvalidOperationException($"AW9523 Port0 direction config failed. Status: {dir0Status}");
    }

    if (!TryClearBits(aw9523, regPort1Direction, boostEnableMask, out I2cTransferStatus dir1Status))
    {
        throw new InvalidOperationException($"AW9523 Port1 direction config failed. Status: {dir1Status}");
    }

    if (!TrySetBits(aw9523, regPort0Output, busEnableMask, out I2cTransferStatus port0Status))
    {
        throw new InvalidOperationException($"AW9523 BUS_EN update failed. Status: {port0Status}");
    }

    if (!TrySetBits(aw9523, regPort1Output, boostEnableMask, out I2cTransferStatus port1Status))
    {
        throw new InvalidOperationException($"AW9523 BOOST_EN update failed. Status: {port1Status}");
    }

    Debug.WriteLine("CoreS3 internal bus power enabled via AW9523: BUS_EN + BOOST_EN.");
    Thread.Sleep(10);
}

static bool TrySetBits(I2cDevice device, byte register, byte bits, out I2cTransferStatus status)
{
    byte[] readCmd = new byte[] { register };
    byte[] value = new byte[1];
    I2cTransferResult read = device.WriteRead(readCmd, value);
    if (read.Status != I2cTransferStatus.FullTransfer)
    {
        status = read.Status;
        return false;
    }

    byte[] write = new byte[] { register, (byte)(value[0] | bits) };
    I2cTransferResult writeResult = device.Write(write);
    status = writeResult.Status;
    return writeResult.Status == I2cTransferStatus.FullTransfer;
}

static bool TryClearBits(I2cDevice device, byte register, byte bits, out I2cTransferStatus status)
{
    byte[] readCmd = new byte[] { register };
    byte[] value = new byte[1];
    I2cTransferResult read = device.WriteRead(readCmd, value);
    if (read.Status != I2cTransferStatus.FullTransfer)
    {
        status = read.Status;
        return false;
    }

    byte[] write = new byte[] { register, (byte)(value[0] & ~bits) };
    I2cTransferResult writeResult = device.Write(write);
    status = writeResult.Status;
    return writeResult.Status == I2cTransferStatus.FullTransfer;
}

static bool TryProbeI2cAddress(int busId, int address, out I2cTransferStatus status)
{
    using I2cDevice device = I2cDevice.Create(new I2cConnectionSettings(busId, address));
    // Use Read instead of WriteByte: sends address + READ bit and checks for ACK.
    // SCCB devices (like GC0308) may NACK write data bytes but will ACK the address
    // in a read transaction, so WriteByte can give a false-negative.
    byte[] buf = new byte[1];
    I2cTransferResult result = device.Read(buf);
    status = result.Status;
    return result.Status == I2cTransferStatus.FullTransfer;
}

static void ScanI2cRange(int busId, int startAddress, int endAddress)
{
    Debug.WriteLine($"I2C scan on bus {busId}: 0x{startAddress:X2}-0x{endAddress:X2}");

    bool anyDeviceFound = false;
    for (int address = startAddress; address <= endAddress; address++)
    {
        if (TryProbeI2cAddress(busId, address, out I2cTransferStatus status))
        {
            Debug.WriteLine($"  ACK at 0x{address:X2}");
            anyDeviceFound = true;
        }
        else if (status == I2cTransferStatus.PartialTransfer)
        {
            // Partial = address ACK but data NACK. Device is present.
            Debug.WriteLine($"  ACK(partial) at 0x{address:X2}");
            anyDeviceFound = true;
        }
    }

    if (!anyDeviceFound)
    {
        Debug.WriteLine("  No responding devices in scan range.");
    }
}

static void DumpPowerState()
{
    try
    {
        // ── AXP2101 power rail state ──────────────────────────────────────────────────
        using I2cDevice i2cAxp = I2cDevice.Create(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
        byte[] cmd1 = new byte[] { 0x90 };
        byte[] val1 = new byte[1];
        i2cAxp.WriteRead(cmd1, val1);
        byte ldoCtrl = val1[0];
        // Bit2=ALDO3(camera), Bit3=ALDO4(TF), expected: both set → 0x0C minimum
        Debug.WriteLine($"AXP2101 reg 0x90 (LDO enables): 0x{ldoCtrl:X2}");
        Debug.WriteLine($"  ALDO3 (camera rail):  {((ldoCtrl & 0x04) != 0 ? "ON" : "OFF ← CAMERA NOT POWERED")}");
        Debug.WriteLine($"  ALDO4 (TF card rail): {((ldoCtrl & 0x08) != 0 ? "ON" : "OFF")}");

        byte[] cmd2 = new byte[] { 0x94 };
        byte[] val2 = new byte[1];
        i2cAxp.WriteRead(cmd2, val2);
        // AXP2101 voltage formula: mV = (regVal + 5) * 100. For 3.3V: regVal = 28 = 0x1C
        Debug.WriteLine($"AXP2101 reg 0x94 (ALDO3 voltage): 0x{val2[0]:X2} = {(val2[0] + 5) * 100} mV (expected 3300 mV)");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"AXP2101 diagnostic failed: {ex.Message}");
    }

    try
    {
        // ── AW9523 GPIO state ─────────────────────────────────────────────────────────
        const int aw9523Address = 0x58;
        using I2cDevice i2cAw = I2cDevice.Create(new I2cConnectionSettings(1, aw9523Address));

        byte[] readDir0 = new byte[] { 0x04 };
        byte[] dir0 = new byte[1];
        i2cAw.WriteRead(readDir0, dir0);

        byte[] readDir1 = new byte[] { 0x05 };
        byte[] dir1 = new byte[1];
        i2cAw.WriteRead(readDir1, dir1);

        byte[] readOut0 = new byte[] { 0x02 };
        byte[] out0 = new byte[1];
        i2cAw.WriteRead(readOut0, out0);

        byte[] readOut1 = new byte[] { 0x03 };
        byte[] out1 = new byte[1];
        i2cAw.WriteRead(readOut1, out1);

        // Bit1 of Dir0 should be 0 (output); Bit1 of Out0 should be 1 (BUS_EN high)
        // Bit7 of Dir1 should be 0 (output); Bit7 of Out1 should be 1 (BOOST_EN high)
        Debug.WriteLine($"AW9523 Dir0 (0x04): 0x{dir0[0]:X2}  BUS_EN direction: {((dir0[0] & 0x02) == 0 ? "OUTPUT ✓" : "INPUT ← not driven")}");
        Debug.WriteLine($"AW9523 Out0 (0x02): 0x{out0[0]:X2}  BUS_EN level:     {((out0[0] & 0x02) != 0 ? "HIGH ✓" : "LOW")}");
        Debug.WriteLine($"AW9523 Dir1 (0x05): 0x{dir1[0]:X2}  BOOST_EN direction:{((dir1[0] & 0x80) == 0 ? "OUTPUT ✓" : "INPUT ← not driven")}");
        Debug.WriteLine($"AW9523 Out1 (0x03): 0x{out1[0]:X2}  BOOST_EN level:    {((out1[0] & 0x80) != 0 ? "HIGH ✓" : "LOW")}");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"ALDO4 diagnostic failed: {ex.Message}");
    }
}
