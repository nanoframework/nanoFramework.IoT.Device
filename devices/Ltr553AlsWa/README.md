# LTR-553ALS-WA - Proximity and Ambient Light Sensor

The LTR-553ALS-WA is a combined proximity sensor (PS) and ambient light sensor (ALS) from Lite-On Technology, communicating over I2C. It features an 11-bit proximity sensor with built-in IR LED and a dual-channel 16-bit ambient light sensor (visible+IR and IR-only channels). This sensor is found on the M5Stack CoreS3 board.

## Documentation

- [LTR-553ALS-WA Datasheet (PDF)](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/K128%20CoreS3/LTR-553ALS-WA.PDF)
- [M5Stack CoreS3 Documentation](https://docs.m5stack.com/en/core/CoreS3)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

### Basic Reading

```csharp
using Ltr553AlsWa sensor = new(I2cDevice.Create(new I2cConnectionSettings(1, Ltr553AlsWa.DefaultI2cAddress)));
Debug.WriteLine($"Part ID: 0x{sensor.PartId:X2}, Manufacturer ID: 0x{sensor.ManufacturerId:X2}");

// Enable both sensors
sensor.PsEnabled = true;
sensor.AlsEnabled = true;

// Allow the sensor to take a first measurement after mode changes.
Thread.Sleep(600);

while (true)
{
    try
    {
        bool psReady = sensor.IsPsDataReady();
        bool alsReady = sensor.IsAlsDataReady();

        if (psReady && alsReady)
        {
            ushort proximity = sensor.GetProximity(out bool saturated);
            sensor.GetAlsData(out ushort ch0, out ushort ch1);
            Debug.WriteLine($"Proximity: {proximity}{(saturated ? " [SATURATED]" : string.Empty)}, ALS CH0 (visible+IR): {ch0}, ALS CH1 (IR): {ch1}");
        }
        else
        {
            Debug.WriteLine($"Waiting for fresh data... PS ready: {psReady}, ALS ready: {alsReady}");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Exception: {ex.Message}");
    }

    Thread.Sleep(500);
}
```

Example of output including some debug low level reads:

```text
Hello LTR-553ALS-WA!
CoreS3 internal bus power enabled: AW9523 BUS_EN + BOOST_EN.
AXP2101 Chip ID: 0x4A (expected 0x4A)
CoreS3 power rails enabled: ALDO1/2/3/4 + BLDO1/2.
LTR553 low-level probe on I2C1 @ 0x23:
  ALS_CONTR (0x80): status=1, bytesW=2, value=0x01
  PS_CONTR (0x81): status=1, bytesW=2, value=0x02
  PART_ID (0x86): status=1, bytesW=2, value=0x92
  MANUFAC_ID (0x87): status=1, bytesW=2, value=0x05
  ALS_PS_STATUS (0x8C): status=1, bytesW=2, value=0x05
Part ID: 0x92, Manufacturer ID: 0x05
Proximity: 5, ALS CH0 (visible+IR): 13, ALS CH1 (IR): 18
Proximity: 0, ALS CH0 (visible+IR): 13, ALS CH1 (IR): 17
Proximity: 0, ALS CH0 (visible+IR): 13, ALS CH1 (IR): 17
Proximity: 2, ALS CH0 (visible+IR): 13, ALS CH1 (IR): 17
Proximity: 1, ALS CH0 (visible+IR): 13, ALS CH1 (IR): 18
Proximity: 145, ALS CH0 (visible+IR): 2, ALS CH1 (IR): 2
Proximity: 408, ALS CH0 (visible+IR): 0, ALS CH1 (IR): 1
Proximity: 432, ALS CH0 (visible+IR): 0, ALS CH1 (IR): 0
Proximity: 436, ALS CH0 (visible+IR): 0, ALS CH1 (IR): 0
Proximity: 442, ALS CH0 (visible+IR): 0, ALS CH1 (IR): 0
Proximity: 441, ALS CH0 (visible+IR): 0, ALS CH1 (IR): 0
Proximity: 0, ALS CH0 (visible+IR): 5, ALS CH1 (IR): 8
```

> [!Note] This sample contains specific code for the M5Stack CoreS3.

### Configuring ALS

```csharp
// Set higher gain for low-light conditions
sensor.AlsGain = AlsGain.Gain48X;
sensor.AlsIntegrationTime = AlsIntegrationTime.Integration200Ms;
sensor.AlsMeasurementRate = AlsMeasurementRate.Rate500Ms;
```

### Configuring Proximity LED

```csharp
sensor.LedPulseFrequency = LedPulseFrequency.Frequency60kHz;
sensor.LedDutyCycle = LedDutyCycle.DutyCycle100Percent;
sensor.LedPeakCurrent = LedPeakCurrent.Current100mA;
sensor.LedPulseCount = 1;
```

### Interrupt Configuration

```csharp
// Set PS interrupt thresholds (fire when proximity > 500 or < 100)
sensor.SetPsThreshold(lower: 100, upper: 500);
sensor.SetInterruptMode(InterruptMode.PsOnly);
sensor.SetInterruptPersistence(psCount: 4, alsCount: 0);
```

### Software Reset

```csharp
// Reset the sensor to default state (both subsystems go to standby)
sensor.Reset();
```

## Features

- **Proximity sensing**: 11-bit resolution (0–2047), higher values indicate closer objects, with saturation detection
- **Dual-channel ambient light**: Channel 0 (visible + IR) and Channel 1 (IR only), 16-bit each
- **Configurable LED drive**: Pulse frequency (30–100 kHz), duty cycle (25–100%), peak current (5–100 mA), and pulse count (1–15)
- **Configurable ALS**: Gain (1x–96x) and integration time (50–400 ms)
- **Threshold-based interrupts**: Configurable thresholds and persistence for both PS and ALS
- **Software reset**: Reset sensor to default state

## M5Stack CoreS3

On the M5Stack CoreS3, the LTR-553ALS-WA is on the internal system I2C bus at address 0x23, shared with other peripherals (AXP2101, BMI270, BM8563, etc.). The sensor is paired with the GC0308 camera on the same ribbon cable. Refer to the [CoreS3 pin map](https://docs.m5stack.com/en/core/CoreS3) for I2C bus configuration details.
