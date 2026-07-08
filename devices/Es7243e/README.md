# ES7243E - Audio ADC (microphone capture codec)

The [Everest Semiconductor ES7243E](https://www.everest-semi.com/) is a low-power audio ADC used on many ESP32 audio boards (ESP-BOX, ESP32-S3-Korvo, LyraT). It digitizes analog microphone inputs and outputs the captured audio as an I2S PCM stream.

This binding covers the **I2C control plane only**. It configures the codec (clocking, serial data format, microphone gain, mute and power state). The captured PCM audio samples are streamed out over **I2S** using `System.Device.I2s` and are not handled by this binding.

> [!IMPORTANT]
> The register sequences are ported from the Espressif [`esp_codec_dev`](https://components.espressif.com/components/espressif/esp_codec_dev) component and must be validated on real hardware if different from the ESP32-S3 Box Lite. The default I2C address (typically 0x10 or 0x11) and the exact serial-format bit mapping depend on your board wiring - confirm against the schematic and codec datasheet.

## Documentation

- ES7243E is configured over I2C (control) and streams captured audio over I2S (data).
- Two chips make up a full-duplex audio path on boards such as the ESP32-S3-BOX-Lite: the ES7243E (ADC, capture) and the ES8156 (DAC, playback).
- Espressif [`esp_codec_dev` component](https://components.espressif.com/components/espressif/esp_codec_dev) - the source of the register sequences used by this binding ([ES7243E driver source](https://github.com/espressif/esp-adf/tree/master/components/esp_codec_dev/device/es7243e)).
- Espressif [ESP32-S3-BOX-Lite hardware overview](https://github.com/espressif/esp-box/blob/master/docs/hardware_overview/esp32_s3_box_lite/hardware_overview_for_lite.md) and the [ESP-BOX repository](https://github.com/espressif/esp-box).

## Audio path on the ESP32-S3-BOX-Lite

| Signal | Pin | Purpose |
| --- | --- | --- |
| I2C SCL | GPIO18 | Codec register control clock |
| I2C SDA | GPIO8 | Codec register control data |
| I2S DSIN | GPIO16 | Capture samples from the ES7243E into the ESP32 |
| I2S DOUT | GPIO15 | Playback samples out of the ESP32 to the ES8156 |
| I2S WS / LRCLK | GPIO47 | Word select / left-right clock |

The **I2C bus carries register configuration only** (sample rate, data format, mic gain, mute, power). No audio samples travel over I2C. The **actual PCM audio streams over I2S**.

## Usage

**Important**: I2C pins for the ESP32 must be properly set up before creating the `I2cDevice`. For this, make sure you install the `nanoFramework.Hardware.Esp32` NuGet and use the `Configuration` class to configure the pins:

```csharp
using Iot.Device.Es7243e;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Device.I2s;

// Setup ESP32-S3-BOX-Lite I2C control bus (SDA = GPIO8, SCL = GPIO18).
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es7243e.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es7243e adc = new Es7243e(i2cDevice);

// Setup the ESP32 I2S receiver first so MCLK/BCLK/WS are running before the codec is configured.
// ESP32-S3-BOX-Lite I2S pin-out: BCLK = GPIO17, WS = GPIO47, DSIN = GPIO16, MCLK = GPIO2.
Configuration.SetPinFunction(17, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(47, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(16, DeviceFunction.I2S1_MDATA_IN);
Configuration.SetPinFunction(2, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Rx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = 16_000,
});

// Bring the codec up and start capturing. Initialize() already puts the ES7243E into 16-bit I2S
// mode; set the mic gain after Start() (which resets it to a low value).
adc.Initialize();
adc.Start();
adc.SetMicGain(Es7243e.MaxMicGain);

// Read a block of audio and compute a simple peak amplitude. When you speak or tap near the
// microphones the reported value rises above the idle noise floor, confirming the ES7243E is
// configured correctly and streaming over I2S.
SpanByte buffer = new byte[1024];
i2s.Read(buffer);

int peak = 0;
for (int i = 0; i + 1 < buffer.Length; i += 2)
{
    short sample = (short)(buffer[i] | (buffer[i + 1] << 8));
    int magnitude = sample < 0 ? -sample : sample;
    if (magnitude > peak)
    {
        peak = magnitude;
    }
}

Debug.WriteLine($"Peak amplitude = {peak}");
```

The full sample loops the I2S read so you can watch the peak amplitude react to sound in real time.

For other targets (STM32, etc.) use the preset hardware I2C pins (no `SetPinFunction` call needed).
