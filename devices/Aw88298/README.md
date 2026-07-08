# AW88298 - I2S Class-D smart audio amplifier (speaker output)

The [Awinic AW88298](https://www.awinic.com/) is a 16-bit I2S Class-D smart audio amplifier with an integrated boost converter and DSP. It drives a speaker directly from an I2S PCM stream and is used, for example, on the **M5Stack CoreS3** to drive the built-in 1W speaker.

This binding covers the **I2C control plane only**. It configures the amplifier (reset, power/enable, output volume, mute and boost). The PCM audio samples are streamed over **I2S** using `System.Device.I2s` and are not handled by this binding.

> [!IMPORTANT]
> The register sequences are ported from the Espressif [`esp_codec_dev`](https://components.espressif.com/components/espressif/esp_codec_dev) component and should be validated should be validated on hardware other than the CoreS3. The AW88298 uses 16-bit registers. The default I2C address is 0x36.

## Documentation

- AW88298 is configured over I2C (control) and streams audio over I2S (data).
- On the M5Stack CoreS3 the AW88298 (speaker) and the ES7210 (microphones) share the same internal I2C bus and I2S data lines.
- Espressif [`esp_codec_dev` component](https://components.espressif.com/components/espressif/esp_codec_dev) - the source of the register sequences ([AW88298 driver source](https://github.com/espressif/esp-adf/tree/master/components/esp_codec_dev/device/aw88298)).
- M5Stack [CoreS3 documentation](https://docs.m5stack.com/en/core/CoreS3) and the Espressif [CoreS3 board support package](https://github.com/espressif/esp-bsp/tree/master/bsp/m5stack_core_s3).

## Audio path on the M5Stack CoreS3

| Signal | Pin | Purpose |
| --- | --- | --- |
| I2C SCL | GPIO11 | Codec register control clock |
| I2C SDA | GPIO12 | Codec register control data |
| I2S MCLK | GPIO0 | Master clock |
| I2S BCLK | GPIO34 | Bit clock |
| I2S WS / LRCLK | GPIO33 | Word select / left-right clock |
| I2S DOUT | GPIO13 | Playback samples out of the ESP32 to the AW88298 |

> [!NOTE]
> On the CoreS3 the internal bus and the speaker power are gated by the on-board **AXP2101 PMIC** and
> **AW9523 IO expander** (pin 2). The sample powers these up over I2C before configuring the amplifier;
> for a fuller implementation use the `Iot.Device.Axp2101` / `Iot.Device.Aw9523x` bindings or the
> M5Stack CoreS3 board package.

## Usage

```csharp
using Iot.Device.Aw88298;
using nanoFramework.Hardware.Esp32;
using System.Device.I2c;
using System.Device.I2s;

// Setup the M5Stack CoreS3 internal I2C bus (SDA = GPIO12, SCL = GPIO11).
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Aw88298.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

Aw88298 amplifier = new Aw88298(i2cDevice);

// Setup the ESP32 I2S transmitter (MCLK = GPIO0, BCLK = GPIO34, WS = GPIO33, DOUT = GPIO13).
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
    SampleRate = 16_000,
    BufferSize = 40_000,
});

// Start the clocks, then configure the amplifier.
// Initialize takes the I2S sample rate so the amplifier's I2S control register matches the stream.
i2s.Write(new byte[512]);
amplifier.Initialize(16_000);
amplifier.Volume = 100;
amplifier.Muted = false;
amplifier.Start();

// Write interleaved stereo 16-bit PCM to i2s.Write(...) to play audio.
```
