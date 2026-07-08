// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Es7210;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

// This sample targets the M5Stack CoreS3 microphone path (dual microphones -> ES7210 ADC).
//
// M5Stack CoreS3 audio wiring (see the ESP-IDF BSP for m5stack_core_s3):
//   I2C control : SCL = GPIO11, SDA = GPIO12 (internal system bus)
//   I2S data    : MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DSIN = GPIO14 (from the ES7210)
//   Microphone power is gated by the on-board AW9523 IO expander (pin 2). The board package
//   (or the Iot.Device.Aw9523x binding) must drive that pin high for the microphones to work.

// Setup the M5Stack CoreS3 internal I2C bus (SDA = GPIO12, SCL = GPIO11).
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

// The CoreS3 gates power to the internal bus and the audio codecs through the AXP2101 PMIC and the
// AW9523 IO expander. They must be powered up before the ES7210 will respond on I2C or capture audio.
EnableCoreS3Power();

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es7210.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es7210 adc = new Es7210(i2cDevice);

// Quick sanity check that the ES7210 acknowledges on the I2C bus.
SpanByte probeWrite = new byte[] { 0x00 };
SpanByte probeRead = new byte[1];
I2cTransferResult probe = i2cDevice.WriteRead(probeWrite, probeRead);
bool codecPresent = probe.Status == I2cTransferStatus.FullTransfer;
Debug.WriteLine($"ES7210 I2C probe at 0x{Es7210.DefaultI2cAddress:X2}: {(codecPresent ? "OK (codec present)" : "FAILED - check address/wiring")}");

// Setup the ESP32 I2S receiver first so MCLK/BCLK/WS are running before the codec is configured.
// M5Stack CoreS3 I2S pin-out: MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DSIN = GPIO14.
Configuration.SetPinFunction(34, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(33, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(14, DeviceFunction.I2S1_MDATA_IN);
Configuration.SetPinFunction(0, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Rx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = 16_000,
});

// A first read starts the I2S master clocks so the codec sees MCLK/BCLK before it is configured.
SpanByte warmup = new byte[256];
i2s.Read(warmup);

// Bring the codec up and start capturing. Set the mic gain (0..14, higher = louder).
adc.Initialize();
adc.Start();
adc.MicGain = Es7210.MaxMicGain;

// Select which microphones to capture: MIC1 only, MIC2 only, or both (the default).
adc.SelectedMicrophones = Microphones.Microphone1 | Microphones.Microphone2;

Debug.WriteLine("ES7210 initialized and capturing. Speak or tap near the microphones.");

// Read audio and report a per-channel peak so you can see the mics are alive.
SpanByte buffer = new byte[1024];

while (true)
{
    i2s.Read(buffer);

    int peakLeft = 0;
    int peakRight = 0;
    int sampleIndex = 0;
    for (int i = 0; i + 1 < buffer.Length; i += 2)
    {
        short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
        int magnitude = sample < 0 ? -sample : sample;

        if ((sampleIndex & 1) == 0)
        {
            if (magnitude > peakRight)
            {
                peakRight = magnitude;
            }
        }
        else
        {
            if (magnitude > peakLeft)
            {
                peakLeft = magnitude;
            }
        }

        sampleIndex++;
    }

    Debug.WriteLine($"Peak amplitude  left = {peakLeft}  right = {peakRight}");
    Thread.Sleep(200);
}

// Powers up the M5Stack CoreS3 internal bus and audio codecs. The CoreS3 gates these rails through
// the AXP2101 PMIC (0x34) and the AW9523 IO expander (0x58). See the Iot.Device.Axp2101 and
// Iot.Device.Aw9523x bindings (or the M5Stack CoreS3 board package) for a fuller implementation.
static void EnableCoreS3Power()
{
    // AXP2101 PMIC: set and enable the peripheral rails used by the CoreS3.
    using (I2cDevice axp = I2cDevice.Create(new I2cConnectionSettings(1, 0x34)))
    {
        WriteRegister(axp, 0x92, 0x0D); // ALDO1 = 1.8V
        WriteRegister(axp, 0x93, 0x1C); // ALDO2 = 3.3V
        WriteRegister(axp, 0x94, 0x1C); // ALDO3 = 3.3V
        WriteRegister(axp, 0x95, 0x1C); // ALDO4 = 3.3V
        WriteRegister(axp, 0x96, 0x1C); // BLDO1 = 3.3V
        WriteRegister(axp, 0x97, 0x1C); // BLDO2 = 3.3V
        SetRegisterBits(axp, 0x90, 0x3F); // Enable ALDO1-4 + BLDO1-2.
    }

    // AW9523 IO expander: replicate the M5Stack CoreS3 (LovyanGFX) initialization. P0 must be switched
    // to push-pull GPIO mode (GCR bit 4), otherwise the audio-enable pin (P0_2) can only pull low.
    using (I2cDevice aw = I2cDevice.Create(new I2cConnectionSettings(1, 0x58)))
    {
        WriteRegister(aw, 0x11, 0x10); // GCR: P0 port push-pull.
        WriteRegister(aw, 0x12, 0xFF); // P0 pins in GPIO mode (not LED/constant-current mode).
        WriteRegister(aw, 0x13, 0xFF); // P1 pins in GPIO mode.
        WriteRegister(aw, 0x04, 0x18); // P0 direction: P0_3/P0_4 inputs, the rest outputs.
        WriteRegister(aw, 0x05, 0x0C); // P1 direction: P1_2/P1_3 inputs, the rest outputs.
        WriteRegister(aw, 0x03, 0x83); // P1 output: defaults + BOOST_EN (P1_7) high.
        WriteRegister(aw, 0x02, 0x05); // P0 output: defaults + audio (mic) enable (P0_2) high.
    }

    Thread.Sleep(20);
}

static void WriteRegister(I2cDevice device, byte register, byte value)
{
    device.Write(new byte[] { register, value });
}

static byte ReadRegister(I2cDevice device, byte register)
{
    byte[] read = new byte[1];
    device.WriteRead(new byte[] { register }, read);
    return read[0];
}

static void SetRegisterBits(I2cDevice device, byte register, byte bits)
{
    byte current = ReadRegister(device, register);
    WriteRegister(device, register, (byte)(current | bits));
}
