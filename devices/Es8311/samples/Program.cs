// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Aw8737;
using Iot.Device.Es8311;
using Iot.Device.M5Pm1;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2s;
using System.Diagnostics;
using System.Threading;

// This sample targets the M5Stack M5StickS3 speaker path and plays a short melody so the AW8737A
// amplifier can be heard: ES8311 codec -> AW8737A amplifier -> speaker. The amplifier is controlled by
// a plain GPIO (enable/disable, Mode 1).
//
// M5StickS3 audio wiring (see the M5Stack M5Unified sources):
//   I2C control : SCL = GPIO48, SDA = GPIO47 (internal bus; ES8311 at 0x18, M5PM1 at 0x6E)
//   I2S data    : MCLK = GPIO18, BCLK = GPIO17, WS = GPIO15, DOUT = GPIO14
//   The AW8737A power rail is gated by the M5PM1 GPIO3 pin; its active-low SHDN pin is driven here from
//   a host GPIO.

const int SampleRate = 44_100;
const short Amplitude = 6_000;
const int AmplifierControlPin = 21;

// --- I2C bus (SDA = GPIO47, SCL = GPIO48) ---
ConfigurePin(47, DeviceFunction.I2C1_DATA);
ConfigurePin(48, DeviceFunction.I2C1_CLOCK);

// --- M5PM1 PMIC: gate the amplifier power rail on G3 ---
// The M5PM1 is a 100 kHz (standard-mode) device; the ESP32 default is 400 kHz, which the PMIC NAKs.
I2cConnectionSettings pmicSettings = new I2cConnectionSettings(1, M5Pm1.I2cDefaultAddress, I2cBusSpeed.StandardMode);
M5Pm1 pmic = new M5Pm1(new I2cDevice(pmicSettings));

// Bring the board power up (charge path + external 5V) using the read-modify-write setters. A direct
// full write to the power-control register (0x06) must NOT be used: it clears a power-hold bit and
// switches the device off. The LDO rail is left off (enabling it browns the board out).
pmic.BatteryChargeEnabled = true;
pmic.ExternalOutputEnabled = true;

// PM1_G3 is used by vendor firmware to gate the audio PA. Drive it high to power the amplifier.
pmic.SetGpioFunction(Pin.Gpio3, GpioFunction.Gpio);
pmic.SetGpioDrive(Pin.Gpio3, GpioDrive.PushPull);
pmic.SetGpioMode(Pin.Gpio3, PinMode.Output);
pmic.WriteGpio(Pin.Gpio3, PinValue.High);

// Give the codec power rail time to stabilise, then read the PMIC state back so the log confirms it.
System.Threading.Thread.Sleep(100);
Debug.WriteLine($"M5PM1 G3={pmic.ReadGpioOutputLatch(Pin.Gpio3)}.");

// Diagnostic: dump the M5PM1 power/GPIO registers and scan the codec address.
DumpM5Pm1Registers();
ScanAddress(0x18);

// --- ES8311 codec (I2C at 0x18) ---
// Upstream (M5Unified) accesses the codec at 100 kHz; FastMode does not work on this internal bus.
I2cConnectionSettings codecSettings = new I2cConnectionSettings(1, Es8311.DefaultI2cAddress, I2cBusSpeed.StandardMode);
Es8311 codec = new Es8311(new I2cDevice(codecSettings));
Debug.WriteLine($"ES8311 device ID: 0x{codec.GetDeviceId():X4} (expected 0x{Es8311.DeviceId:X4}).");

codec.Initialize();
codec.StartPlayback();
codec.Muted = false;
codec.Volume = 80;
Debug.WriteLine($"ES8311 configured: muted={codec.Muted}, volume={codec.Volume}%.");

// Diagnostic: read back the key codec registers directly so we can confirm which config writes landed.
DumpCodecRegisters();

// --- AW8737A amplifier: enable (Mode 1) using a plain GPIO ---
Aw8737 amplifier = new Aw8737(AmplifierControlPin);
amplifier.Enabled = true;
Debug.WriteLine($"AW8737 enabled: {amplifier.Enabled}. Playing a melody; you should hear the speaker.");

