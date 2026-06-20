// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Axp2101;
using Iot.Device.Ltr553AlsWa;
using nanoFramework.Hardware.Esp32;
using UnitsNet;

Debug.WriteLine("Hello LTR-553ALS-WA!");

//////////////////////////////////////////////////////////////////////
// When connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus.
// On the M5Stack CoreS3, the LTR-553ALS-WA is on the internal I2C bus.
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

EnableCoreS3InternalBusPower();

// CoreS3 note: the LTR553 is on rails controlled by AXP2101.
// If those rails are off, the sensor will read 0x00 IDs and never become data-ready.
EnableCoreS3SensorPower();
if (!DiagnoseLtrLink())
{
    Debug.WriteLine("LTR553 did not ACK on I2C address 0x23. Aborting driver init.");
    Debug.WriteLine("Check CoreS3 camera/proximity ribbon cable and board hardware revision.");
    Thread.Sleep(Timeout.Infinite);
}

using Ltr553AlsWa sensor = new(I2cDevice.Create(new I2cConnectionSettings(1, Ltr553AlsWa.DefaultI2cAddress)));
byte partId = sensor.PartId;
byte manufacturerId = sensor.ManufacturerId;
Debug.WriteLine($"Part ID: 0x{partId:X2}, Manufacturer ID: 0x{manufacturerId:X2}");
if (manufacturerId != Ltr553AlsWa.ExpectedManufacturerId)
{
    Debug.WriteLine($"Warning: unexpected manufacturer ID. Expected 0x{Ltr553AlsWa.ExpectedManufacturerId:X2}.");
}

// Configure ALS settings
sensor.AlsGain = AlsGain.Gain1X;
sensor.AlsIntegrationTime = AlsIntegrationTime.Integration100Ms;
sensor.AlsMeasurementRate = AlsMeasurementRate.Rate500Ms;

// Configure PS LED settings
sensor.LedPulseFrequency = LedPulseFrequency.Frequency60kHz;
sensor.LedDutyCycle = LedDutyCycle.DutyCycle100Percent;
sensor.LedPeakCurrent = LedPeakCurrent.Current100mA;
sensor.LedPulseCount = 1;

// Enable both proximity and ambient light sensors
sensor.PsEnabled = true;
sensor.AlsEnabled = true;

// Allow the sensor to take a first measurement after mode changes.
Thread.Sleep(600);

while (true)
{
    try
    {
        bool psReady = sensor.IsPsDataReady();
        bool alsReady = sensor.IsAlsDataReady();

        if (psReady && alsReady)
        {
            ushort proximity = sensor.GetProximity(out bool saturated);
            sensor.GetAlsData(out ushort ch0, out ushort ch1);
            Debug.WriteLine($"Proximity: {proximity}{(saturated ? " [SATURATED]" : string.Empty)}, ALS CH0 (visible+IR): {ch0}, ALS CH1 (IR): {ch1}");
        }
        else
        {
            Debug.WriteLine($"Waiting for fresh data... PS ready: {psReady}, ALS ready: {alsReady}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception: {ex.Message}");
    }

    Thread.Sleep(500);
}

static void EnableCoreS3SensorPower()
{
    using I2cDevice i2cAxp2101 = I2cDevice.Create(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
    using Axp2101 power = new(i2cAxp2101);

    byte chipId = power.GetChipId();
    Debug.WriteLine($"AXP2101 Chip ID: 0x{chipId:X2} (expected 0x{Axp2101.ChipId:X2})");
    if (chipId != Axp2101.ChipId)
    {
        Debug.WriteLine("Warning: AXP2101 chip ID mismatch. Skipping power-rail setup.");
        return;
    }

    // Match vendor CoreS3 power profile for internal peripherals.
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

    Debug.WriteLine("CoreS3 power rails enabled: ALDO1/2/3/4 + BLDO1/2.");
    Thread.Sleep(20);
}

static void EnableCoreS3InternalBusPower()
{
    const int aw9523Address = 0x58;
    const byte regPort0Output = 0x02;
    const byte regPort1Output = 0x03;
    const byte busEnableMask = 0x02; // BUS_EN
    const byte boostEnableMask = 0x80; // BOOST_EN

    using I2cDevice aw9523 = I2cDevice.Create(new I2cConnectionSettings(1, aw9523Address));

    if (!TrySetBits(aw9523, regPort0Output, busEnableMask, out I2cTransferStatus port0Status))
    {
        Debug.WriteLine($"Warning: AW9523 BUS_EN update failed. Status: {port0Status}");
        return;
    }

    if (!TrySetBits(aw9523, regPort1Output, boostEnableMask, out I2cTransferStatus port1Status))
    {
        Debug.WriteLine($"Warning: AW9523 BOOST_EN update failed. Status: {port1Status}");
        return;
    }

    Debug.WriteLine("CoreS3 internal bus power enabled: AW9523 BUS_EN + BOOST_EN.");
    Thread.Sleep(10);
}

static bool DiagnoseLtrLink()
{
    using I2cDevice ltr = I2cDevice.Create(new I2cConnectionSettings(1, Ltr553AlsWa.DefaultI2cAddress));
    Debug.WriteLine("LTR553 low-level probe on I2C1 @ 0x23:");
    I2cTransferStatus s0 = ReadAndLog(ltr, 0x80, "ALS_CONTR");
    I2cTransferStatus s1 = ReadAndLog(ltr, 0x81, "PS_CONTR");
    I2cTransferStatus s2 = ReadAndLog(ltr, 0x86, "PART_ID");
    I2cTransferStatus s3 = ReadAndLog(ltr, 0x87, "MANUFAC_ID");
    I2cTransferStatus s4 = ReadAndLog(ltr, 0x8C, "ALS_PS_STATUS");

    return s0 == I2cTransferStatus.FullTransfer
        || s1 == I2cTransferStatus.FullTransfer
        || s2 == I2cTransferStatus.FullTransfer
        || s3 == I2cTransferStatus.FullTransfer
        || s4 == I2cTransferStatus.FullTransfer;
}

static I2cTransferStatus ReadAndLog(I2cDevice device, byte register, string name)
{
    byte[] writeBuffer = new byte[] { register };
    byte[] readBuffer = new byte[1];
    I2cTransferResult result = device.WriteRead(writeBuffer, readBuffer);
    Debug.WriteLine($"  {name} (0x{register:X2}): status={result.Status}, bytesW={result.BytesTransferred}, value=0x{readBuffer[0]:X2}");
    return result.Status;
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
