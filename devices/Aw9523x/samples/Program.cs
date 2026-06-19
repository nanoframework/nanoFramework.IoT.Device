// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.Aw9523x;
using nanoFramework.Hardware.Esp32;

Debug.WriteLine("AW9523X sample for M5Stack CoreS3");

Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

EnableCoreS3InternalBusPower();
Thread.Sleep(20);

if (!DiagnoseAw9523Link())
{
    Debug.WriteLine("AW9523 did not ACK on I2C address 0x58. Aborting driver init.");
    Debug.WriteLine("Check CoreS3 hardware and internal power rails.");
    EnterSafeIdle();
}

try
{
    using I2cDevice aw9523Device = I2cDevice.Create(new I2cConnectionSettings(1, Aw9523x.DefaultI2cAddress));
    using Aw9523x aw9523 = new(aw9523Device);

    byte chipId = aw9523.ChipId;
    Debug.WriteLine($"Chip ID: 0x{chipId:X2} (expected 0x{Aw9523x.ExpectedChipId:X2})");
    if (chipId != Aw9523x.ExpectedChipId)
    {
        Debug.WriteLine("Warning: unexpected chip ID. Check board wiring and I2C bus.");
        EnterSafeIdle();
    }

    while (true)
    {
        try
        {
            byte p0 = aw9523.ReadOutputPort(Port.Port0);
            byte p1 = aw9523.ReadOutputPort(Port.Port1);
            Debug.WriteLine($"Port0 OUT: 0x{p0:X2}, Port1 OUT: 0x{p1:X2}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading ports: {ex.Message}");
        }

        Thread.Sleep(1000);
    }
}
catch (Exception ex)
{
    Debug.WriteLine($"Fatal initialization error: {ex.Message}");
    EnterSafeIdle();
}

static bool DiagnoseAw9523Link()
{
    using I2cDevice aw = I2cDevice.Create(new I2cConnectionSettings(1, Aw9523x.DefaultI2cAddress));
    Debug.WriteLine("AW9523 low-level probe on I2C1 @ 0x58:");
    I2cTransferStatus s0 = ReadAndLog(aw, 0x00, "INPUT_P0");
    I2cTransferStatus s1 = ReadAndLog(aw, 0x01, "INPUT_P1");
    I2cTransferStatus s2 = ReadAndLog(aw, 0x10, "CHIP_ID");
    I2cTransferStatus s3 = ReadAndLog(aw, 0x11, "GLOBAL_CTRL");

    return s0 == I2cTransferStatus.FullTransfer
        || s1 == I2cTransferStatus.FullTransfer
        || s2 == I2cTransferStatus.FullTransfer
        || s3 == I2cTransferStatus.FullTransfer;
}

static I2cTransferStatus ReadAndLog(I2cDevice device, byte register, string name)
{
    byte[] writeBuffer = new byte[] { register };
    byte[] readBuffer = new byte[1];
    I2cTransferResult result = device.WriteRead(writeBuffer, readBuffer);
    Debug.WriteLine($"  {name} (0x{register:X2}): status={result.Status}, bytesW={result.BytesTransferred}, value=0x{readBuffer[0]:X2}");
    return result.Status;
}

static void EnterSafeIdle()
{
    while (true)
    {
        Thread.Sleep(1000);
    }
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
