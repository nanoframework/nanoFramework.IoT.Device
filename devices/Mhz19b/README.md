# MH-Z19B CO2-Sensor

Binding for the MH-Z19B NDIR infrared gas module. The gas module measures the CO2 gas concentration in the ambient air.

## Documentation

[MH-Z19b Datasheet](https://www.winsen-sensor.com/d/files/infrared-gas-sensor/mh-z19b-co2-ver1_0.pdf)

## Usage

The binding can be instantiated using an existing serial UART stream or with the name (e.g. ```COM2``` ) of the serial interface to be used.
If using an existing stream ```shouldDispose``` indicates whether the stream shall be disposed when the binding gets disposed.
If providing the name of the serial interface the connection gets closed and disposed when the binding is disposed.

**Important**: make sure you properly setup the UART pins especially for ESP32 before creating the `Serialport`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`

```csharp
public Mhz19b(Stream stream, bool shouldDispose)
public Mhz19b(string uartDevice)
```

The CO2 concentration reading can be retrieved with

```csharp
public VolumeConcentration GetCo2Reading()
```

The sample application demonstrates the use of the binding API for sensor calibration.

**Note:** Refer to the datasheet for more details on sensor calibration **before** using the calibration API of the binding. You may decalibrate the sensor otherwise!

## Binding Notes

The MH-Z19B gas module provides a serial communication interface (UART) which can be directly wired to a ESP32 board. The module is supplied with 5V. The UART level is at 3.3V and no level shifter is required.

|Function| ESP32Pin| MH-Z19 pin|
|--------|-----------|------------|
|Vcc +5V |(+5V)      |6 (Vin)     |
|GND	 |(GND)      |7 (GND)     |
|UART    |32 (TXD0)  |2 (RXD)     |
|UART    |33 (RXD0)  |3 (TXD)     |
Table: MH-Z19B to ESP32 connection

The binding supports the connection through an UART interface (e.g. ``COM2```) or (serial port) stream.
When using the UART interface the binding instantiates the port with the required UART settings and opens it.
The use of an existing stream adds flexibility to the actual interface that used with the binding.
In either case the binding supports all commands of the module.

**Make sure that you read the datasheet carefully before altering the default calibration behaviour.
Automatic baseline correction is enabled by default.**
