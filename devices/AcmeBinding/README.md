# AcmeBinding

A synthetic, hardware-free device binding used only to exercise every [`System.Device.Model`](../System.Device.Model/README.md) attribute case in one place — `[Interface]`, `[Telemetry]`, `[Property]`, `[Command]`, and `[Component]` — so tooling that reflects over this metadata (such as the [AI-Native IoT MCP generation](../../src/hackaton-2026/kickoff-slides.md) hackathon project) can be built and tested without real hardware.

It is not a real device driver and does not talk to any bus (I2C/SPI/GPIO). `AcmeDevice` simulates readings and state entirely in memory.

## Attribute coverage

| Case | Member |
|---|---|
| `[Telemetry]` — no-arg property, UnitsNet-typed | `AcmeDevice.Temperature` |
| `[Telemetry]` — no-arg method returning a value | `AcmeDevice.GetUptimeSeconds()` |
| `[Telemetry]` — method returning bool with one `out` arg | `AcmeDevice.TryReadOrientation(out Vector3)` |
| `[Property]` — read-only | `AcmeDevice.FirmwareVersion` |
| `[Property]` — writable, single get/set pair | `AcmeDevice.SamplingRateHz` |
| `[Property]` — read-only, non-primitive (enum) type | `AcmeDevice.CurrentMode` |
| `[Property]` — writable, separately-named getter/setter methods merged by name | `AcmeDevice.GetThreshold()` / `AcmeDevice.SetThreshold(double)` |
| `[Command]` — zero parameters | `AcmeDevice.Reset()` |
| `[Command]` — one parameter, non-primitive (enum) type | `AcmeDevice.SetMode(AcmeMode)` |
| `[Command]` — one parameter, non-primitive (`System.Drawing.Color`) type | `AcmeDevice.SetStatusLedColor(Color)` |
| `[Command]` — multiple (3) parameters | `AcmeDevice.Calibrate(double, double, int)` |
| `[Component]` — nested `[Interface]`-annotated sub-device | `AcmeDevice.Beeper` (`AcmeBeeperModule`) |

`AcmeBeeperModule` (the `[Component]`) has its own `[Telemetry]` (`IsBeeping`) and two `[Command]`s (`Beep`, `StopBeep`), so nesting/flattening has something real to walk.

## Usage

```csharp
using Iot.Device.AcmeBinding;

AcmeDevice device = new AcmeDevice();

device.SetMode(AcmeMode.Active);
device.Calibrate(offset: 0.5, scale: 1.02, iterations: 3);
device.Beeper.Beep(200);
```

See the [samples](samples/Program.cs) for a runnable example.
