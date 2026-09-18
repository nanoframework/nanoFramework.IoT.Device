# ES8311 - Low-power mono audio CODEC (DAC + ADC)

The [Everest Semiconductor ES8311](http://www.everest-semi.com/) is a low-power mono audio CODEC that combines a DAC (playback) and an ADC (microphone capture) in a single chip. It is controlled over I2C and streams PCM audio over I2S. It is used, for example, on the **M5Stack M5StickS3**, where the ES8311 feeds an **AW8737** speaker amplifier.

This binding covers the **I2C control plane only**. It resets and clocks the codec, powers up the DAC and/or the ADC, and exposes the output volume, mute and microphone gain. The PCM audio samples are streamed over **I2S** using `System.Device.I2s` and are not handled by this binding. The register sequences are ported from the M5Stack `M5Unified` and Espressif `esp_codec_dev` drivers.

## Documentation

- [M5StickS3 product page](https://docs.m5stack.com/en/core/M5StickS3)
- [Espressif esp_codec_dev ES8311 driver](https://github.com/espressif/esp-adf/tree/master/components/audio_hal/driver/es8311)
- [M5Stack M5Unified audio implementation](https://github.com/m5stack/M5Unified)

## Device family

| Property | Value |
| --- | --- |
| I2C address | `0x18` (CE low) or `0x19` (CE high) |
| Device ID | `0x8311` (registers 0xFD / 0xFE) |
| DAC volume | register `0x32` |
| DAC mute | register `0x31` |
| ADC (mic) gain | register `0x17` |

## Usage

**Important**: on the M5StickS3 the ES8311 is on the internal I2C bus (SDA = GPIO47, SCL = GPIO48) and the audio uses I2S (MCLK = GPIO18, BCLK = GPIO17, WS = GPIO15, DOUT = GPIO14). The AW8737 speaker amplifier is enabled via the M5PM1 GPIO3 pin.

```csharp
using Iot.Device.Es8311;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Device.I2s;

// M5StickS3 internal I2C bus.
Configuration.SetPinFunction(47, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(48, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es8311.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es8311 codec = new Es8311(i2cDevice);

// Setup the ESP32 I2S transmitter (MCLK = GPIO18, BCLK = GPIO17, WS = GPIO15, DOUT = GPIO14).
Configuration.SetPinFunction(17, DeviceFunction.I2S1_BCK);
Configuration.SetPinFunction(15, DeviceFunction.I2S1_WS);
Configuration.SetPinFunction(14, DeviceFunction.I2S1_DATA_OUT);
Configuration.SetPinFunction(18, DeviceFunction.I2S1_MCK);

I2sDevice i2s = new I2sDevice(new I2sConnectionSettings(1)
{
    Mode = I2sMode.Master | I2sMode.Tx,
    CommunicationFormat = I2sCommunicationFormat.I2S,
    ChannelFormat = I2sChannelFormat.RightLeft,
    BitsPerSample = I2sBitsPerSample.Bit16,
    SampleRate = 44_100,
});

// Start the clocks, then configure the codec for playback.
i2s.Write(new byte[512]);
codec.Initialize();
codec.StartPlayback();
codec.Muted = false;
codec.Volume = 80;

// Write interleaved stereo 16-bit PCM to i2s.Write(...) to play audio.
```

For microphone capture, call `codec.StartCapture()` and set `codec.MicGain`, then read PCM with an I2S receiver.
