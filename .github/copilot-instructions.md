# Copilot Instructions for nanoFramework.IoT.Device

## Repository Overview

This repository contains **device bindings** (drivers) for sensors, displays, and other hardware peripherals targeting [.NET nanoFramework](https://www.nanoframework.net/) — a free, open-source implementation of .NET for constrained embedded devices (ESP32, STM32, etc.). Bindings are ported primarily from the [.NET IoT](https://github.com/dotnet/iot) repository and adapted to nanoFramework's constraints.

Each binding is published as a NuGet package with the ID pattern `nanoFramework.Iot.Device.<DeviceName>`.

---

## Repository Structure

```
devices/           # One subfolder per device binding (e.g. devices/Bmp180/)
  <DeviceName>/
    <DeviceName>.nfproj       # nanoFramework project file (NOT .csproj)
    <DeviceName>.sln          # Solution file
    <DeviceName>.nuspec       # NuGet package definition
    <DeviceName>.cs           # Main device driver class(es)
    Register.cs               # Register map (if applicable)
    Properties/AssemblyInfo.cs
    Settings.StyleCop         # StyleCop settings for this device
    packages.config           # NuGet package references (packages.config style, NOT PackageReference)
    packages.lock.json
    version.json              # Nerdbank.GitVersioning config
    category.txt              # Device category for listing generation
    README.md                 # Usage documentation with code examples
    samples/
      <DeviceName>.Sample.nfproj
      Program.cs              # Sample using top-level statements
      Properties/AssemblyInfo.cs
      packages.config
      packages.lock.json
tests/             # Unit tests for shared/common libraries only (not individual device bindings)
src/               # Tooling: code converter, doc generation, shared code
StyleCop/          # Global StyleCop settings and sync scripts
assets/            # Shared assets (logo, etc.)
.pipeline-assets/  # Azure Pipelines build scripts
```

---

## Project System

- **Project files use `.nfproj` extension**, not `.csproj`. This is the nanoFramework MSBuild project system.
- The project GUID type is `{11A8DD76-328B-46DF-9F39-F559912D0360}` for nanoFramework projects.
- `TargetFrameworkVersion` is `v1.0` (nanoFramework-specific, not .NET version).
- `LangVersion` is `9.0`.
- All assemblies are **signed** with `devices/key.snk`.
- **NuGet packages use `packages.config` style**, not `<PackageReference>`. Both the `.nfproj` and `packages.config` must be kept in sync when adding/updating dependencies.
- `RestorePackagesWithLockFile` is `true`; lock files are committed.
- StyleCop integration via `StyleCop.MSBuild` NuGet package; `StyleCopTreatErrorsAsWarnings` is `false` (errors fail the build).

---

## nanoFramework vs .NET Differences — Critical Constraints

When writing or modifying code, always apply these rules:

### Types
- Use `SpanByte` instead of `Span<byte>`. (`SpanByte` is nanoFramework's equivalent.)
- `System.Numerics.Vector3` uses **`double`** components (not `float`).
- No multidimensional arrays (`[,]`). Use jagged arrays (`[][]`) instead.
- No `Queue<T>` or other generic collections. Use `ArrayList` with explicit casts.
- No `Console` class. Use `Debug.WriteLine` for output in both library and sample code.
- No `Enum.GetValues` / `Enum.IsDefined` — these are unsupported; remove or replace them.
- `unsafe` blocks require `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` in the `.nfproj`.

### Patterns
- Infinite loops use `Thread.Sleep(Timeout.Infinite)` or `while (true) { }`.
- I2C device creation: `new I2cDevice(settings)` or `I2cDevice.Create(settings)`.
- For ESP32 I2C, pin functions must be configured before use:
  ```csharp
  Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);   // SDA
  Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);  // SCL
  ```
  GPIO 21 (SDA) and GPIO 22 (SCL) are the default ESP32 I2C1 pins used in all samples.
- SPI and other buses follow similar ESP32 pin configuration patterns.

---

## Code Conventions

### Namespaces
- Library namespace: `Iot.Device.<DeviceName>` (e.g., `Iot.Device.Bmp180`).
- Assembly name: `Iot.Device.<DeviceName>`.
- NuGet package name: `nanoFramework.Iot.Device.<DeviceName>`.

### Device Classes
- Main device class is named after the device (e.g., `Bmp180`, `At24cxx`).
- Must implement `IDisposable` and call `Dispose()` on owned `I2cDevice`/`SpiDevice`.
- Decorated with `[Interface("...")]` attribute from `System.Device.Model`.
- `DefaultI2cAddress` or `DefaultSpiChipSelectLine` constant exposed as `public const`.
- Private fields prefixed with `_` (e.g., `_i2cDevice`, `_calibrationData`).

### XML Documentation
- All `public` members must have XML doc comments (`<summary>`, `<param>`, `<returns>`, `<exception>` as appropriate).
- `internal` and `private` members are exempt (StyleCop settings: `IgnorePrivates=True`, `IgnoreInternals=True`).

### File Headers
- Every `.cs` file starts with:
  ```csharp
  // Licensed to the .NET Foundation under one or more agreements.
  // The .NET Foundation licenses this file to you under the MIT license.
  ```

### AssemblyInfo.cs
- Every project has `Properties/AssemblyInfo.cs` with `AssemblyTitle`, `AssemblyCompany`, and `AssemblyCopyright`.

---

## Adding a New Device Binding

To add a new device, create a folder under `devices/<DeviceName>/` with these files:

1. **`<DeviceName>.nfproj`** — Copy from an existing binding (e.g., `devices/Bmp180/Bmp180.nfproj`) and update:
   - `ProjectGuid` (generate a new GUID)
   - `RootNamespace` and `AssemblyName` → `Iot.Device.<DeviceName>`
   - `DocumentationFile`
   - `AssemblyOriginatorKeyFile` → `..\key.snk`
   - NuGet `Reference` items and `Compile` items
2. **`<DeviceName>.sln`** — Solution referencing the `.nfproj` and the samples `.nfproj`.
3. **`<DeviceName>.nuspec`** — NuGet spec; `id` = `nanoFramework.Iot.Device.<DeviceName>`.
4. **`packages.config`** — List all NuGet dependencies with `targetFramework="netnano1.0"`.
5. **`version.json`** — Copy verbatim from any existing device (e.g., `devices/Bmp180/version.json`).
6. **`Settings.StyleCop`** — Copy from any existing device.
7. **`category.txt`** — One-line category string (e.g., `Sensor`).
8. **`README.md`** — Document usage, wiring, and provide a code sample.
9. **`Properties/AssemblyInfo.cs`** — Standard assembly attributes.
10. **`samples/`** — Sample project with `Program.cs` using top-level statements.

---

## Building

Building requires the **nanoFramework MSBuild components** (`InstallNanoMSBuildComponents`) and runs on **Windows** (the CI uses `windows-latest`). The standard Linux toolchain cannot build `.nfproj` files.

NuGet restore: use `nuget restore` (not `dotnet restore`) since projects use `packages.config`.

The build pipeline script is `.pipeline-assets/pipeline-build-solutions.PS1`.

There are no standard `dotnet build` or `dotnet test` commands for device bindings. Unit tests exist only for shared libraries under `tests/` and also use `.nfproj`.

---

## Versioning

Each device binding has its own `version.json` using [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning):
```json
{
  "version": "2.0-preview.{height}",
  "semVer1NumericIdentifierPadding": 3,
  "nuGetPackageVersion": { "semVer": 2.0 },
  "publicReleaseRefSpec": ["^refs/heads/develop$", "^refs/heads/main$", "^refs/heads/v\\d+(?:\\.\\d+)?$"]
}
```
The `$version$` and `$commit$` tokens in `.nuspec` files are filled by `nanovc` (the nanoFramework versioning CLI tool).

---

## Common Dependencies (NuGet Packages)

| Package | Purpose |
|---|---|
| `nanoFramework.CoreLibrary` | Base class library (mscorlib) |
| `nanoFramework.System.Device.I2c` | I2C bus |
| `nanoFramework.System.Device.Spi` | SPI bus |
| `nanoFramework.System.Device.Gpio` | GPIO |
| `nanoFramework.System.Device.Model` | `[Interface]` attribute |
| `nanoFramework.System.Buffers.Binary.BinaryPrimitives` | `BinaryPrimitives` helpers |
| `nanoFramework.System.Math` | `Math` class |
| `nanoFramework.UnitsNet.*` | Units of measurement |
| `nanoFramework.Hardware.Esp32` | ESP32-specific (`Configuration.SetPinFunction`) |
| `nanoFramework.Iot.Device.Common.WeatherHelper` | Weather calculations |
| `Nerdbank.GitVersioning` | Versioning (dev dependency) |
| `StyleCop.MSBuild` | Style enforcement (dev dependency) |

---

## Samples Pattern

All samples use C# top-level statements (no explicit `Main` method). The standard pattern:

```csharp
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using Iot.Device.<DeviceName>;
// For ESP32:
// using nanoFramework.Hardware.Esp32;

// When connecting to an ESP32, configure I2C GPIOs:
// Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
// Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

const int busId = 1;
I2cConnectionSettings settings = new I2cConnectionSettings(busId, <DeviceClass>.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

// ... use device ...

Thread.Sleep(Timeout.Infinite);
```

---

## Migrating from .NET IoT

The `src/nanoFramework.IoT.Device.CodeConverter` tool automates much of the migration from .NET IoT. Key manual adjustments:
- Replace `Span<byte>` with `SpanByte`.
- Replace `Console.WriteLine` with `Debug.WriteLine`.
- Remove or replace unsupported APIs (`Queue<T>`, `Enum.GetValues`, multidimensional arrays, etc.).
- Remove Raspberry Pi-specific code; replace with ESP32/MCU equivalents.
- Replace infinite `while(!Console.KeyAvailable)` with `while(true)` + `Thread.Sleep(Timeout.Infinite)`.
- Replace `GpioController` usage patterns — nanoFramework uses `GpioPin` directly.

See `tips-trick.md` and `migrate-binding-to-dotnetiot.md` for detailed migration guidance.

---

## Branches and CI

- `main` — stable releases
- `develop` — active development; PRs should target this branch
- `release-*` — release branches

The CI pipeline (`azure-pipelines.yml`) runs on Windows, installs nanoFramework MSBuild components, restores NuGet packages, then builds and packs all device solutions. Dependency updates are automated via five scheduled GitHub Actions workflows (`.github/workflows/update-dependencies-*.yml`).
