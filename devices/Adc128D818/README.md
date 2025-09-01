# ADC128D818 - Analog to Digital Converter

ADC128D818 is an Analog-to-Digital converter (ADC) 12-Bit, 8-Channel, ADC system monitor with temperature sensor and internal/external reference.

## Documentation

Product datasheet can be found at the manufacturer website [here](https://www.ti.com/product/ADC128D818).

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`. For this you have to install the `nanoFramework.Hardware.ESP32` NuGet package. Please note that the I2C pin in your device can be different from the ones in the code snippet.

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

For other devices like STM32, please make sure you're using the pre-set pins for the I2C bus you want to use.

```csharp
// set I2C bus ID: 1
// ADC128D818 A0 and A1 pins connected to GND
I2cConnectionSettings settings = new I2cConnectionSettings(1, (int)I2cAddress.Low_Low);
I2cDevice device = I2cDevice.Create(settings);

// pass in I2cDevice
using (Adc128D818 adc = new Adc128D818(device))
{
    // set device to mode 0
    adc.Mode = Mode.Mode0;

    // enable internal voltage reference
    adc.VoltageReference = VoltageReference.Internal2_56;

    // set conversion mode to continuous
    adc.ConversionRate = ConversionRate.Continuous;

    // start conversion
    adc.Start();

    // read raw data from channel IN0
    int channelIn0 = adc.ReadChannel(Channel.In0);

    // output reading
    Console.WriteLine($"Channel IN0 reading: {channelIn0}");

    // read local temperature
    int localTemp = adc.ReadChannel(Channel.Temperature);)

    // convert temperature reading 
    Temperature temperature = Adc128D818.ConvertLocalTemperatureReading(localTemp);

    // output temperature
    Console.WriteLine($"Local temperature: {temperature.DegreesCelsius} °C");

    // shutdown the device
    adc.DeepShutdown();
}
```

## Acknowledgments

The development of this library was kindly sponsored by [OrgPal.IoT](https://www.orgpal.com/)!

![orgpallogo.png](./orgpallogo.png)
