# AXP192 - Enhanced single Cell Li-Battery and Power System Management IC

## Documentation

-Product documentation can be found [here](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/AXP192_datasheet_en.pdf).
-Registers can be found [here](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/AXP192_datasheet_cn.pdf) (Chineese only, note: bing translator make miracle).
- This sensor is used in the [M5Stick](https://docs.m5stack.com/en/core/m5stickc). Initialization code for this device can be found [here](https://github.com/m5stack/M5StickC/blob/master/src/AXP192.cpp).

## Usage

```csharp
// Make sure you configure properly the I2C pins, here example for ESP32
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);

I2cDevice i2cAxp192 = new(new I2cConnectionSettings(1, Axp192.I2cDefaultAddress));
Axp192 power = new Axp192(i2cAxp192);
```

> **Important**: make sure you read th documentation of your battery and setup the proper charging values, stop current. Overcharging your battery may damage it.

### Using the button

One button is available and can be setup to track short and long press:

```csharp
// This part of the code will handle the button behavior
power.EnableButtonPressed(ButtonPressed.LongPressed | ButtonPressed.ShortPressed);
power.SetButtonBehavior(LongPressTiming.S2, ShortPressTiming.Ms128, true, SignalDelayAfterPowerUp.Ms32, ShutdownTiming.S10);
```

The status is kept in the registers up to the next status read. You can then have both a short and a long press, you can get the status like this:

```csharp
var status = power.GetButtonStatus();
if ((status & ButtonPressed.ShortPressed) == ButtonPressed.ShortPressed)
{
    Debug.WriteLine("Short press");
}
else if ((status & ButtonPressed.LongPressed) == ButtonPressed.LongPressed)
{
    Debug.WriteLine("Long press");
}
```

### Battery status

You can get various elements regarding the battery status:

```csharp
Debug.WriteLine($"Battery:");
Debug.WriteLine($"  Charge curr  : {power.GetBatteryChargeCurrent().Milliamperes} mA");
Debug.WriteLine($"  Status       : {power.GetBatteryChargingStatus()}");
Debug.WriteLine($"  Dicharge curr: {power.GetBatteryDischargeCurrent().Milliamperes} mA");
Debug.WriteLine($"  Inst Power   : {power.GetBatteryInstantaneousPower().Milliwatts} mW");
Debug.WriteLine($"  Voltage      : {power.GetBatteryVoltage().Volts} V");
Debug.WriteLine($"  Is battery   : {power.IsBatteryConnected()} ");
```

### Advanced features

The AXP192 can charge the battery, get and set charging current, cut off voltage, has protection for temperature. Most feature can be access or setup. You can check out the [sample](./samples) to get more details on how to set those advance features.

> Note: this binding uses UnitsNet for the units like Voltage, Amperes.

Here is an example reading the current, voltage:

```csharp
Debug.WriteLine($"Temperature : {power.GetInternalTemperature().DegreesCelsius} °C");
Debug.WriteLine($"Input:");
// Note: the current and voltage will show 0 when plugged into USB.
// To see something else than 0, you should output those data on a serial port for example
// Or display on the screen.
Debug.WriteLine($"  Current   : {power.GetInputCurrent().Milliamperes} mA");
Debug.WriteLine($"  Voltage   : {power.GetInputVoltage().Volts} V");
Debug.WriteLine($"  Status    : {power.GetInputPowerStatus()}");
Debug.WriteLine($"  USB volt  : {power.GetUsbVoltageInput().Volts} V");
Debug.WriteLine($"  USB Curr  : {power.GetUsbCurrentInput().Milliamperes} mA");
```

### Coulomb counter

The AXP192 has a Coulomb counter where the value is in mili Amperes per hour (this one is not yet using UnitsNet). You first have to enable the Counter and then you can read the value. It is recommended to let some time between the moment you enable and read the data. Features to reset, stop the count are available as well

```csharp
power.EnableCoulombCounter();
// Do something here
// You can then read periodically the Coulomb counter:
Debug.WriteLine($"Coulomb: {power.GetCoulomb()} mA/h");
```
