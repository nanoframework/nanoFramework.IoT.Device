# Plantower PMS5003 particulate-matter sensor

The PMS5003 measures PM1.0, PM2.5, and PM10 mass concentration and particle counts in six size bins. This binding reads the sensor's 32-byte active-reporting frames over UART and validates the frame header, length, and checksum.

## Documentation

- [PMS5003 data manual](https://www.digikey.com/htmldatasheets/production/2903006/0/0/1/pms5003-series-manual.html)

## ESP32 wiring

| PMS5003 | ESP32 | Notes |
|---|---|---|
| Pin 1 VCC | 5V | Sensor power |
| Pin 2 GND | GND | Common ground |
| Pin 4 RXD | GPIO17 | Optional for active-reporting mode |
| Pin 5 TXD | GPIO16 | Sensor data to ESP32 COM2 RX |
| Pin 3 SET | 3.3V | High for normal operation |
| Pin 6 RESET | 3.3V | High for normal operation |

The PMS5003 UART uses 9600 baud, 8 data bits, no parity, and one stop bit. Configure the ESP32 UART pin functions before opening `SerialPort`.

## Usage

```csharp
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);

SerialPort serialPort = new SerialPort("COM2", Pms5003.DefaultBaudRate, Parity.None, 8, StopBits.One);
serialPort.ReadTimeout = 5000;
serialPort.Open();
Pms5003 sensor = new Pms5003(serialPort);

PmsReading reading = sensor.Read();
Debug.WriteLine($"PM2.5: {reading.Pm2Point5Atmospheric} ug/m3");
```

`Read()` synchronizes to the next frame header. A serial timeout is surfaced by `SerialPort`; malformed frames throw `InvalidOperationException`. Dispose the sensor to dispose its serial port, or pass `shouldDispose: false` to retain ownership.