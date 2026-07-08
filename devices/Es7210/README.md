# ES7210 - 4-channel audio ADC (microphone capture codec)

The [Everest Semiconductor ES7210](https://www.everest-semi.com/) is a 4-channel high-performance audio ADC used on many ESP32 audio boards (ESP32-S3-Korvo, ESP-BOX, and the **M5Stack CoreS3**). It digitizes analog microphone inputs and outputs the captured audio as an I2S PCM stream.

This binding covers the **I2C control plane only**. It configures the codec (clocking, microphone selection, gain, mute and power state) for a two-microphone (MIC1 + MIC2) capture setup in slave mode. The captured PCM audio samples are streamed out over **I2S** using `System.Device.I2s` and are not handled by this binding.

> [!IMPORTANT]
> The register sequences are ported from the Espressif [`esp_codec_dev`](https://components.espressif.com/components/espressif/esp_codec_dev) component and should be validated on other hardware than CoreS3. The default I2C address is 0x40.

## Documentation

- ES7210 is configured over I2C (control) and streams captured audio over I2S (data).
- On the M5Stack CoreS3 the ES7210 (microphones) and the AW88298 (speaker) share the same internal I2C bus and I2S data lines.
- Espressif [`esp_codec_dev` component](https://components.espressif.com/components/espressif/esp_codec_dev) - the source of the register sequences ([ES7210 driver source](https://github.com/espressif/esp-adf/tree/master/components/esp_codec_dev/device/es7210)).
- M5Stack [CoreS3 documentation](https://docs.m5stack.com/en/core/CoreS3) and the Espressif [CoreS3 board support package](https://github.com/espressif/esp-bsp/tree/master/bsp/m5stack_core_s3).

## Audio path on the M5Stack CoreS3

| Signal | Pin | Purpose |
| --- | --- | --- |
| I2C SCL | GPIO11 | Codec register control clock |
| I2C SDA | GPIO12 | Codec register control data |
| I2S MCLK | GPIO0 | Master clock |
| I2S BCLK | GPIO34 | Bit clock |
| I2S WS / LRCLK | GPIO33 | Word select / left-right clock |
| I2S DSIN | GPIO14 | Capture samples from the ES7210 into the ESP32 |

> [!NOTE]
> On the CoreS3 the internal bus and the microphone power are gated by the on-board **AXP2101 PMIC**
> and **AW9523 IO expander** (pin 2). The sample powers these up over I2C before configuring the codec;
> for a fuller implementation use the `Iot.Device.Axp2101` / `Iot.Device.Aw9523x` bindings or the
> M5Stack CoreS3 board package.

## Usage

```csharp
using Iot.Device.Es7210;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Device.I2s;

// Setup the M5Stack CoreS3 internal I2C bus (SDA = GPIO12, SCL = GPIO11).
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Es7210.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Es7210 adc = new Es7210(i2cDevice);

// Setup the ESP32 I2S receiver (MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DSIN = GPIO14).
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

// Start the clocks, then configure the codec.
SpanByte warmup = new byte[256];
i2s.Read(warmup);

adc.Initialize();
adc.Start();
adc.MicGain = Es7210.MaxMicGain;

// Select which microphones to capture: MIC1 only, MIC2 only, or both (the default).
adc.SelectedMicrophones = Microphones.Microphone1 | Microphones.Microphone2;

// Read interleaved stereo 16-bit PCM with i2s.Read(...) to obtain microphone samples.
```
