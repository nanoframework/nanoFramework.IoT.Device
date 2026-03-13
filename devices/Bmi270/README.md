# BMI270 - accelerometer and gyroscope

The BMI270 is a 6-axis IMU (Inertial Measurement Unit) from Bosch Sensortec that combines a 3-axis accelerometer and a 3-axis gyroscope. It is designed for wearable and IoT applications and includes advanced features such as step counting and gesture recognition.

Key features:

- Accelerometer programmable FSR of ±2g, ±4g, ±8g, and ±16g
- Gyroscope programmable FSR of ±125 dps, ±250 dps, ±500 dps, ±1000 dps, and ±2000 dps
- Internal temperature sensor
- Built-in step counter, activity recognition, and wrist gesture features
- Auxiliary I2C master interface (used for connecting a BMM150 magnetometer on the M5Stack CoreS3)

## Documentation

- [Datasheet](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/K128%20CoreS3/BMI270.PDF)
- [Bosch BMI270-Sensor-API](https://github.com/boschsensortec/BMI270-Sensor-API) — reference C implementation and config file
- This sensor is used on the [M5Stack CoreS3](https://docs.m5stack.com/en/core/CoreS3) development kit.

## Important: Firmware Config Upload

Unlike simpler IMU sensors, the BMI270 **requires an 8 KB configuration file to be uploaded via I2C after every power-on reset**. This binding handles this automatically during initialization. The config file is sourced from the [Bosch BMI270-Sensor-API](https://github.com/boschsensortec/BMI270-Sensor-API) (BSD-3-Clause licensed).

## Usage

Create the `Bmi270AccelerometerGyroscope` class and pass the I2C device. The default I2C address is `0x68` (SDO=GND), and the secondary address is `0x69` (SDO=VDDIO). The M5Stack CoreS3 uses address `0x69`.

```csharp
// I2C pins for the M5Stack CoreS3
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);

using (Bmi270AccelerometerGyroscope imu = new(I2cDevice.Create(settings)))
{
    Debug.WriteLine("BMI270 initialized successfully!");

    Debug.WriteLine("Start calibration ...");
    var offset = imu.Calibrate(1000);
    Debug.WriteLine($"Calibration done, calculated offsets X:{offset.X} Y:{offset.Y} Z:{offset.Z}");

    Debug.WriteLine($"Internal temperature: {imu.GetInternalTemperature().DegreesCelsius} C");

    while (true)
    {
        var acc = imu.GetAccelerometer();
        var gyr = imu.GetGyroscope();
        Debug.WriteLine($"Accelerometer data x:{acc.X} y:{acc.Y} z:{acc.Z}");
        Debug.WriteLine($"Gyroscope data x:{gyr.X} y:{gyr.Y} z:{gyr.Z}\n");
        Thread.Sleep(100);
    }
}
```

### Sample output

```text
BMI270 initialized successfully!
Start calibration ...
Calibration done, calculated offsets X:12.345 Y:-6.789 Z:3.210
Internal temperature: 28.5 C
Accelerometer data x:-0.041 y:0.003 z:1.012
Gyroscope data x:0.183 y:-0.427 z:-0.091

Accelerometer data x:-0.039 y:0.001 z:1.009
Gyroscope data x:0.122 y:-0.381 z:-0.067
```

See [samples](samples) for a complete sample application.

## Calibration

The gyroscope can be calibrated using the `Calibrate` function. During calibration, keep the sensor still and in a fixed position (e.g. lying flat on a table).

```csharp
var offset = imu.Calibrate(1000);
Debug.WriteLine($"Calibration done, offsets X:{offset.X} Y:{offset.Y} Z:{offset.Z}");
```

You can also write directly to the compensation registers using `SetGyroscopeOffset`.

> **Note:** When the device reboots, the offset registers are cleared. Persist calibration data yourself if needed.

## Output Data Rate (ODR)

The accelerometer and gyroscope output data rates can be configured at construction time or changed at runtime:

```csharp
// Set ODR during construction
using var imu = new Bmi270AccelerometerGyroscope(
    I2cDevice.Create(settings),
    accelerometerOdr: AccelerometerOutputDataRate.Odr200Hz,
    gyroscopeOdr: GyroscopeOutputDataRate.Odr400Hz);

// Or change at runtime
imu.AccelerometerOdr = AccelerometerOutputDataRate.Odr50Hz;
imu.GyroscopeOdr = GyroscopeOutputDataRate.Odr100Hz;
```

## Sleep mode

The sensor can be put in suspend mode to save power:

```csharp
imu.Sleep();
// ... later ...
imu.WakeUp();
```

## Step counter and activity recognition

The BMI270 has a built-in step counter and activity recognition engine. Enable it after initialization:

```csharp
// Enable step counter with activity recognition
imu.EnableStepCounter(enableActivityRecognition: true);

// Read the step count
int steps = imu.GetStepCount();
Debug.WriteLine($"Steps: {steps}");

// Read the current activity
ActivityType activity = imu.GetActivity();
Debug.WriteLine($"Activity: {activity}"); // Still, Walking, Running, or Unknown

// Reset step counter to zero
imu.ResetStepCounter();

// Disable when no longer needed
imu.DisableStepCounter();
```

## Interrupts

The BMI270 has two interrupt pins (INT1, INT2) that can be configured for various sources:

```csharp
// Configure INT1 as push-pull, active-high
imu.ConfigureInterruptPin(1, outputEnable: true, activeHigh: true, openDrain: false);

// Map step counter interrupt to INT1
imu.MapFeatureInterrupt(1, FeatureInterruptSource.StepCounter);

// Map data-ready interrupt to INT1
imu.MapDataInterrupt(1, DataInterruptSource.DataReady);

// Read interrupt status (clears latched status)
var featureStatus = imu.GetFeatureInterruptStatus();
var dataStatus = imu.GetDataInterruptStatus();
```

## FIFO

The BMI270 has a 6 KB FIFO buffer for efficient burst reads:

```csharp
// Enable FIFO for accelerometer and gyroscope, 512-byte watermark
imu.EnableFifo(enableAccelerometer: true, enableGyroscope: true, watermarkBytes: 512);

// Read available data
int available = imu.GetFifoByteCount();
SpanByte buffer = new byte[available];
int bytesRead = imu.ReadFifo(buffer);
// In headerless mode: each frame = 12 bytes (6 acc + 6 gyro), little-endian int16

// Flush FIFO
imu.FlushFifo();

// Disable FIFO
imu.DisableFifo();
```

## 9-axis with BMM150 magnetometer (M5Stack CoreS3)

On the M5Stack CoreS3, the BMM150 magnetometer is connected through the BMI270's auxiliary I2C interface. This binding provides a `Bmm150I2cBmi270` adapter that allows the existing `Bmm150` binding to communicate with the magnetometer through the BMI270.

```csharp
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);

using (Bmi270AccelerometerGyroscope imu = new(I2cDevice.Create(settings)))
{
    // Enable auxiliary I2C for BMM150
    imu.EnableAuxiliaryI2c(Bmm150.SecondaryI2cAddress);

    // Create BMM150 using the BMI270 aux I2C bridge
    I2cConnectionSettings bmm150Settings = new(1, Bmi270AccelerometerGyroscope.SecondaryI2cAddress);
    using (Bmm150 mag = new(I2cDevice.Create(bmm150Settings), new Bmm150I2cBmi270(Bmm150.SecondaryI2cAddress)))
    {
        while (true)
        {
            var acc = imu.GetAccelerometer();
            var gyr = imu.GetGyroscope();
            var magData = mag.ReadMagnetometer();
            Debug.WriteLine($"Accel: {acc.X:F3} {acc.Y:F3} {acc.Z:F3}");
            Debug.WriteLine($"Gyro:  {gyr.X:F3} {gyr.Y:F3} {gyr.Z:F3}");
            Debug.WriteLine($"Mag:   {magData.X:F3} {magData.Y:F3} {magData.Z:F3}");
            Thread.Sleep(100);
        }
    }
}
```

## M5Stack CoreS3

On the M5Stack CoreS3, the BMI270 is connected to the internal I2C bus at address `0x69`. The BMM150 magnetometer is also on the board, connected through the BMI270's auxiliary I2C interface at address `0x10`. See the [CoreS3 documentation](https://docs.m5stack.com/en/core/CoreS3) for the full pinout.

| Device | I2C Address |
| ------ | ----------- |
| BMI270 | 0x69 |
| BMM150 | 0x10 (via BMI270 aux I2C) |
