// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Aw88298;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

// This sample targets the M5Stack CoreS3 speaker path (AW88298 amplifier -> built-in 1W speaker).
// It generates a short melody as PCM audio and streams it to the AW88298 over I2S.
//
// M5Stack CoreS3 audio wiring (see the ESP-IDF BSP for m5stack_core_s3):
//   I2C control : SCL = GPIO11, SDA = GPIO12 (internal system bus)
//   I2S data    : MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DOUT = GPIO13 (to the AW88298)
//   Speaker/mic power is gated by the on-board AW9523 IO expander (pin 2). The board package
//   (or the Iot.Device.Aw9523x binding) must drive that pin high to actually hear audio.

const int SampleRate = 16_000;
const short Amplitude = 8_000;

// Setup the M5Stack CoreS3 internal I2C bus (SDA = GPIO12, SCL = GPIO11).
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

// The CoreS3 gates power to the internal bus and the audio codecs through the AXP2101 PMIC and the
// AW9523 IO expander. They must be powered up before the AW88298 will respond on I2C or make sound.
EnableCoreS3Power();

I2cConnectionSettings settings = new I2cConnectionSettings(1, Aw88298.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Aw88298 amplifier = new Aw88298(i2cDevice);

// Setup the ESP32 I2S transmitter first so MCLK/BCLK/WS are running before the amplifier configures.
// M5Stack CoreS3 I2S pin-out: MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DOUT = GPIO13.
Configuration.SetPinFunction(34, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(33, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(13, DeviceFunction.I2S1_DATA_OUT);
Configuration.SetPinFunction(0, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Tx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = SampleRate,
    BufferSize = 40_000,
});

// A first write of silence starts the master clocks so the amplifier locks during configuration.
i2s.Write(new byte[512]);

// Bring the amplifier up (the I2S control register is derived from the stream sample rate), un-mute
// and set a high volume, then enable the output stage.
amplifier.Initialize(SampleRate);
amplifier.Volume = 100;
amplifier.Muted = false;
amplifier.Start();

Debug.WriteLine("AW88298 initialized. Playing a melody...");

// A simple ascending arpeggio (C4, E4, G4, C5).
int[] melody = new int[] { 262, 330, 392, 523 };

while (true)
{
    for (int note = 0; note < melody.Length; note++)
    {
        byte[] tone = GenerateTriangleTone(melody[note], 350, SampleRate, Amplitude);
        i2s.Write(tone);
    }

    // Short silence between repeats.
    i2s.Write(new byte[SampleRate]);
}

// Generates a triangle-wave tone as interleaved stereo 16-bit little-endian PCM.
static byte[] GenerateTriangleTone(int frequency, int durationMs, int sampleRate, short amplitude)
{
    int totalSamples = sampleRate * durationMs / 1000;
    int period = sampleRate / frequency;
    if (period < 2)
    {
        period = 2;
    }

    int half = period / 2;
    byte[] data = new byte[totalSamples * 4];
    int index = 0;

    for (int n = 0; n < totalSamples; n++)
    {
        int position = n % period;
        int value;
        if (position < half)
        {
            value = -amplitude + (2 * amplitude * position) / half;
        }
        else
        {
            value = amplitude - (2 * amplitude * (position - half)) / half;
        }

        short sample = (short)value;
        byte low = (byte)(sample & 0xFF);
        byte high = (byte)((sample >> 8) & 0xFF);

        data[index++] = low;
        data[index++] = high;
        data[index++] = low;
        data[index++] = high;
    }

    return data;
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
    // to push-pull GPIO mode (GCR bit 4), otherwise the speaker-enable pin (P0_2) can only pull low and
    // the amplifier stays powered off even though it answers on I2C.
    using (I2cDevice aw = I2cDevice.Create(new I2cConnectionSettings(1, 0x58)))
    {
        WriteRegister(aw, 0x11, 0x10); // GCR: P0 port push-pull.
        WriteRegister(aw, 0x12, 0xFF); // P0 pins in GPIO mode (not LED/constant-current mode).
        WriteRegister(aw, 0x13, 0xFF); // P1 pins in GPIO mode.
        WriteRegister(aw, 0x04, 0x18); // P0 direction: P0_3/P0_4 inputs, the rest outputs.
        WriteRegister(aw, 0x05, 0x0C); // P1 direction: P1_2/P1_3 inputs, the rest outputs.
        WriteRegister(aw, 0x03, 0x83); // P1 output: defaults + BOOST_EN (P1_7) high.
        WriteRegister(aw, 0x02, 0x05); // P0 output: defaults + audio (speaker) enable (P0_2) high.
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