// --- ESP32 I2S transmitter ---
ConfigurePin(17, DeviceFunction.I2S1_BCK);
ConfigurePin(15, DeviceFunction.I2S1_WS);
ConfigurePin(14, DeviceFunction.I2S1_DATA_OUT);
ConfigurePin(18, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Tx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = SampleRate,
    BufferSize = 40_000,
});

// Stream the melody continuously.
Playback.I2s = i2s;
Playback.SampleRate = SampleRate;
Playback.Amplitude = Amplitude;
Playback.Loop();

// soft reboot without a full power cycle, which would otherwise throw an ArgumentException).
static void ConfigurePin(int pinNumber, DeviceFunction function)
{
    if (Configuration.GetFunctionPin(function) != pinNumber)
    {
        Configuration.SetPinFunction(pinNumber, function);
    }
}

// Dumps the M5PM1 power/GPIO registers over a fresh I2C handle so we can see its actual power state.
static void DumpM5Pm1Registers()
{
    I2cDevice pm1 = new I2cDevice(new I2cConnectionSettings(1, 0x6E, I2cBusSpeed.StandardMode));
    foreach (byte register in new byte[] { 0x04, 0x06, 0x10, 0x11, 0x12, 0x13, 0x16, 0x17 })
    {
        byte[] value = new byte[1];
        I2cTransferResult result = pm1.WriteRead(new byte[] { register }, value);
        Debug.WriteLine($"  M5PM1 reg 0x{register:X2} = 0x{value[0]:X2} (status={result.Status}).");
    }

    pm1.Dispose();
}

// Probes a single 7-bit I2C address with a 1-byte read and logs the transfer status, so we can compare
// the codec address against known-present and known-absent devices under identical bus conditions.
static void ScanAddress(int address)
{
    I2cDevice device = new I2cDevice(new I2cConnectionSettings(1, address, I2cBusSpeed.StandardMode));
    byte[] value = new byte[1];
    I2cTransferResult result = device.WriteRead(new byte[] { 0x00 }, value);
    Debug.WriteLine($"  I2C scan 0x{address:X2}: status={result.Status}, value=0x{value[0]:X2}.");
    device.Dispose();
}

// Reads back the key ES8311 registers over a fresh I2C handle and logs them, so we can confirm exactly
// which configuration writes landed (in particular the DAC volume register 0x32).
static void DumpCodecRegisters()
{
    I2cDevice dump = new I2cDevice(new I2cConnectionSettings(1, Es8311.DefaultI2cAddress, I2cBusSpeed.StandardMode));
    foreach (byte register in new byte[] { 0x00, 0x01, 0x02, 0x0D, 0x12, 0x13, 0x31, 0x32, 0x37 })
    {
        byte[] value = new byte[1];
        I2cTransferResult result = dump.WriteRead(new byte[] { register }, value);
        Debug.WriteLine($"  ES8311 reg 0x{register:X2} = 0x{value[0]:X2} (status={result.Status}).");
    }

    dump.Dispose();
}

// Generates a triangle-wave tone as interleaved stereo 16-bit little-endian PCM.
// Streams a repeating melody over I2S on a background thread so the master/bit clocks keep running
// while the codec is configured over I2C.
static class Playback
{
    public static I2sDevice I2s;
    public static int SampleRate;
    public static short Amplitude;

    // A simple ascending arpeggio (C4, E4, G4, C5).
    private static readonly int[] Melody = new int[] { 262, 330, 392, 523 };

    public static void Loop()
    {
        while (true)
        {
            for (int note = 0; note < Melody.Length; note++)
            {
                byte[] tone = GenerateTriangleTone(Melody[note], 350, SampleRate, Amplitude);
                I2s.Write(tone);
            }
        }
    }

    // Generates a triangle-wave tone as interleaved stereo 16-bit little-endian PCM.
    private static byte[] GenerateTriangleTone(int frequency, int durationMs, int sampleRate, short amplitude)
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
}
