# ES8156 - Low-power stereo audio DAC (playback codec)

The [Everest Semiconductor ES8156](https://www.everest-semi.com/) is a low-power stereo audio DAC used on many ESP32 audio boards (ESP-BOX, ESP32-S3-Korvo, LyraT). It converts an I2S PCM stream into analog audio for a headphone/line output or a speaker amplifier.

This binding covers the **I2C control plane only**. It configures the codec (clocking, serial data format, volume, mute and power state). The actual PCM audio samples are streamed over **I2S** using `System.Device.I2s` and are not handled by this binding.

> [!IMPORTANT]
> The register sequences are ported from the Espressif [`esp_codec_dev`](https://components.espressif.com/components/espressif/esp_codec_dev) component and must be validated on real hardware. The default I2C address and the exact serial-format bit mapping depend on your board wiring - confirm against the schematic and codec datasheet.

## Documentation

- ES8156 is configured over I2C (control) and streams audio over I2S (data).
- Two chips make up a full-duplex audio path on boards such as the ESP32-S3-BOX-Lite: the ES8156 (DAC, playback) and the ES7243E (ADC, capture).
- Espressif [`esp_codec_dev` component](https://components.espressif.com/components/espressif/esp_codec_dev) - the source of the register sequences used by this binding ([ES8156 driver source](https://github.com/espressif/esp-adf/tree/master/components/esp_codec_dev/device/es8156)).
- Espressif [ESP32-S3-BOX-Lite hardware overview](https://github.com/espressif/esp-box/blob/master/docs/hardware_overview/esp32_s3_box_lite/hardware_overview_for_lite.md) and the [ESP-BOX repository](https://github.com/espressif/esp-box).

## Audio path on the ESP32-S3-BOX-Lite

| Signal | Pin | Purpose |
| --- | --- | --- |
| I2C SCL | GPIO18 | Codec register control clock |
| I2C SDA | GPIO8 | Codec register control data |
| I2S DOUT | GPIO15 | Playback samples out of the ESP32 to the ES8156 |
| I2S DSIN | GPIO16 | Capture samples from the ES7243E into the ESP32 |
| I2S WS / LRCLK | GPIO47 | Word select / left-right clock |
| POWER_AMP | GPIO46 | Enables the speaker power amplifier |

The **I2C bus carries register configuration only** (sample rate, data format, volume, mute, power). No audio samples travel over I2C. The **actual PCM audio streams over I2S**.

## Usage

**Important**: I2C pins for the ESP32 must be properly set up before creating the `I2cDevice`. For this, make sure you install the `nanoFramework.Hardware.Esp32` NuGet and use the `Configuration` class to configure the pins:

```csharp
using Iot.Device.Es8156;
using nanoFramework.Hardware.Esp32;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.I2s;

// Setup ESP32-S3-BOX-Lite I2C control bus (SDA = GPIO8, SCL = GPIO18).
// GPIO8/GPIO18 are passed as pin numbers directly because the Gpio helper enum omits the
// classic ESP32 flash pins (IO6-IO11).
Configuration.SetPinFunction(8, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(18, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es8156.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es8156 dac = new Es8156(i2cDevice);

// Enable the speaker power amplifier (POWER_AMP = GPIO46).
GpioController gpio = new GpioController();
GpioPin powerAmp = gpio.OpenPin(46, PinMode.Output);
powerAmp.Write(PinValue.High);

// Setup the ESP32 I2S transmitter first so MCLK/BCLK/WS are running before the codec is
// configured. ESP32-S3-BOX-Lite: BCLK = GPIO17, WS = GPIO47, DOUT = GPIO15, MCLK = GPIO2.
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
    SampleRate = 16_000,
    BufferSize = 40_000,
});

// A first write of silence starts the master clocks so the codec locks during configuration.
i2s.Write(new byte[512]);

// Bring the codec up and un-mute at a moderate volume.
dac.Initialize();
dac.Volume = 70;
dac.Muted = false;

// Write interleaved stereo 16-bit PCM to i2s.Write(...) to play audio. The sample generates
// a short triangle-wave melody so you can hear the DAC output on the speaker.
```

For other targets (STM32, etc.) use the preset hardware I2C pins (no `SetPinFunction` call needed).
