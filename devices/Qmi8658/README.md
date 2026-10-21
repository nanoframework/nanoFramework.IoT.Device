# QMI8658 - Six-axis accelerometer and gyroscope

The QST QMI8658 combines a three-axis accelerometer and a three-axis gyroscope.
This I2C binding supports configurable ranges and normal-mode output data rates,
raw-axis acceleration in **g**, angular rate in **degrees per second**, and internal
die temperature as a UnitsNet `Temperature`. It follows the `Vector3` and
`GetAccelerometer()` / `GetGyroscope()` / `GetInternalTemperature()` conventions of
the MPU6886 and BMI270 bindings. nanoFramework's `Vector3` uses `double` components.

## Documentation

- [Waveshare ESP32-S3-Touch-LCD-2 documentation](https://www.waveshare.com/wiki/ESP32-S3-Touch-LCD-2)
- [Board schematic](https://files.waveshare.com/wiki/ESP32-S3-Touch-LCD-2/ESP32-S3-Touch-LCD-2-SchDoc.pdf)
- [Vendor examples and QMI8658C datasheet](https://files.waveshare.com/wiki/ESP32-S3-Touch-LCD-2/ESP32-S3-Touch-LCD-2-Demo.zip)
  (datasheet: `Arduino/libraries/FastIMU/datasheets/QMI8658C-Datasheet.pdf`)

## Wiring

The Waveshare ESP32-S3-Touch-LCD-2 already connects the IMU to the following bus:

| Signal | ESP32-S3 GPIO |
| --- | --- |
| SDA | GPIO48 |
| SCL | GPIO47 |
| I2C address | 0x6B (`SecondaryI2cAddress`) |

The controller also supports address 0x6A (`DefaultI2cAddress`). Use the address
configured by your module. The expected WHO_AM_I register is 0x05; this is not an
I2C address. Separate modules require appropriate power, a common ground and I2C
pull-ups according to their datasheet. Configure ESP32 pins **before** creating
an I2C device. STM32 targets use their preset hardware I2C pins.

The touch controller and other peripherals share this board's bus. Use a separate
`I2cDevice` for each address; do not reset or reconfigure the bus in each driver.
The sample uses polling and needs no interrupt GPIO or graphics package.

## Usage

```csharp
using System.Device.I2c;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Iot.Device.Qmi8658;
using nanoFramework.Hardware.Esp32;

Configuration.SetPinFunction(48, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(47, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Qmi8658AccelerometerGyroscope.SecondaryI2cAddress);
using Qmi8658AccelerometerGyroscope imu = new Qmi8658AccelerometerGyroscope(new I2cDevice(settings));

while (true)
{
    if (imu.IsDataReady)
    {
        imu.ReadAccelerationAndGyroscope(out Vector3 acceleration, out Vector3 angularRate);
        Debug.WriteLine($"Acceleration (g): {acceleration}");
        Debug.WriteLine($"Angular rate (dps): {angularRate}");
        Debug.WriteLine($"Die temperature: {imu.GetInternalTemperature().DegreesCelsius} C");
    }

    Thread.Sleep(100);
}
```

Defaults are **+/-4 g**, **+/-512 dps**, and **250 Hz** for both sensors. Initialization
checks the identifier, issues software reset (0xB0 to register 0x60), waits 20 ms,
enables address auto-increment and configures both sensors. Read transfers use
little-endian signed 16-bit samples. Temperature is signed raw / 256 degrees C,
with no additional offset. It represents die temperature, not ambient temperature.

## Configuration and units

`AccelerometerScale` supports +/-2, 4, 8 and 16 g. `GyroscopeScale` supports +/-16,
32, 64, 128, 256, 512, 1024 and **2048** dps. These are the actual QMI8658 ranges,
not the 125/250/500/1000/2000 dps ranges used by some other IMUs. Values are scaled
by full-scale / 32768. Acceleration includes gravity: a stationary level board
should measure roughly 1 g on the axis aligned with gravity.

Both `AccelerometerDataRate` and `GyroscopeDataRate` support 8000, 4000, 2000, 1000, 500, 250,
125, 62.5 and 31.25 Hz in normal mode. Low-power accelerometer-only modes are not
exposed because this binding keeps both sensors enabled. Configure multiple
settings in a single operation to avoid repeated stop/start cycles:

```csharp
imu.Configure(
    AccelerometerRange.Range8G,
    GyroscopeRange.Range1024Dps,
    OutputDataRate.Odr125Hz,
    OutputDataRate.Odr125Hz);
```

Each configuration disables both sensors, waits 70 ms for power-down (up to two
periods at the lowest supported ODR), writes the ranges and rates, enables both
low-pass filters at 5.32% of ODR, then restarts both sensors and waits 200 ms for
startup/filter settling. Configuration changes are not intended for every sample.
Invalid enum values throw `ArgumentOutOfRangeException` rather than choosing a
silent default. Failed transfers or a mismatched device ID throw `IOException`;
after a failed configuration, measurements throw `InvalidOperationException` until
`Configure` succeeds. Exception explanations are provided by XML documentation.

`GetAccelerometer()` and `GetGyroscope()` read each sensor independently.
`ReadAccelerationAndGyroscope` reads all six axes in one 12-byte I2C burst; output
updates remain asynchronous, so it does not guarantee simultaneous sample times.
`IsDataReady` checks both STATUS0 ready bits. Measurement calls do not wait for new
data and may repeat the latest sample. The binding does not enable FIFO, external
magnetometers, embedded fusion, self-test, interrupts or calibration. Axes are in
the chip's native orientation; mounting correction and bias calibration belong in
the application. This driver is not thread-safe and reuses its I2C buffers.

Disposing the driver disposes its I2C device by default. Pass `shouldDispose: false`
if the caller retains ownership. Do not access the device after disposal.

## Building and testing

Build the [solution](Qmi8658.sln) using nanoFramework's Windows toolchain:

```powershell
nuget restore Qmi8658.sln
msbuild Qmi8658.sln /p:platform="Any CPU" /p:Configuration=Release /verbosity:minimal
```

The [sample](samples/Program.cs) targets the Waveshare ESP32-S3-Touch-LCD-2.
The [tests](tests/Qmi8658Tests.cs) cover all range/rate encodings, signed little-endian
conversion, scaling limits, combined-buffer offsets, temperature and invalid values.
Build [NFUnitTest.nfproj](tests/NFUnitTest.nfproj), then run it with the nanoFramework
Test Adapter and [emulator runsettings](tests/nano.runsettings). These tests do not
exercise physical I2C. On the board, verify initialization at 0x6B, data-ready polling,
approximately 1 g at rest, near-zero stationary angular rate, axis signs while moving,
and plausible die temperature. Recheck readings after changing ranges and rates.
