# AXP2101 - Enhanced Single Cell Li-Battery and Power System Management IC

## Summary

The AXP2101 is an advanced power management IC (PMIC) manufactured by X-Powers, widely used in embedded devices such as the **M5Stack CoreS3** (ESP32-S3-based). It provides comprehensive power management with multiple configurable voltage regulators, battery charging control, ADC measurements, fuel gauge, watchdog, and interrupt handling. It communicates over I2C at default address `0x34`.

## Documentation

- [AXP2101 Datasheet (PDF)](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/core/K128%20CoreS3/AXP2101_Datasheet_V1.0_en.pdf)
- [M5Stack CoreS3 Documentation](https://docs.m5stack.com/en/core/CoreS3)
- [CoreS3 Schematics (PDF)](https://m5stack-doc.oss-cn-shenzhen.aliyuncs.com/490/Sch_M5_CoreS3_v1.0.pdf)
- [XPowersLib (lewisxhe) - Reference C++ implementation](https://github.com/lewisxhe/XPowersLib)
- Related nanoFramework binding: [`Iot.Device.Axp192`](../Axp192/README.md)

## Device Capabilities

| Feature | Details |
| --- | --- |
| **DCDC Converters** | 5 channels (DCDC1-DCDC5) with configurable voltage outputs |
| **LDO Regulators** | 4x ALDO (1-4), 2x BLDO (1-2), 1x CPUSLDO, 2x DLDO (1-2) |
| **Battery Charging** | Linear charger with configurable current (0-1000 mA), voltage (4.0-4.44V), and termination settings |
| **Fuel Gauge** | Built-in battery percentage calculation, gauge data ROM |
| **ADC** | Battery voltage, VBUS voltage, system voltage, die temperature sensor, TS pin |
| **Button Battery** | Backup/coin cell battery charging support |
| **Interrupts** | 3 interrupt status registers supporting ~24 different IRQ sources |
| **Watchdog** | Configurable timer with multiple reset behaviors |
| **Power Control** | Power-on/off source detection, shutdown control, sleep/wakeup |
| **Data Buffer** | 4-byte user data storage |

### Voltage Ranges

| Channel | Voltage Range | Step |
| --- | --- | --- |
| DCDC1 | 1500-3400 mV | 100 mV |
| DCDC2 | 500-1200 mV / 1220-1540 mV | 10 mV / 20 mV |
| DCDC3 | 500-1200 mV / 1220-1540 mV / 1600-3400 mV | 10 mV / 20 mV / 100 mV |
| DCDC4 | 500-1200 mV / 1220-1840 mV | 10 mV / 20 mV |
| DCDC5 | 1200 mV (special), 1400-3700 mV | 100 mV |
| ALDO1-4 | 500-3500 mV | 100 mV |
| BLDO1-2 | 500-3500 mV | 100 mV |
| CPUSLDO | 500-1400 mV | 50 mV |
| DLDO1-2 | 500-3400 mV | 100 mV |

## Usage

```csharp
// Configure I2C pins for ESP32-S3 (M5Stack CoreS3)
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cDevice i2cAxp2101 = new(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
Axp2101 power = new Axp2101(i2cAxp2101);
```

> **Important**: make sure you read the documentation of your battery and set up the proper charging values and stop current. Overcharging your battery may damage it.

### Setting up power rails

Configure DCDC and LDO voltage rails for your board. Example for M5Stack CoreS3:

```csharp
// DCDC1: ESP32-S3 core — 3.3V (do NOT disable this!)
power.DcDc1Voltage = ElectricPotential.FromVolts(3.3);
power.EnableDcDc1();

// ALDO1: System peripherals — 1.8V
power.Aldo1Voltage = ElectricPotential.FromVolts(1.8);
power.EnableAldo1();

// ALDO2: Display/LCD — 3.3V
power.Aldo2Voltage = ElectricPotential.FromVolts(3.3);
power.EnableAldo2();
```

> **Warning**: Never disable DCDC1 on a CoreS3 as it powers the ESP32-S3 core. Disabling it will brick the system until the next power cycle.

### Battery charging

```csharp
power.EnableCellBatteryCharge();
power.SetChargeConstantCurrent(ChargingCurrent.Current200mA);
power.SetChargeTargetVoltage(ChargeTargetVoltage.Voltage4V2);
power.SetPrechargeCurrent(PrechargeCurrent.Current25mA);
power.SetChargeTerminationCurrent(ChargeTerminationCurrent.Current25mA);
power.EnableChargeTerminationLimit();
power.SetThermalThreshold(ThermalThreshold.Temperature100C);
```

### Battery status

```csharp
int percentage = power.GetBatteryPercentage();
ElectricPotential batteryVoltage = power.GetBatteryVoltage();
ChargingStatus chargeStatus = power.GetChargerStatus();
bool batteryConnected = power.IsBatteryConnected;

Debug.WriteLine($"Battery: {percentage}%, {batteryVoltage.Volts:F2}V, {chargeStatus}");
Debug.WriteLine($"  Connected: {batteryConnected}");
```

### VBUS monitoring

```csharp
if (power.IsVbusGood)
{
    ElectricPotential vbusVoltage = power.GetVbusVoltage();
    Debug.WriteLine($"VBUS: {vbusVoltage.Volts:F2}V, Connected");
}
```

### Using the power button

The power key supports short and long press detection via interrupts:

```csharp
power.EnableIrq(Axp2101Irq.PowerKeyShortPress | Axp2101Irq.PowerKeyLongPress);
power.SetPowerKeyPressOnTime(0); // 128 ms
power.SetPowerKeyPressOffTime(0); // 4 s
power.EnableLongPressShutdown();
power.ClearIrqStatus();

// In your main loop:
if (power.IsPowerKeyShortPressIrq())
{
    Debug.WriteLine("Short press");
}

if (power.IsPowerKeyLongPressIrq())
{
    Debug.WriteLine("Long press");
}

power.ClearIrqStatus();
```

### Temperature and ADC

Enable ADC channels and read measurements:

```csharp
power.EnableBatteryVoltageMeasure();
power.EnableVbusVoltageMeasure();
power.EnableSystemVoltageMeasure();
power.EnableTemperatureMeasure();

Debug.WriteLine($"Die Temperature: {power.GetInternalTemperature().DegreesCelsius:F1} °C");
Debug.WriteLine($"System Voltage : {power.GetSystemVoltage().Volts:F2} V");
```

### Watchdog

```csharp
power.SetWatchdogConfig(WatchdogConfig.IrqAndReset);
power.SetWatchdogTimeout(WatchdogTimeout.Timeout8s);
power.EnableWatchdog();

// Feed the watchdog periodically:
power.ClearWatchdog();
```

### Advanced features

The AXP2101 supports many additional features including VBUS input limits, backup battery charging, fast power-on sequencing, fuel gauge ROM programming, sleep/wakeup, and DCDC forced-PWM mode. You can check out the [sample](./samples) to get more details on how to use these features.

> Note: this binding uses UnitsNet for units like Voltage and Temperature.

## M5Stack CoreS3 Power Rail Mapping

| Channel | Usage | Voltage |
| --- | --- | --- |
| DCDC1 | ESP32-S3 core | 3.3V |
| ALDO1 | System peripherals | 1.8V |
| ALDO2 | Display/LCD power | 3.3V |
| ALDO3 | Sensor/peripheral power | 3.3V |
| ALDO4 | Camera sensor | 3.3V |
| BLDO1 | I/O level shift | 3.3V |
| BLDO2 | Reserved/peripheral | 3.3V |

## Differences from AXP192

If migrating from the [`Axp192`](../Axp192/README.md) binding, note these key differences:

| Feature | AXP192 | AXP2101 |
| --- | --- | --- |
| **DCDC channels** | DCDC1, DCDC3 | DCDC1-5 |
| **LDO channels** | LDO2, LDO3 | ALDO1-4, BLDO1-2, CPUSLDO, DLDO1-2 |
| **Voltage ranges** | Simpler step logic | Multi-range with variable steps (DCDC2-4) |
| **ADC** | Full coulomb counter, multiple current ADC | Simplified ADC (no coulomb counter) |
| **Fuel gauge** | Coulomb counter based | Built-in fuel gauge with battery % register |
| **GPIO** | GPIO0-4 with configurable behavior | No user-accessible GPIO (TS pin is configurable) |
| **Interrupt registers** | 5 registers | 3 registers (~24 IRQ sources) |
| **Watchdog** | Not present | Configurable watchdog timer |
| **JEITA** | Not present | Temperature-compensated charging |
| **Data buffer** | 6 bytes | 4 bytes |
| **Chip ID register** | 0x03 (no standard value) | 0x03 → 0x4A |
| **Default I2C address** | 0x34 | 0x34 (same) |
