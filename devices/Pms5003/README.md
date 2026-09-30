# Plantower PMS5003 particulate-matter sensor

The PMS5003 measures PM1.0, PM2.5, and PM10 mass concentration and particle counts in six size bins. This binding reads the sensor's 32-byte active-reporting frames over UART and validates the frame header, length, and checksum.

## Documentation

- [PMS5003 data manual](https://www.digikey.com/htmldatasheets/production/2903006/0/0/1/pms5003-series-manual.html)

## ESP32-C3 Super Mini wiring

| PMS5003 | ESP32-C3 Super Mini | Notes |
|---|---|---|
| Pin 1 VCC | 5V | Sensor power |
| Pin 2 GND | GND | Common ground |
| Pin 4 RXD | GPIO21 (TX) | Optional for active-reporting mode |
| Pin 5 TXD | GPIO20 (RX) | Sensor data to ESP32 COM1 RX |
| Pin 3 SET | GPIO3 | Driven high for normal operation |
| Pin 6 RESET | GPIO2 | Driven high to keep the sensor out of reset |

The PMS5003 UART uses 9600 baud, 8 data bits, no parity, and one stop bit. Configure the ESP32 UART pin functions before opening `SerialPort`.

## Usage

```csharp
const int Reset = 2;
const int Set = 3;

GpioController gpioController = new GpioController();
GpioPin resetPin = gpioController.OpenPin(Reset, PinMode.Output);
GpioPin setPin = gpioController.OpenPin(Set, PinMode.Output);
resetPin.Write(PinValue.High);
setPin.Write(PinValue.High);
Configuration.SetPinFunction(Gpio.IO20, DeviceFunction.COM1_RX);
Configuration.SetPinFunction(Gpio.IO21, DeviceFunction.COM1_TX);

SerialPort serialPort = new SerialPort("COM1", Pms5003.DefaultBaudRate, Parity.None, 8, StopBits.One);
serialPort.ReadTimeout = 5000;
serialPort.Open();
Pms5003 sensor = new Pms5003(serialPort);

PmsReading reading = sensor.Read();
Debug.WriteLine($"PM2.5: {reading.Pm2Point5Atmospheric} ug/m3");
```

`Read()` synchronizes to the next frame header. A serial timeout is surfaced by `SerialPort`; malformed frames throw `InvalidOperationException`. Dispose the sensor to dispose its serial port, or pass `shouldDispose: false` to retain ownership.