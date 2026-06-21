// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Axp2101;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.I2c;
using System.Diagnostics;
using System.Threading;
using UnitsNet;

Debug.WriteLine("Hello from AXP2101!");
Debug.WriteLine("Sample targeting M5Stack CoreS3 (ESP32-S3)");

//////////////////////////////////////////////////////////////////////
// 1. I2C initialization with ESP32-S3 pin configuration
//////////////////////////////////////////////////////////////////////

// M5Stack CoreS3 uses I2C1 on SDA=12, SCL=11 for the internal AXP2101 PMIC.
// Adjust pin numbers for your specific board.
Configuration.SetPinFunction(12, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(11, DeviceFunction.I2C1_CLOCK);

I2cDevice i2cAxp2101 = new(new I2cConnectionSettings(1, Axp2101.I2cDefaultAddress));
Axp2101 power = new Axp2101(i2cAxp2101);

//////////////////////////////////////////////////////////////////////
// 2. Chip ID verification
//////////////////////////////////////////////////////////////////////

byte chipId = power.GetChipId();
Debug.WriteLine($"AXP2101 Chip ID: 0x{chipId:X2} (expected 0x{Axp2101.ChipId:X2})");

if (chipId != Axp2101.ChipId)
{
    Debug.WriteLine("ERROR: Chip ID mismatch! Check wiring and I2C address.");
    Thread.Sleep(Timeout.Infinite);
}

//////////////////////////////////////////////////////////////////////
// 3. Power rail setup — CoreS3 typical configuration
//////////////////////////////////////////////////////////////////////

// DCDC1: ESP32-S3 core — 3.3V (do NOT disable this!)
power.DcDc1Voltage = ElectricPotential.FromVolts(3.3);
power.EnableDcDc1();

// ALDO1: System peripherals — 1.8V
power.Aldo1Voltage = ElectricPotential.FromVolts(1.8);
power.EnableAldo1();

// ALDO2: Display/LCD power — 3.3V
power.Aldo2Voltage = ElectricPotential.FromVolts(3.3);
power.EnableAldo2();

// ALDO3: Sensor/peripheral power — 3.3V
power.Aldo3Voltage = ElectricPotential.FromVolts(3.3);
power.EnableAldo3();

// ALDO4: Camera sensor — 3.3V
power.Aldo4Voltage = ElectricPotential.FromVolts(3.3);
power.EnableAldo4();

// BLDO1: I/O level shift — 3.3V
power.Bldo1Voltage = ElectricPotential.FromVolts(3.3);
power.EnableBldo1();

// BLDO2: Reserved/peripheral — 3.3V
power.Bldo2Voltage = ElectricPotential.FromVolts(3.3);
power.EnableBldo2();

Debug.WriteLine("");
Debug.WriteLine("Power rails configured:");
Debug.WriteLine($"  DCDC1: {power.DcDc1Voltage.Millivolts} mV (enabled: {power.IsDcDc1Enabled})");
Debug.WriteLine($"  ALDO1: {power.Aldo1Voltage.Millivolts} mV (enabled: {power.IsAldo1Enabled})");
Debug.WriteLine($"  ALDO2: {power.Aldo2Voltage.Millivolts} mV (enabled: {power.IsAldo2Enabled})");
Debug.WriteLine($"  ALDO3: {power.Aldo3Voltage.Millivolts} mV (enabled: {power.IsAldo3Enabled})");
Debug.WriteLine($"  ALDO4: {power.Aldo4Voltage.Millivolts} mV (enabled: {power.IsAldo4Enabled})");
Debug.WriteLine($"  BLDO1: {power.Bldo1Voltage.Millivolts} mV (enabled: {power.IsBldo1Enabled})");
Debug.WriteLine($"  BLDO2: {power.Bldo2Voltage.Millivolts} mV (enabled: {power.IsBldo2Enabled})");

//////////////////////////////////////////////////////////////////////
// 4. Charging configuration
//////////////////////////////////////////////////////////////////////

power.EnableCellBatteryCharge();
power.SetChargeConstantCurrent(ChargingCurrent.Current200mA);
power.SetChargeTargetVoltage(ChargeTargetVoltage.Voltage4V2);
power.SetPrechargeCurrent(PrechargeCurrent.Current25mA);
power.SetChargeTerminationCurrent(ChargeTerminationCurrent.Current25mA);
power.EnableChargeTerminationLimit();
power.SetThermalThreshold(ThermalThreshold.Temperature100C);

Debug.WriteLine("");
Debug.WriteLine("Charging configured:");
Debug.WriteLine($"  CC current : {power.GetChargeConstantCurrent()}");
Debug.WriteLine($"  Target volt: {power.GetChargeTargetVoltage()}");

//////////////////////////////////////////////////////////////////////
// 5. LED control
//////////////////////////////////////////////////////////////////////

power.SetChargeLedMode(ChargeLedMode.ControlledByCharger);
Debug.WriteLine($"  LED mode   : {power.GetChargeLedMode()}");

//////////////////////////////////////////////////////////////////////
// 6. ADC channel enable
//////////////////////////////////////////////////////////////////////

power.EnableBatteryVoltageMeasure();
power.EnableVbusVoltageMeasure();
power.EnableSystemVoltageMeasure();
power.EnableTemperatureMeasure();

//////////////////////////////////////////////////////////////////////
// 7. VBUS input limits
//////////////////////////////////////////////////////////////////////

power.SetVbusVoltageLimit(VbusVoltageLimit.Voltage4V36);
power.SetVbusCurrentLimit(VbusCurrentLimit.Current500mA);

//////////////////////////////////////////////////////////////////////
// 8. Button handling — enable IRQs for power key
//////////////////////////////////////////////////////////////////////

power.EnableIrq(Axp2101Irq.PowerKeyShortPress | Axp2101Irq.PowerKeyLongPress);
power.SetPowerKeyPressOnTime(0); // 128 ms
power.SetPowerKeyPressOffTime(0); // 4 s
power.EnableLongPressShutdown();
power.SetLongPressPowerOff();

// Set power-off voltage to 3.0V
power.SetSysPowerDownVoltage(ElectricPotential.FromVolts(3.0));

// Clear any pending IRQs before entering the main loop
power.ClearIrqStatus();

//////////////////////////////////////////////////////////////////////
// 9. Main monitoring loop
//////////////////////////////////////////////////////////////////////

Debug.WriteLine("");
Debug.WriteLine("Entering monitoring loop...");
Debug.WriteLine("");

while (true)
{
    // Battery monitoring
    int percentage = power.GetBatteryPercentage();
    ElectricPotential batteryVoltage = power.GetBatteryVoltage();
    ChargingStatus chargeStatus = power.GetChargerStatus();
    bool batteryConnected = power.IsBatteryConnected;

    Debug.WriteLine($"Battery: {percentage}%, {batteryVoltage.Volts:F2}V, {chargeStatus}, Connected: {batteryConnected}");

    // VBUS monitoring
    bool vbusGood = power.IsVbusGood;
    if (vbusGood)
    {
        ElectricPotential vbusVoltage = power.GetVbusVoltage();
        Debug.WriteLine($"VBUS: {vbusVoltage.Volts:F2}V, Connected");
    }
    else
    {
        Debug.WriteLine("VBUS: Not connected");
    }

    // System voltage
    ElectricPotential sysVoltage = power.GetSystemVoltage();
    Debug.WriteLine($"System: {sysVoltage.Volts:F2}V");

    // Die temperature
    Temperature dietemp = power.GetInternalTemperature();
    Debug.WriteLine($"Die Temperature: {dietemp.DegreesCelsius:F1} °C");

    // Check power key button IRQs
    if (power.IsPowerKeyShortPressIrq())
    {
        Debug.WriteLine(">>> Power key SHORT press detected");
    }

    if (power.IsPowerKeyLongPressIrq())
    {
        Debug.WriteLine(">>> Power key LONG press detected");
    }

    // Clear IRQ status after reading
    power.ClearIrqStatus();

    Debug.WriteLine("");
    Thread.Sleep(5000);
}

// Unreachable, but good practice
// Thread.Sleep(Timeout.Infinite);
