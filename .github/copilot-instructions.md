# Copilot Instructions for nanoFramework.IoT.Device

## Repository Overview

This repository contains **IoT device bindings** (drivers) for [.NET nanoFramework](https://www.nanoframework.net/) — a free, open-source platform that enables running C# on resource-constrained embedded devices (MCUs) such as ESP32 and STM32. Each binding is a standalone NuGet package targeting the `netnano1.0` framework, **not** standard .NET or .NET Core.

The bindings are adapted from the [.NET IoT repository](https://github.com/dotnet/iot) with changes needed for the nanoFramework runtime.

---

## Directory Structure

```
devices/              ← One sub-folder per device binding (primary working area)
  <DeviceName>/
    <DeviceName>.nfproj        ← Main library project (NOT .csproj)
    <DeviceName>.sln           ← Solution file (includes main, samples, tests)
    <DeviceName>.nuspec        ← NuGet packaging descriptor
    packages.config            ← NuGet package references
    packages.lock.json         ← Locked NuGet packages
    version.json               ← Nerdbank.GitVersioning configuration
    category.txt               ← Device category tags (for README auto-generation)
    Settings.StyleCop          ← StyleCop rules
    README.md                  ← Device documentation
    Properties/AssemblyInfo.cs ← Assembly metadata
    samples/                   ← Sample application project (.nfproj)
    tests/                     ← Unit test project (.nfproj, nano.runsettings)

src/                  ← Tooling and helper projects (not device bindings)
  nanoFramework.IoT.Device.CodeConverter/  ← Converts .NET IoT code to nanoFramework
  device-listing/              ← Generates the devices/README.md
  devices_generated/           ← Auto-generated bindings (do not manually edit)

.pipeline-assets/
  pipeline-build-solutions.PS1 ← Azure Pipelines build script (Windows/MSBuild)

azure-pipelines.yml   ← CI/CD configuration (Azure Pipelines, Windows)
```

---

## nanoFramework vs Standard .NET — Critical Differences

**These constraints apply to ALL code in `devices/` (library and sample code):**

### What is NOT available in nanoFramework
- **No `Console`** — use `Debug.WriteLine()` instead (from `System.Diagnostics`)
- **No generic collections** — no `List<T>`, `Queue<T>`, `Dictionary<K,V>`. Use `ArrayList` with casts
- **No multidimensional arrays `[,]`** — use jagged arrays `[][]` instead
- **No `Enum.GetValues()` / `Enum.IsDefined()`** — use switch statements or remove the check
- **No `Span<byte>`** — use `SpanByte` (nanoFramework type)
- **Limited LINQ** — avoid LINQ in library code
- **No `stackalloc` in many contexts** — allocate with `new byte[]` instead
- **No generics in all scenarios** — generics are partially supported; test carefully
- **No `Thread.Sleep(Timeout.Infinite)` in samples** — use a `while(true)` loop

### nanoFramework-specific APIs
- `SpanByte` replaces `Span<byte>` for I2C/SPI buffers
- `System.Diagnostics.Debug.WriteLine()` for output (shows in VS debug window / serial output)
- `nanoFramework.Hardware.Esp32.Configuration.SetPinFunction()` to configure GPIO pins on ESP32
- `nanoFramework.TestFramework` for unit tests (attributes: `[TestClass]`, `[TestMethod]`, `[Setup]`)
- `UnitsNet` NuGet packages (nanoFramework-specific builds, e.g. `nanoFramework.UnitsNet.Temperature`) for physical measurements

### Vector3 / System.Numerics
The `devices/System.Numerics/` implementation uses `double` components (X, Y, Z) — **not** `float` as in standard .NET.

---

## Project Conventions

### Naming
- **NuGet package ID**: `nanoFramework.Iot.Device.<DeviceName>`
- **Assembly name**: `Iot.Device.<DeviceName>`
- **Root namespace**: `Iot.Device.<DeviceName>`
- **Project GUID type**: `{11A8DD76-328B-46DF-9F39-F559912D0360}` (nanoFramework project type)

### File Headers
Every `.cs` source file must start with:
```csharp
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
```

### Strong Name Signing
All library projects are signed with the shared `devices/key.snk` key. Each `.nfproj` must have:
```xml
<SignAssembly>true</SignAssembly>
<AssemblyOriginatorKeyFile>..\key.snk</AssemblyOriginatorKeyFile>
<DelaySign>false</DelaySign>
```

### StyleCop
StyleCop is enforced via `StyleCop.MSBuild` NuGet. Errors (not just warnings) are expected to be fixed. The `Settings.StyleCop` file in each device folder configures the rules.

### Code Style
- XML documentation comments (`/// <summary>`) are required on all public APIs
- Constructor pattern: accept `I2cDevice` or `SpiDevice` as the primary argument
- Use `IDisposable` and dispose the underlying device in `Dispose()`
- Constants for default I2C addresses: `public const byte DefaultI2cAddress = 0x...;`

---

## I2C Sample Code Pattern

For ESP32, always configure I2C GPIO pins before creating `I2cDevice`. The standard pins for I2C bus 1 are GPIO21 (SDA) and GPIO22 (SCL):

```csharp
// Setup ESP32 I2C port (required before creating I2cDevice on ESP32)
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, MyDevice.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);
MyDevice device = new MyDevice(i2cDevice);
```

For STM32 and other targets, use the preset hardware I2C pins (no `SetPinFunction` call needed).

---

## Unit Testing

- Test projects use `nanoFramework.TestFramework` (not xUnit, MSTest, or NUnit)
- Test class attribute: `[TestClass]` on the class
- Test method attribute: `[TestMethod]` on each test
- Setup attribute: `[Setup]` on the setup method
- Use `Assert.Equal(expected, actual)`, `Assert.True(condition)`, etc.
- Use `Assert.SkipTest("reason")` to skip tests that need real hardware
- Each test project must have a `nano.runsettings` with `<IsRealHardware>False</IsRealHardware>` to run in the nanoCLR emulator
- Test project output type is `Library` with `<IsTestProject>true</IsTestProject>`

Example `nano.runsettings`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <MaxCpuCount>1</MaxCpuCount>
    <ResultsDirectory>.\TestResults</ResultsDirectory>
    <TestSessionTimeout>120000</TestSessionTimeout>
    <TargetFrameworkVersion>net48</TargetFrameworkVersion>
    <TargetPlatform>x64</TargetPlatform>
  </RunConfiguration>
  <nanoFrameworkAdapter>
    <Logging>None</Logging>
    <IsRealHardware>False</IsRealHardware>
  </nanoFrameworkAdapter>
</RunSettings>
```

---

## Build System

### IMPORTANT: Cannot build on Linux
The build system requires **Windows + Visual Studio + MSBuild** with the nanoFramework project system components installed. The `dotnet build` / `dotnet restore` commands do **not** work for `.nfproj` files.

**CI runs on Azure Pipelines (Windows), not GitHub Actions.**

To build manually on a supported Windows machine:
```powershell
nuget restore <DeviceName>.sln
msbuild <DeviceName>.sln /p:Configuration=Release /p:Platform="Any CPU"
```

### NuGet Restore
Use `nuget restore <SolutionFile>.sln` (not `dotnet restore`). The packages folder is local to each device (`devices/<DeviceName>/packages/`).

### Versioning
Uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning). Each device has a `version.json` that sets the base version (e.g., `"version": "1.2"`). The full version is computed from the git history.

### Pipeline Build Script
The CI script `.pipeline-assets/pipeline-build-solutions.PS1` automatically detects which device folders changed in a PR/commit and only builds those. It:
1. Runs `nuget restore`
2. Runs `nanovc` for version checks
3. Runs `msbuild` in Release mode
4. Runs `nuget pack` to produce `.nupkg` and `.snupkg`

---

## Adding a New Device Binding

When adding a new binding under `devices/<NewDevice>/`, the following files are required:

1. **`<NewDevice>.nfproj`** — library project targeting netnano1.0
2. **`<NewDevice>.sln`** — solution with main lib, samples, and tests projects
3. **`<NewDevice>.nuspec`** — NuGet spec with proper ID, description, and dependencies
4. **`packages.config`** — lists NuGet dependencies with exact versions
5. **`packages.lock.json`** — locked package list (generated by NuGet restore)
6. **`version.json`** — Nerdbank.GitVersioning config (copy from another device, set correct version)
7. **`category.txt`** — one category tag per line (e.g., `thermometer`, `i2c`)
8. **`Settings.StyleCop`** — copy from another device folder
9. **`README.md`** — must include: description, datasheet links, wiring instructions, code sample
10. **`Properties/AssemblyInfo.cs`** — with correct assembly title and copyright
11. **`samples/<NewDevice>.samples.nfproj`** + **`samples/Program.cs`**
12. **`tests/NFUnitTest.nfproj`** + **`tests/nano.runsettings`** + test `.cs` files

Use an existing device (e.g., `devices/At24cxx/` or `devices/Bmxx80/`) as a template.

---

## Common NuGet Packages Used

| Purpose | Package ID |
|---|---|
| Core library | `nanoFramework.CoreLibrary` |
| I2C | `nanoFramework.System.Device.I2c` |
| SPI | `nanoFramework.System.Device.Spi` |
| GPIO | `nanoFramework.System.Device.Gpio` |
| Math | `nanoFramework.System.Math` |
| Temperature unit | `nanoFramework.UnitsNet.Temperature` |
| Pressure unit | `nanoFramework.UnitsNet.Pressure` |
| Humidity unit | `nanoFramework.UnitsNet.RelativeHumidity` |
| Binary primitives | `nanoFramework.System.Buffers.Binary.BinaryPrimitives` |
| Device model attributes | `nanoFramework.System.Device.Model` |
| Unit tests | `nanoFramework.TestFramework` |
| ESP32 hardware | `nanoFramework.Hardware.Esp32` |
| Versioning (dev) | `Nerdbank.GitVersioning` |
| StyleCop (dev) | `StyleCop.MSBuild` |

---

## Cross-Device Dependencies

Some devices depend on other device libraries in this repo (not just NuGet packages):

- **NFC devices** (`Mfrc522`, `Pn5180`, `Pn532`) depend on `devices/Card/Card.sln`
- **Mpu9250** depends on `devices/Ak8963/`
- These dependencies must have `nuget restore` run on them before building the dependent device

---

## Known Errors and Workarounds

### `dotnet build` / `dotnet restore` fail on `.nfproj`
**Error**: `error MSB4019: The imported project "NFProjectSystem.Default.props" was not found`  
**Cause**: `.nfproj` files require the nanoFramework MSBuild components, which are only available on Windows with the nanoFramework extension installed.  
**Workaround**: All building must be done via `nuget restore` + `msbuild` on a configured Windows machine or through Azure Pipelines CI.

### StyleCop warnings treated as errors
**Cause**: `<StyleCopTreatErrorsAsWarnings>false</StyleCopTreatErrorsAsWarnings>` means StyleCop issues fail the build.  
**Workaround**: Ensure all public members have XML doc comments, and follow the existing code style.

### NuGet package restore fails with locked mode
**Cause**: `<RestoreLockedMode>true</RestoreLockedMode>` is active in CI.  
**Workaround**: Run `nuget restore` once without CI mode to update `packages.lock.json`, then commit it.

### Missing `packages.lock.json` causes CI build failure
**Cause**: `RestoreLockedMode` requires an up-to-date lock file.  
**Workaround**: After adding or updating NuGet packages, regenerate `packages.lock.json` by running `nuget restore` locally on Windows.
