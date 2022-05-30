# Sensirion SPS30 Particulate Matter Sensor

This is a library to interact with the Sensirion SPS30 Particulate Matter Sensor. Currently, only the UART interface using the SHDLC protocol is implemented. The SPS30 also supports I2C.

![sps30-image.png](https://raw.githubusercontent.com/nanoframework/nanoFramework.IoT.Device/develop/devices/SPS30/sps30-image.png)

## Documentation

* The datasheet for this sensor can be found [here](https://sensirion.com/media/documents/8600FF88/616542B5/Sensirion_PM_Sensors_Datasheet_SPS30.pdf)

## Usage for the UART interface

**Important**: make sure you properly setup the UART pins for ESP32 before creating the `SerialPort`. For this, make sure you install the `nanoFramework.Hardware.Esp32` NuGet and use the `Configuration` class to configure the pins:

```csharp
Configuration.SetPinFunction(4, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(15, DeviceFunction.COM2_RX);
```

Initialize the `SerialPort`, wrap it in the `SHDLCProtocol`, then pass to the `SPS30Sensor`:
```csharp
var serial = new SerialPort("COM2", 115200, Parity.None, 8, StopBits.One);
var shdlc = new SHDLCProtocol(serial, timeoutInMillis: 10000);
var sps30 = new SPS30Sensor(shdlc);
```

Check out the sample for more information. Example output:
```
SPS30 detected: ID=00080000, serial=4E1AD1BB796C64C5, version=Firmware V2.1, Hardware V7, SHDLC V2.0, status=RawRegister: 0, FanSpeedOutOfRange: False, LaserFailure: False, FanFailureBlockedOrBroken: False, cleaninginterval=604800
Measurement: MassConcentration PM1.0=0.71818184, PM2.5=0.75944983, PM4.0=0.75944983, PM10.0=0.75944983, NumberConcentration PM0.5=4.89496469, PM1.0=5.69449424, PM2.5=5.73169803, PM4.0=5.73438549, PM10.0=0, TypicalParticleSize=0.38276255
```
