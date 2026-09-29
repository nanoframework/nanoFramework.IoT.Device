# INA236 - Bidirectional Current/Power Monitor

The INA236 is a current-shunt and power monitor with an I2C-compatible interface. It measures shunt voltage, bus voltage, current, and power, with programmable conversion times and averaging.

## Documentation

- [INA236 datasheet](https://www.ti.com/lit/ds/symlink/ina236.pdf)
- [Upstream .NET IoT binding](https://github.com/dotnet/iot/tree/main/src/devices/Ina236)

## Usage

The sample uses I2C bus 1 and the default address `0x40`. On ESP32, configure the I2C pins before creating the device.

```csharp
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(Gpio.IO22, DeviceFunction.I2C1_CLOCK);

I2cConnectionSettings settings = new I2cConnectionSettings(1, Ina236.DefaultI2cAddress);
I2cDevice i2cDevice = new I2cDevice(settings);

using (Ina236 device = new Ina236(
    i2cDevice,
    ElectricResistance.FromMilliohms(8),
    ElectricCurrent.FromAmperes(10)))
{
    while (true)
    {
        Debug.WriteLine($"Bus: {device.ReadBusVoltage().Volts} V");
        Debug.WriteLine($"Shunt: {device.ReadShuntVoltage().Millivolts} mV");
        Debug.WriteLine($"Current: {device.ReadCurrent().Amperes} A");
        Debug.WriteLine($"Power: {device.ReadPower().Watts} W");
        Thread.Sleep(1000);
    }
}
```

Known breakout boards commonly use an 8 milliohm shunt and are designed for currents up to 10 A. Verify the shunt value and current rating for your board. At 10 A, an 8 milliohm shunt dissipates 0.8 W.

The INA236A supports addresses `0x40` through `0x43`; the INA236B supports `0x60` through `0x63`. The address depends on the device variant and ADDR pin connection.
