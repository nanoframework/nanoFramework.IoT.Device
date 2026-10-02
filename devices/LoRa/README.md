# Semtech SX1262 - LoRa transceiver

The [Semtech SX1262](https://www.semtech.com/products/wireless-rf/lora-connect/sx1262) is a sub-GHz LoRa transceiver used on many LoRaWAN and point-to-point modules.

This binding provides **Iot.Device.LoRa** for .NET nanoFramework: **`ILoRaDevice`** for common operations and **`Sx1262`** for SPI radios with reset, BUSY, and DIO1 lines.

## Documentation

- SX1261/2 product page and documentation: [Semtech SX1262](https://www.semtech.com/products/wireless-rf/lora-connect/sx1262#documentation)
- Repository layout: library in **`LoRa/`**, sample in **`samples/Sx1262Sample/`**, hardware-free tests in **`tests/`**

## Board

LoRa modules that expose SPI (NSS, SCK, MOSI, MISO), **RESET**, **BUSY**, and **DIO1** can be used with **`Sx1262`**. Use **3.3 V** logic and wiring per your module’s pinout.

The sample below follows a **Heltec Vision Master E213 (HT-VME213)** style mapping on **ESP32** with **SPI1**. You can adapt the same pattern to any MCU with an SPI port and three free GPIOs.

> **Tip:** You can add a module photo and a wiring diagram to this folder (for example `sensor.jpg` and a breadboard image) and reference them here, similar to [devices/Nrf24l01](https://github.com/nanoframework/nanoFramework.IoT.Device/tree/main/devices/Nrf24l01).

## Usage

### Hardware required

- SX1262-based module (3.3 V)
- Host MCU with SPI (the sample targets **ESP32**)
- Jumper wires

### Connection (sample — ESP32 SPI1, HT-VME213 style)

- VCC — 3.3 V (per module datasheet)
- GND — GND
- MOSI — SPI1 MOSI (GPIO **10** in the sample)
- MISO — SPI1 MISO (GPIO **11**)
- SCK — SPI1 clock (GPIO **9**)
- NSS / CS — SPI1 chip select (GPIO **8**)
- RESET — GPIO output (GPIO **12**)
- BUSY — GPIO input (GPIO **13**)
- DIO1 — GPIO input, IRQ (GPIO **14**)

### Code

**Important:** On **ESP32**, configure SPI pin functions before **`SpiDevice.Create`**, and add the **`nanoFramework.Hardware.Esp32`** NuGet package to your application (see **`samples/Sx1262Sample`**).

```csharp
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;

using Iot.Device.LoRa;
using Iot.Device.LoRa.Drivers.Sx1262;

using nanoFramework.Hardware.Esp32;

// Map SPI1 on ESP32 (GPIOs must match your board)
Configuration.SetPinFunction(10, DeviceFunction.SPI1_MOSI);
Configuration.SetPinFunction(9, DeviceFunction.SPI1_CLOCK);
Configuration.SetPinFunction(11, DeviceFunction.SPI1_MISO);

SpiConnectionSettings settings = new SpiConnectionSettings(1, chipSelectLine: 8)
{
    ClockFrequency = 1_000_000,
    Mode = SpiMode.Mode0,
    DataBitLength = 8
};

using GpioController gpio = new GpioController();
using SpiDevice spi = SpiDevice.Create(settings);

// SPI device, reset, BUSY, DIO1; share gpio and do not dispose it from the driver when appropriate
using (Sx1262 lora = new Sx1262(spi, resetPin: 12, busyPin: 13, dio1Pin: 14, gpioController: gpio, shouldDispose: false))
{
    lora.Reset();
    lora.Initialize();

    lora.PacketReceived += (sender, msg) =>
    {
        string text = Encoding.UTF8.GetString(msg.Payload, 0, msg.Payload.Length);
        Debug.WriteLine("RX: '" + text + "' RSSI=" + msg.Rssi + " dBm SNR=" + msg.Snr + " dB");
    };

    lora.StartPolling();

    while (true)
    {
        string message = "Hello from .NET nanoFramework: " + DateTime.UtcNow;
        Debug.WriteLine(message);
        lora.Send(Encoding.UTF8.GetBytes(message), timeoutMs: 3000);
        Thread.Sleep(10_000);
    }
}
```

For **STM32** and other targets, use your platform’s SPI preset pins and chip select; you do not need **`nanoFramework.Hardware.Esp32`**.

**Usage notes**

- Call **`Reset()`** then **`Initialize()`** once before TX/RX.
- **`Send`** must not be called from the **`PacketReceived`** callback (poll thread); send from the main loop or another worker thread.
- Full error handling, cleanup, and **`StopPolling`** are in **`samples/Sx1262Sample/Program.cs`**.

### Result

After deployment, use the debug console to see startup logs, transmitted lines, and received packets with RSSI/SNR. You can capture a screenshot and add **`RunningResult.jpg`** here, as in the [Nrf24l01 sample](https://github.com/nanoframework/nanoFramework.IoT.Device/tree/main/devices/Nrf24l01).
