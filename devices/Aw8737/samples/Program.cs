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

// Bring the board power up (charge path + external 5V), matching the vendor power-on sequence. After a
// deep battery discharge the M5PM1 resets these to off, so re-enable them explicitly. The internal LDO
// supplies the peripheral rail that powers the ES8311 codec.
pmic.BatteryChargeEnabled = true;
pmic.ExternalOutputEnabled = true;

// PM1_G3 is used by vendor firmware to gate the audio PA. Drive it high to power the amplifier.
pmic.SetGpioFunction(Pin.Gpio3, GpioFunction.Gpio);
pmic.SetGpioDrive(Pin.Gpio3, GpioDrive.PushPull);
pmic.SetGpioMode(Pin.Gpio3, PinMode.Output);
pmic.WriteGpio(Pin.Gpio3, PinValue.High);

// Give the codec power rail time to stabilise, then read the PMIC state back so the log confirms the
// LDO and amplifier gate were actually applied.
System.Threading.Thread.Sleep(100);
Debug.WriteLine($"M5PM1 power: charge={pmic.BatteryChargeEnabled}, ext5V={pmic.ExternalOutputEnabled}, G3={pmic.ReadGpioOutputLatch(Pin.Gpio3)}.");
// --- ESP32 I2S transmitter: start MCLK/BCLK/WS BEFORE the codec is accessed. The ES8311 register
// interface needs its clock running, so the I2S must be streaming before the codec is read/configured. ---
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

// A first write of silence starts the master clocks so the codec has MCLK for its register interface.
i2s.Write(new byte[512]);

// --- ES8311 codec (I2C at 0x18), accessed only after its MCLK is running ---
I2cConnectionSettings codecSettings = new I2cConnectionSettings(1, Es8311.DefaultI2cAddress, I2cBusSpeed.StandardMode);
Es8311 codec = new Es8311(new I2cDevice(codecSettings));
Debug.WriteLine($"ES8311 device ID: 0x{codec.GetDeviceId():X4} (expected 0x{Es8311.DeviceId:X4}).");

// --- AW8737A amplifier: enable (Mode 1) using a plain GPIO ---
Aw8737 amplifier = new Aw8737(AmplifierControlPin);
amplifier.Enabled = true;
Debug.WriteLine($"AW8737 enabled: {amplifier.Enabled}.");

codec.Initialize();
codec.StartPlayback();
codec.Muted = false;
codec.Volume = 80;

// Read the codec state back so the log confirms the DAC configuration writes actually landed.
Debug.WriteLine($"ES8311 configured: muted={codec.Muted}, volume={codec.Volume}%. Playing a melody through the AW8737A. You should hear the speaker.");

// A simple ascending arpeggio (C4, E4, G4, C5).
int[] melody = new int[] { 262, 330, 392, 523 };

// Raise the ES8311 volume in steps, playing ~10 s at each level.
byte[] volumes = new byte[] { 40, 60, 80, 100 };

// 4 notes x 350 ms x 7 passes ~= 10 seconds per segment.
const int PassesPerSegment = 7;

while (true)
{
    for (int step = 0; step < volumes.Length; step++)
    {
        codec.Volume = volumes[step];
        Debug.WriteLine($"ES8311 volume {volumes[step]}%. Playing ~10 s...");

        PlayMelody(i2s, melody, PassesPerSegment, SampleRate, Amplitude);
    }
}

// Plays the melody the given number of times over I2S.
static void PlayMelody(I2sDevice i2s, int[] melody, int passes, int sampleRate, short amplitude)
{
    for (int pass = 0; pass < passes; pass++)
    {
        for (int note = 0; note < melody.Length; note++)
        {
            byte[] tone = GenerateTriangleTone(melody[note], 350, sampleRate, amplitude);
            i2s.Write(tone);
        }
    }
}

// Sets an ESP32 pin function, skipping it when the pin already has that function (for example after a
// soft reboot without a full power cycle, which would otherwise throw an ArgumentException).
static void ConfigurePin(int pinNumber, DeviceFunction function)
{
    if (Configuration.GetFunctionPin(function) != pinNumber)
    {
        Configuration.SetPinFunction(pinNumber, function);
    }
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
