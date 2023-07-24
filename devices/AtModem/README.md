# Generic AT Modem and SIM7070, SIM7080, SIM7090  - Dual Mode Wireless Module CatM

This binding is a generic AT Modem handler that can be extended for different usage. The first implementation is for the 'Sim7080' which supports both 'CAT-M' and 'NB-IoT'. It can be controlled through AT command via a Serial/UART interface.

> Note: The module is tested on a Sim7080 but is also compatible with Sim7070 and Sim7090.

## Documentation

* [Product details](https://www.simcom.com/index.php/product/SIM7080G.html)
* [Datasheet](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/unit/sim7080g/en/Sim7080_Series_SPEC_20200427.pdf) for the Sim7080G.
* [Manual](https://m5stack.oss-cn-shenzhen.aliyuncs.com/resource/docs/datasheet/unit/sim7080g/en/SIM7070_Sim7080_SIM7090%20Series_AT%20Command%20Manual_V1.04.pdf) for the AT Commands.

## Usage

**Important**: make sure you properly setup the RT/RX pins especially for ESP32 before creating the `SerialPort`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
static SerialPort _serialPort;
static Sim7080G _sim;

private static void OpenSerialPort(
    string port = "COM3",
    int baudRate = 115200,
    Parity parity = Parity.None,
    StopBits stopBits = StopBits.One,
    Handshake handshake = Handshake.XOnXOff,
    int dataBits = 8,
    char watchChar = '\r')
{
    //In this example configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
    Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
    Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

    _serialPort = new(port)
    {
        //Set parameters
        BaudRate = baudRate,
        Parity = parity,
        StopBits = stopBits,
        Handshake = handshake,
        DataBits = dataBits,
    };

    try
    {
        //Open the serial port
        _serialPort.Open();
    }
    catch (Exception exception)
    {
        Debug.WriteLine(exception.Message);
    }

    //Set a watch char to be notified when it's available in the input stream
    _serialPort.WatchChar = watchChar;
}
```

## Internal storage

TODO

## SMS

TODO

## Networking

TODO

## Call

TODO

## MQTT

TODO

## Articles

Part of this code is adaptation and port of [ATLib](https://github.com/hbjorgo/ATLib).

* [Starting a Narrowband IoT project with nanoFramework](https://medium.com/itnext/when-machines-talk-bccba9a8c049)
* [Establishing a connection to Azure IoT Hub using an MQTT client with nanoFramework](https://medium.com/itnext/establishing-a-connection-to-azure-iot-hub-using-an-mqtt-client-with-nanoframework-d9c2e1b4ebbe)
* [Creating a tracking device with nanoFramework](https://medium.com/itnext/creating-a-tracking-device-with-nanoframework-6d27b5b4e7ab)