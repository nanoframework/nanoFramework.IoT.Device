// See https://aka.ms/new-console-template for more information
using IoT.Device.AtModem;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Modem;
using IoT.Device.Sim7080;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;

Console.WriteLine("Hello SIM7080!");

SerialPort _serialPort;
OpenSerialPort("COM10");
AtChannel atChannel = AtChannel.Create(_serialPort.BaseStream);
Sim7080 sim7080 = new(atChannel);
atChannel.Open();

var pinStatus = sim7080.GetSimStatusAsync();
if (pinStatus.IsSuccess)
{
    Console.WriteLine($"SIM status: {(SimStatus)pinStatus.Result}");
    if((SimStatus)pinStatus.Result == SimStatus.SIM_PIN)
    {
        var pinRes = sim7080.EnterSimPinAsync(new PersonalIdentificationNumber("1234"));
        if (pinRes.IsSuccess)
        {
            Console.WriteLine("PIN entered successfully");
        }
        else
        {
            Console.WriteLine("PIN entered failed");
        }
    }
}
else
{
    Console.WriteLine($"SIM status failed: {pinStatus.ErrorMessage}");
}

var signalRes = sim7080.GetSignalStrengthAsync();
if (signalRes.IsSuccess)
{
    Console.WriteLine($"Signal strength: {signalRes.Result}");
}
else
{
    Console.WriteLine($"Signal strength failed: {signalRes.ErrorMessage}");
}

sim7080.Dispose();
CloseSerialPort();

/// <summary>
/// Configure and open the serial port for communication.
/// </summary>
/// <param name="port"></param>
/// <param name="baudRate"></param>
/// <param name="parity"></param>
/// <param name="stopBits"></param>
/// <param name="handshake"></param>
/// <param name="dataBits"></param>
/// <param name="readBufferSize"></param>
/// <param name="readTimeout"></param>
/// <param name="writeTimeout"></param>
/// <param name="watchChar"></param>
void OpenSerialPort(
    string port = "COM3",
    int baudRate = 115200,
    Parity parity = Parity.None,
    StopBits stopBits = StopBits.One,
    Handshake handshake = Handshake.None,
    int dataBits = 8,
    int readTimeout = 10000,
    int writeTimeout = Timeout.Infinite)
{
    // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
    //Configuration.SetPinFunction(16, DeviceFunction.COM3_RX);
    //Configuration.SetPinFunction(17, DeviceFunction.COM3_TX);

    _serialPort = new(port)
    {
        //Set parameters
        BaudRate = baudRate,
        Parity = parity,
        StopBits = stopBits,
        Handshake = handshake,
        DataBits = dataBits,
        ReadTimeout = readTimeout,
        WriteTimeout = writeTimeout
    };

    try
    {
        // Open the serial port
        _serialPort.Open();
    }
    catch (Exception exception)
    {
        Debug.WriteLine(exception.Message);
    }
}

/// <summary>
/// Close the serial port
/// </summary>
void CloseSerialPort()
{
    if (_serialPort.IsOpen)
    {
        _serialPort.Close();
        _serialPort.Dispose();
    }
}