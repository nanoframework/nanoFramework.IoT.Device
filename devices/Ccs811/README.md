# CCS811 Gas sensor

CCS811 is an ultra-low power digital gas sensor solution for monitoring indoor air quality.
CCS811 integrates a gas sensor solution for detecting low levels of Volatile Organic Compounds typically found indoors, with a microcontroller unit (MCU) and an Analog-to-Digital converter to monitor the local environment and provide an indication of the indoor air quality via an equivalent CO2 or Total Volatile Organic Compounds output over a standard I2C digital interface.

## Documentation

- This device can be found in multiple places like the [Adafruit](https://www.adafruit.com/product/3566) or [Sparkfun](https://www.sparkfun.com/products/14193) and a lot of different implementations on sites like [Banggood](https://www.banggood.com/search/ccs811.html?from=nav).
- Device [documentation](https://www.sciosense.com/products/environmental-sensors/ccs811-gas-sensor-solution/)

## Device information

*Important*:

- CCS811 needs 20 minutes to warm up before giving any accurate measurement. Once, you'll select a mode and the internal resistor will start heating, keep in mind that accurate results will show up after 20 minutes approximately
- When you'll receive it, the device needs to be put on reading mode every second for about 48h as it needs time to get a a stable internal resistor
- The sensor autocalibrate over time. There is a notion of baseline. This baseline should be handle with care and is not the same for all the devices. Also it does evolve over time.

CCS811 exposes 3 pins, here is a short information on every one:

- The Address pins allows you to select the first of second I2C address. Place it to the ground to select the first one (0x5A) or to VCC to select the second one (0x5B).
- The Reset pin is sometime present. If present and you want to use it, this will perform a full hard reset.
- The Wake pin is used to select the chip and wake it up. If you don't want to use it, just put it to the ground.
- The Interrupt pin allows interruption, if used, the interrupt mode and events will be activated. This needs to be activated to be able to use the embedded Threshold feature.

Understanding the measurement:

- CCS811 provides equivalent CO2 in part per millions as well as Total Volatile Organic Compounds in part per billion. Those equivalents are calculated based on the own internal mechanism
- You have as well the raw data reading from the current gas sensor in micro Ampere and the raw voltage ADC. The ADC voltage is 1.65 V for a reading 1023 in a linear mode.

**Important** to understand:

In order to have this sensor working on an MCU, you need to lower the bus speed. This sensor uses a mode called I2C stretching and it *may* not be supported natively on your MCU. So you **must** lower the I2C clock to the minimum to make it working properly or use a software I2C with a low clock as well.

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the preset pins for the I2C bus you want to use.

You'll find below how to use the sensor. A full example covering in details all the usage can be found in the [samples directory](./samples).

### Create the device

To create a device without any of the pins:

```csharp
var ccs811 = new Ccs811Sensor(I2cDevice.Create(new I2cConnectionSettings(1, Ccs811Sensor.I2cFirstAddress)));
```

To create a device with a wake pin and interrupt pin:

```csharp
var ccs811 = new Ccs811Sensor(I2cDevice.Create(new I2cConnectionSettings(1, Ccs811Sensor.I2cFirstAddress)), pinWake: 3, pinInterruption: 2);
```

*Note*:

- If you are using the software I2C device instead of the hardware I2C, adjust the bus number. If like in the previous section, you've setup the software I2C, the bus number is 3. So instancing the device will be then:

```csharp
var ccs811 = new Ccs811Sensor(I2cDevice.Create(new I2cConnectionSettings(3, Ccs811Sensor.I2cFirstAddress)));
```

To create a device using an external chipset like FT4222 to offer GPIO and I2C support including Wake and Interrupt pins:

```csharp
var ftdiI2C = new Ft4222I2c(new I2cConnectionSettings(0, Ccs811Sensor.I2cFirstAddress));
var gpioController = new GpioController(PinNumberingScheme.Board, new Ft4222Gpio());
ccs811 = new Ccs811Sensor(ftdiI2C, gpioController, 3, 2, -1, false);
```

You can then display basic information of the device:

```csharp
Debug.WriteLine($"Hardware identification: 0x{ccs811.HardwareIdentification:X2}, must be 0x81");
Debug.WriteLine($"Hardware version: 0x{ccs811.HardwareVersion:X2}, must be 0x1X where any X is valid");
Debug.WriteLine($"Application version: {ccs811.ApplicationVersion}");
Debug.WriteLine($"Boot loader version: {ccs811.BootloaderVersion}");
```

### Select a measurement mode

This is needed to start the measurement in the constant power 1 measurement per second mode. Keep in mind the important notes regarding data accuracy.

```csharp
ccs811.OperationMode = OperationMode.ConstantPower1Second;
```

Once the measurement is set to anything else than idle, you can see the next section how to read a measure

### Getting measures

If you have selected an Interruption pin, an event mode is put in place. If not, you'll have to check if any measurement is available.

#### Case of not using the Interrupt pin

The basic example shows how to check if any data is ready and then ready the Gas sensor data.

```csharp
while (!ccs811.IsDataReady)
{
    Thread.Sleep(10);
}

var error = ccs811.TryReadGasData(out VolumeConcentration eCO2, out VolumeConcentration eTVOC, out ElectricCurrent curr, out int adc);
Debug.WriteLine($"Success: {error}, eCO2: {eCO2.PartsPerMillion} ppm, eTVOC: {eTVOC.PartsPerBillion} ppb, Current: {curr.Microamperes} µA, ADC: {adc} = {adc * 1.65 / 1023} V.");
```

#### Case of using the Interrupt pin

You can use the previous way or use the Event:

```csharp
// In the code after initialization
ccs811.MeasurementReady += Ccs811MeasurementReady;

// And a function to be called when a measurement is ready
private static void Ccs811MeasurementReady(object sender, MeasurementThresholdArgs args)
{
    Debug.WriteLine($"Measurement Event: Success: {args.MeasurementSuccess}, eCO2: {args.EquivalentCO2.PartsPerMillion} ppm, " +
        $"eTVOC: {args.EquivalentTotalVolatileOrganicCompound.PartsPerBillion} ppb, Current: {args.RawCurrentSelected.Microamperes} µA, " +
        $"ADC: {args.RawAdcReading} = {args.RawAdcReading * 1.65 / 1023} V.");
}
```

### Setting a threshold

This feature is only available if the interruption pin is used. Events needs to be activated as well. This is an example of setting up a threshold between 400 and 600 ppm for the eCO2. Note that the threshold needs to have at least 50 of difference between the minimum and maximum values.

```csharp
ccs811.MeasurementReady += Ccs811MeasurementReady;
ccs811.SetThreshold(VolumeConcentration.FromPartsPerMillion(400), VolumeConcentration.FromPartsPerMillion(600));
```

You will then receive an event with the first data point crossing up the threshold. No other data point will raise an event.

### Adjusting temperature and humidity

The calculation is sensitive to temperature and humidity. It is recommended to adjust the default values with an accurate temperature and relative humidity source sensor. Default values are 25°C for the temperature and 50% for the relative humidity. The following example shows how to adjust for 21.3°C and 42.5%:

```csharp
ccs811.SetEnvironmentData(Temperature.FromDegreesCelsius(21.3), Ratio.FromPercent(42.5));
```

### Reading and loading the baseline

The baseline is used to calculate the eCO2 and eTVOC based on the raw data. It is not intended to be human readable. Refer to the documentation to understand more about the concept.

```csharp
var baseline = ccs811.BaselineAlgorithmCalculation;
Debug.WriteLine($"Baseline calculation value: {baseline}, changing baseline");
// Please refer to documentation, baseline is not a human readable number
ccs811.BaselineAlgorithmCalculation = 50300;
Debug.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}, changing baseline for the previous one");
ccs811.BaselineAlgorithmCalculation = baseline;
Debug.WriteLine($"Baseline calculation value: {ccs811.BaselineAlgorithmCalculation}");
```

## CCS811 Samples

This [sample application](https://github.com/dotnet/iot/tree/main/src/devices/Ccs811/samples) contains flow and menus allowing you to test easily all the feature of the CCS811 and also show how to implement properly all readings.

You can test it thru:

- A native platform like an ESP32

You can use the native GPIO support for the following pins or not:

- The address pin is used to select primary (0x5A) or secondary (0x5B) I2C device address.
- The Reset pin is sometime present or not. If present and you want to use it, this will perform a full hard reset.
- The Wake pin is used to select the chip and wake it up. If you don't want to use it, just put it to the ground.
- The Interrupt pin allows interruption, if used, the interrupt mode and events will be activated. This needs to be activated to be able to use the embedded Threshold feature.

You can select any of the mode.

A variety of tests and reading, including changing the temperature and humidity correction is proposed.

You can log the date an nicely import them later on in Excel. The following example shows a measurement over time. In blue, the equivalent CO2 in ppm and in orange the equivalent TVOC in ppb. Note that the measurement started to be accurate around 11:35 on this graph.

![Graph](./graph.png)

### Sample wiring

**Important** to understand:

In order to have this sensor working on a MCU, you need to lower the bus speed. This sensor uses a mode called I2C stretching and it *may* not be supported natively on your MCU. So you **must** lower the I2C clock to the minimum to make it working properly or use a software I2C with a low clock as well

![Wiring sample](./ccs811_bb.png)
