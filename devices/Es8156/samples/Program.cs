// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Es8156;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

// This sample targets the ESP32-S3-BOX-Lite audio playback path (ES8156 DAC -> speaker amplifier).
// It generates a short melody as PCM audio and streams it to the ES8156 over I2S so you can
// actually hear the codec working.
//
// Bus overview on the ESP32-S3-BOX-Lite:
//   I2C control : SCL = GPIO18, SDA = GPIO8
//   I2S data    : DOUT = GPIO15, WS = GPIO47, BCLK = GPIO17, MCLK = GPIO2
//   Speaker amp : POWER_AMP = GPIO46 (drive high to enable)

const int SampleRate = 16_000;
const short Amplitude = 8_000;

// Setup ESP32-S3-BOX-Lite I2C control bus (SDA = GPIO8, SCL = GPIO18).
// GPIO8/GPIO18 are passed as pin numbers directly: the Gpio helper enum omits the classic
// ESP32 flash pins (IO6-IO11), so the ESP32-S3 pin numbers are used as integer literals.
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es8156.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es8156 dac = new Es8156(i2cDevice);

// Enable the speaker power amplifier (POWER_AMP = GPIO46) so the output can be heard.
GpioController gpio = new GpioController();
GpioPin powerAmp = gpio.OpenPin(46, PinMode.Output);
powerAmp.Write(PinValue.High);

// Setup the ESP32 I2S transmitter FIRST so MCLK/BCLK/WS are already running.
// The ES8156 is a slave on the I2S bus and needs the master clock (MCLK) present while it is
// being configured over I2C, otherwise its internal clock manager never locks.
// ESP32-S3-BOX-Lite I2S pin-out (verify against your board revision):
//   BCLK = GPIO17, WS/LRCLK = GPIO47, DOUT (data to DAC) = GPIO15, MCLK = GPIO2.
Configuration.SetPinFunction(17, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(47, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(15, DeviceFunction.I2S1_DATA_OUT);
Configuration.SetPinFunction(2, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Tx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = SampleRate,
    BufferSize = 40_000,
});

// --- I2S configuration check (software only) ---
// Read back which GPIO each I2S function is actually routed to. If MCLK shows -1 the routing
// did not take and the codec will never get a master clock. On the ESP32-S3-BOX-Lite these
// should print 2 / 17 / 47 / 15 respectively.
Debug.WriteLine($"[i2s-config] I2S1_MCK pin      = {Configuration.GetFunctionPin(DeviceFunction.I2S1_MCK)} (expected 2)");
Debug.WriteLine($"[i2s-config] I2S1_BCK pin      = {Configuration.GetFunctionPin(DeviceFunction.I2S1_BCK)} (expected 17)");
Debug.WriteLine($"[i2s-config] I2S1_WS pin       = {Configuration.GetFunctionPin(DeviceFunction.I2S1_WS)} (expected 47)");
Debug.WriteLine($"[i2s-config] I2S1_DATA_OUT pin = {Configuration.GetFunctionPin(DeviceFunction.I2S1_DATA_OUT)} (expected 15)");

// Read back the connection settings actually held by the device. The enum values print as
// numbers: Mode is a [Flags] enum (Master=1, Slave=2, Tx=4, Rx=8, so Master|Tx = 5);
// BitsPerSample is the bit count; SampleRate is in Hz.
I2sConnectionSettings applied = i2s.ConnectionSettings;
Debug.WriteLine($"[i2s-config] BusId={applied.BusId} Mode={(int)applied.Mode} SampleRate={applied.SampleRate} " +
    $"Bits={(int)applied.BitsPerSample} Channel={(int)applied.ChannelFormat} Format={(int)applied.CommunicationFormat} BufferSize={applied.BufferSize}");

// A first write of silence starts the I2S master clocks so the codec sees MCLK/BCLK before
// it is configured.
i2s.Write(new byte[512]);

// Bring the codec up and un-mute at a moderate volume. The Initialize() sequence already puts
// the ES8156 into 16-bit I2S mode.
dac.Initialize();
dac.Volume = 70;
dac.Muted = false;

// --- Software-only self-tests (no scope or extra hardware needed) ---

// Self-test 1: read the volume back over I2C. If it reads a sensible non-zero value (~70%) it
// proves the codec is alive on the bus and keeping the config we wrote.
byte volume = dac.Volume;
Debug.WriteLine($"[self-test 1] ES8156 volume reads back {volume}% (expected ~70%).");

// Self-test 2: time an I2S write. In master mode i2s.Write() is back-pressured by the DMA and
// the bit/word clock, so streaming N seconds of audio must block for roughly N seconds (minus
// the DMA buffer). If a 3-second buffer takes ~2-3 s, BCLK/WS are genuinely running at the
// configured rate and the ESP32 playback path works - any remaining silence is then on the
// codec / MCLK / amplifier / analog side. If it returns almost instantly, the I2S clock is not
// advancing and nothing is actually being streamed.
const int TestToneMs = 3000;
byte[] testTone = GenerateTriangleTone(440, TestToneMs, SampleRate, Amplitude);

DateTime writeStart = DateTime.UtcNow;
i2s.Write(testTone);
long elapsedMs = (DateTime.UtcNow - writeStart).Ticks / TimeSpan.TicksPerMillisecond;

Debug.WriteLine($"[self-test 2] Wrote {TestToneMs} ms of audio; i2s.Write() blocked for {elapsedMs} ms.");
if (elapsedMs > TestToneMs / 2)
{
    Debug.WriteLine("[self-test 2] => I2S IS clocking data out at the expected rate. The ESP32 playback path works.");
    Debug.WriteLine("[self-test 2] => If you still hear nothing, the problem is the codec / MCLK / amplifier / analog side.");
}
else
{
    Debug.WriteLine("[self-test 2] => i2s.Write() returned far too fast: the I2S clock is NOT advancing.");
    Debug.WriteLine("[self-test 2] => No audio is being streamed - MCLK/BCLK are likely not emitted on this target/firmware.");
}

Debug.WriteLine("ES8156 initialized and un-muted at 70% volume. Playing a melody...");

// A simple ascending arpeggio (C4, E4, G4, C5) so you can clearly hear the DAC output.
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
            // Rising edge from -amplitude to +amplitude.
            value = -amplitude + (2 * amplitude * position) / half;
        }
        else
        {
            // Falling edge from +amplitude back to -amplitude.
            value = amplitude - (2 * amplitude * (position - half)) / half;
        }

        short sample = (short)value;
        byte low = (byte)(sample & 0xFF);
        byte high = (byte)((sample >> 8) & 0xFF);

        // Left channel.
        data[index++] = low;
        data[index++] = high;

        // Right channel (same content).
        data[index++] = low;
        data[index++] = high;
    }

    return data;
}
