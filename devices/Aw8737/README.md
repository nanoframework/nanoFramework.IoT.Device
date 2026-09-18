# AW8737A - Mono Class-K audio power amplifier (speaker output)

The [Awinic AW8737A](https://www.awinic.com/) is a mono Class-K audio power amplifier that drives a speaker from an **analog** audio input (for example the line-out of an ES8311 codec). It is controlled through a single active-low `SHDN` pin. It is used, for example, on the **M5Stack M5StickS3** (paired with an ES8311 codec).

This binding covers the **control pin only** (enable/disable) using a plain **GPIO** pin. The audio signal is analog and is not handled by this binding.

> [!NOTE]
> The AW8737A one-wire protocol can also select operating modes (Mode 1-4 speaker-guard power levels) by emitting one to four 0.75-10 s rising edges on `SHDN`. Those microsecond pulses can't be produced reliably from a managed GPIO, so this binding only enables/disables the amplifier (Mode 1). Generating the mode pulses needs a hardware timer such as the ESP32 RMT peripheral.

## Control interface

Driving `SHDN` high (a rising edge from the shutdown state) enables the amplifier in **Mode 1** &mdash; the highest NCN speaker-guard power level (1.2 W into an 8 load). Driving `SHDN` low shuts it down. The **voltage gain is fixed by the external input resistor** (for example 16.3 V/V with a 3 k resistor), not by software.

## Documentation

- [AW8737A datasheet (Awinic)](https://doc.awinic.com/doc/202504/03c42404-79df-46f8-987d-e80dff05af8a.pdf)
- [M5StickS3 product page](https://docs.m5stack.com/en/core/M5StickS3)

## Usage

```csharp
using Iot.Device.Aw8737;

// The AW8737A SHDN (control) pin wired to a GPIO.
Aw8737 amplifier = new Aw8737(controlPin: 21);

// Enable (Mode 1). The voltage gain is fixed by the external input resistor.
amplifier.Enabled = true;

// Shut down.
amplifier.Enabled = false;
```

Pair this with a DAC / codec such as the `Iot.Device.Es8311` binding to build a complete speaker path.
