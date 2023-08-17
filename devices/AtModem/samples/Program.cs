// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Events;
using IoT.Device.AtModem.Modem;
#if (NANOFRAMEWORK_1_0)
using nanoFramework.Hardware.Esp32;
#endif
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Net.Http;
using System.Threading;

Console.WriteLine("Hello SIM7080!");

SerialPort _serialPort;
#if (NANOFRAMEWORK_1_0)
OpenSerialPort("COM3");
#else
OpenSerialPort("COM10");
#endif

_serialPort.NewLine = "\r\n";
AtChannel atChannel = AtChannel.Create(_serialPort);
atChannel.DebugEnabled = true;
int retries = 10;
Sim7080 modem = new(atChannel);
////Sim800 modem = new(atChannel);
modem.NetworkConnectionChanged += ModemNetworkConnectionChanged;
modem.Network.AutoReconnect = true;
modem.Network.ApplicationNetworkEvent += NetworkApplicationNetworkEvent;

var respDeviceInfo = modem.GetDeviceInformation();
if (respDeviceInfo.IsSuccess)
{
    Console.WriteLine($"Device info: {respDeviceInfo.Result}");
}
else
{
    Console.WriteLine($"Device info failed: {respDeviceInfo.ErrorMessage}");
}

// To test the different storage, we don't need connection
TestBinaryStorage();
TestStorage();

RetryConnect:
var pinStatus = modem.GetSimStatus();
if (pinStatus.IsSuccess)
{
    Console.WriteLine($"SIM status: {(SimStatus)pinStatus.Result}");
    if ((SimStatus)pinStatus.Result == SimStatus.SIM_PIN)
    {
        var pinRes = modem.EnterSimPin(new PersonalIdentificationNumber("1234"));
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
    // Retry
    if (retries-- > 0)
    {
        Console.WriteLine("Retrying to get SIM status");
        Thread.Sleep(1000);
        goto RetryConnect;
    }
    else
    {
        Console.WriteLine("Giving up");
        return;
    }
}

// Wait for network registration for 2 minutes max, if not connected, then something is most likely very wrong
var isConnected = modem.WaitForNetworkRegistration(new CancellationTokenSource(120_000).Token);

if (!isConnected)
{
    Console.WriteLine("Something is very wrong and we did not manage to connect to the network.");
    // Here you can try to setup manually the stored APN, username and password or anything like this
    // Lat's try to disable and reanable the engine
    if (retries-- > 0)
    {
        Console.WriteLine("Retrying to connect");
        modem.Enabled = false;
        Thread.Sleep(200);
        modem.Enabled = true;
        Thread.Sleep(1000);
        goto RetryConnect;
    }

    Console.WriteLine("Giving up");
    return;
}

SignalStrength strenght;
ModemResponse networkReg;
while (true)
{
    networkReg = modem.GetSignalStrength();
    if (networkReg.IsSuccess)
    {
        Console.WriteLine($"Signal strength: {networkReg.Result}");
        strenght = (SignalStrength)networkReg.Result;
        if (strenght.Rssi.Percent < 99)
        {
            Console.WriteLine("Network quality is good!");
            break;
        }
    }
    else
    {
        Console.WriteLine($"Signal strength failed: {networkReg.ErrorMessage}");
    }

    Thread.Sleep(1000);
}

//ConnectToNetwork();
//GetNetworkOperators();
//TestStorageSmsAndCharSet();
//TestSms();
//TestHttp();

void TestHttp()
{
    var httpClient = modem.HttpClient;
    HttpResponseMessage resp;
    resp = httpClient.Get("http://www.ellerbach.net/DateHeure/");
    Console.WriteLine($"Status should be OK 200: {resp.StatusCode}");
    Console.WriteLine($"HTTP GET: {resp.Content?.ReadAsString()}");
    Console.WriteLine();

    resp = httpClient.Get("http://www.ellerbach.net/DateHeure");
    Console.WriteLine($"Status should be MovedPermanently 301: {resp.StatusCode}");
    Console.WriteLine($"HTTP GET: {resp.Content?.ReadAsString()}");
    Console.WriteLine();

    resp = httpClient.Get("http://www.ellerbach.net/DateTime");
    Console.WriteLine($"Status should be NotFound 404: {resp.StatusCode}");
    Console.WriteLine($"HTTP GET: {resp.Content?.ReadAsString()}");
    Console.WriteLine();

    resp = httpClient.Get("https://www.ellerbach.net/DateHeure/");
    Console.WriteLine($"Status should be OK 200: {resp.StatusCode}");
    Console.WriteLine($"HTTP GET: {resp.Content?.ReadAsString()}");
    Console.WriteLine();

    resp = httpClient.Post("https://httpbin.org/post", new StringContent("{\"title\":\"nano\",\"body\":\"Framework\",\"userId\":101}", System.Text.Encoding.UTF8, "application/json"));
    Console.WriteLine($"Status should be OK 200: {resp.StatusCode}");
    Console.WriteLine($"HTTP POST: {resp.Content?.ReadAsString()}");
    Console.WriteLine();
}

modem.Dispose();
CloseSerialPort();

void OpenSerialPort(
    string port = "COM3",
    int baudRate = 115200,
    Parity parity = Parity.None,
    StopBits stopBits = StopBits.One,
    Handshake handshake = Handshake.None,
    int dataBits = 8,
    int readTimeout = Timeout.Infinite,
    int writeTimeout = Timeout.Infinite)
{
#if (NANOFRAMEWORK_1_0)
    // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
    Configuration.SetPinFunction(32, DeviceFunction.COM3_RX);
    Configuration.SetPinFunction(33, DeviceFunction.COM3_TX);
#endif

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

    // Open the serial port
    _serialPort.Open();
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

void ConnectToNetwork()
{
    var network = modem.Network;
    ////var connectRes = network.Connect(new PersonalIdentificationNumber("1234"), new AccessPointConfiguration("free"));
    var connectRes = network.Connect(apn: new AccessPointConfiguration("orange"));
    if (connectRes)
    {
        Console.WriteLine($"Connected to network.");
    }
    else
    {
        Console.WriteLine($"Connected to network failed! Trying to reconnect...");
        connectRes = network.Reconnect();
        if (connectRes)
        {
            Console.WriteLine($"Reconnected to network.");
        }
        else
        {
            Console.WriteLine($"Reconnected to network failed!");
        }
    }

    NetworkInformation networkInformation = network.NetworkInformation;
    Console.WriteLine($"Network information:");
    Console.WriteLine($"  Operator: {networkInformation.NetworkOperator}");
    Console.WriteLine($"  Connextion status: {networkInformation.ConnectionStatus}");
    Console.WriteLine($"  IP Address: {networkInformation.IPAddress}");
    Console.WriteLine($"  Signal quality RSSI: {networkInformation.SignalQuality.Rssi}");
    Console.WriteLine($"  Signal quality BER: {networkInformation.SignalQuality.Ber}");
}

void TestStorageSmsAndCharSet()
{
    var resp = modem.GetAvailableCharacterSets();
    if (resp.IsSuccess)
    {
        Console.Write($"Available character sets: ");
        foreach (string item in (string[])resp.Result)
        {
            Console.Write($"{item} ");
        }
        Console.WriteLine();
    }
    else
    {
        Console.WriteLine($"Available character sets failed: {resp.ErrorMessage}");
    }

    var respChar = modem.GetCurrentCharacterSet();
    if (respChar.IsSuccess)
    {
        Console.WriteLine($"Current character set: {respChar.Result}");
    }
    else
    {
        Console.WriteLine($"Current character set failed: {respChar.ErrorMessage}");
    }

    var respCpms = modem.SmsProvider.GetSupportedPreferredMessageStorages();
    if (respCpms.IsSuccess)
    {
        Console.WriteLine($"Supported preferred message storages: {respCpms.Result}");
        var respPrefStorageSet = modem.SmsProvider.SetPreferredMessageStorage(((SupportedPreferredMessageStorages)respCpms.Result).Storage1[0], ((SupportedPreferredMessageStorages)respCpms.Result).Storage2[0], ((SupportedPreferredMessageStorages)respCpms.Result).Storage3[0]);
        if (respPrefStorageSet.IsSuccess)
        {
            Console.WriteLine($"Preferred message storages set successfully: {respPrefStorageSet.Result}");
        }
        else
        {
            Console.WriteLine($"Preferred message storages set failed: {respPrefStorageSet.ErrorMessage}");
        }
    }
    else
    {
        Console.WriteLine($"Supported preferred message storages failed: {respCpms.ErrorMessage}");
    }

    var respPRefStor = modem.SmsProvider.GetPreferredMessageStorages();
    if (respPRefStor.IsSuccess)
    {
        Console.WriteLine($"Preferred message storages: {respPRefStor.Result}");
    }
    else
    {
        Console.WriteLine($"Preferred message storages failed: {respPRefStor.ErrorMessage}");
    }
}

void TestSms()
{
    while (true)
    {
        if (modem.SmsProvider.IsSmsReady)
        {
            Console.WriteLine($"SMS is ready!");
            break;
        }

        Console.WriteLine($"Waiting for SMS to be ready...");
        Thread.Sleep(1000);
    }

    var resplistSms = modem.SmsProvider.ListSmss(SmsStatus.ALL);
    if (resplistSms.IsSuccess)
    {
        Console.WriteLine($"SMS list:");
        foreach (SmsWithIndex sms in (ArrayList)resplistSms.Result)
        {
            Console.WriteLine($"  Sender: {sms.Sender}");
            Console.WriteLine($"  Date: {sms.ReceiveTime}");
            Console.WriteLine($"  Message: {sms.Message}");
            Console.WriteLine($"  Status: {sms.Status}");
            Console.WriteLine($"  Index: {sms.Index}");
        }

        // Checking if we have at least 1 SMS
        if (((ArrayList)resplistSms.Result).Count > 0)
        {
            // Making sur"e we are using the PDU format
            modem.SmsProvider.SetSmsMessageFormat(SmsTextFormat.PDU);
            var respSmsRead = modem.SmsProvider.ReadSms(((SmsWithIndex)((ArrayList)resplistSms.Result)[0]).Index, SmsTextFormat.PDU);
            if (respSmsRead.IsSuccess)
            {
                Sms sms = (Sms)respSmsRead.Result;
                Console.WriteLine($"SMS read successfully:");
                Console.WriteLine($"  Sender: {sms.Sender}");
                Console.WriteLine($"  Date: {sms.ReceiveTime}");
                Console.WriteLine($"  Message: {sms.Message}");
                Console.WriteLine($"  Status: {sms.Status}");
            }
            else
            {
                Console.WriteLine($"SMS read failed: {respSmsRead.ErrorMessage}");
            }

        }
    }
    else
    {
        Console.WriteLine($"SMS list failed: {resplistSms.ErrorMessage}");
    }

    ModemResponse respSmsSend;
    respSmsSend = modem.SmsProvider.SendSmsInTextFormat(new PhoneNumber("+33664404676"), "Hello from nanoFramework text");
    if (respSmsSend.IsSuccess)
    {
        Console.WriteLine($"SMS sent successfully: {respSmsSend.Result}");
    }
    else
    {
        Console.WriteLine($"SMS sent failed: {respSmsSend.ErrorMessage}");
    }

    respSmsSend = modem.SmsProvider.SendSmsInPduFormat(new PhoneNumber("+33664404676"), "Hello from nanoFramework pdu", IoT.Device.AtModem.CodingSchemes.CodingScheme.Gsm7, true);
    if (respSmsSend.IsSuccess)
    {
        Console.WriteLine($"SMS sent successfully: {respSmsSend.Result}");
    }
    else
    {
        Console.WriteLine($"SMS sent failed: {respSmsSend.ErrorMessage}");
    }
}

void GetNetworkOperators()
{
    var network = modem.Network;

    Console.WriteLine("Getting the list of operators, this may take a while, up to 5 minutes...");
    // Get the operators
    var operators = network.GetOperators();
    if (operators != null)
    {
        foreach (var op in operators)
        {
            Console.WriteLine($"Operator:");
            Console.WriteLine($"  Name: {op.Name}");
            Console.WriteLine($"  Long name: {op.ShortName}");
            Console.WriteLine($"  Format: {op.Format}");
            Console.WriteLine($"  System Mode: {op.SystemMode}");
        }
    }
}

void TestStorage()
{
    const string FileName = "test.txt";
    const string Content = "Hello from nanoFramework";

    // Available storage
    var size = modem.FileStorage.GetAvailableStorage();
    Console.WriteLine($"Available storage: {size}");

    // Create a file
    var respCreate = modem.FileStorage.WriteFile(FileName, Content);
    Console.WriteLine($"Create file: {(respCreate ? "success" : "failure")}");

    // Get file size
    Console.WriteLine($"File size: {modem.FileStorage.GetFileSize(FileName)}");

    // Read file
    var respRead = modem.FileStorage.ReadFile(FileName);
    if (respRead != null && respRead == Content)
    {
        Console.WriteLine($"Read file: success");
    }
    else
    {
        if (respRead != null)
        {
            Console.WriteLine($"Read file: failure, content is {respRead}");
        }
        else
        {
            Console.WriteLine($"Read file: failure");
        }
    }

    bool respDelete = false;

    // Rename file
    var respRename = modem.FileStorage.RenameFile(FileName, "test2.txt");
    if (respRename)
    {
        Console.WriteLine($"Rename file: success");
        // Delete file
        respDelete = modem.FileStorage.DeleteFile("test2.txt");
    }
    else
    {
        Console.WriteLine($"Rename file: failure");
        // Delete file
        respDelete = modem.FileStorage.DeleteFile(FileName);
    }

    Console.WriteLine($"Delete file: {(respDelete ? "success" : "failure")}");

    // Check if we have the support for directory, if yes, we test it
    if (modem.FileStorage.HasDirectorySupport)
    {
        const string DirectoryName = "test";
        // Create a directory
        var respCreateDir = modem.FileStorage.CreateDirectory(DirectoryName);
        Console.WriteLine($"Create directory: {(respCreateDir ? "success" : "failure")}");

        // Create few files in the directory
        for (int i = 0; i < 5; i++)
        {
            var res = modem.FileStorage.WriteFile($"{DirectoryName}\\test{i}.txt", Content);
            if (!res)
            {
                Console.WriteLine($"Create file {i}: failure");
            }
            else
            {
                Console.WriteLine($"Create file {i}: success");
            }
        }

        // List the directory
        var respListDir = modem.FileStorage.ListDirectory(DirectoryName);
        if (respListDir != null)
        {
            Console.WriteLine($"List directory: success");
            foreach (var file in respListDir)
            {
                Console.WriteLine($"  {file}");
            }
        }
        else
        {
            Console.WriteLine($"List directory: failure");
        }

        // Delete the directory
        var respDeleteDir = modem.FileStorage.DeleteDirectory(DirectoryName);
        Console.WriteLine($"Delete directory: {(respDeleteDir ? "success" : "failure")}, failure is normal if the directory is not empty.");

        foreach (string file in respListDir)
        {
            Console.WriteLine($"Delete file {file}: {(modem.FileStorage.DeleteFile($"{DirectoryName}\\{file}") ? "success" : "failure")}");
        }

        respDeleteDir = modem.FileStorage.DeleteDirectory(DirectoryName);
        Console.WriteLine($"Delete directory: {(respDeleteDir ? "success" : "failure")}. Should be success as the directory should be empty.");
    }
}

void TestBinaryStorage()
{
    byte[] bytes = new byte[255];

    for (int i = 0; i < bytes.Length; i++)
    {
        bytes[i] = (byte)i;
    }

    // Create the file
    var respCreate = modem.FileStorage.WriteFile("test.bin", bytes);
    if (respCreate)
    {
        Console.WriteLine($"Create file: success");
    }
    else
    {
        Console.WriteLine($"Create file: failure");
    }

    // Read the file, forces to test the resize feature as well
    byte[] buff = new byte[0];
    var respRead = modem.FileStorage.ReadFile("test.bin", ref buff);
    if (respRead)
    {
        Console.WriteLine($"Read file: success");
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] != buff[i])
            {
                Console.WriteLine($"Read file: failure, byte {i} is {buff[i]} instead of {bytes[i]}");
            }
        }
    }
    else
    {
        Console.WriteLine($"Read file: failure");
    }

    // Delete the file to clean everything
    var respDelete = modem.FileStorage.DeleteFile("test.bin");
    if (respDelete)
    {
        Console.WriteLine($"Delete file: success");
    }
    else
    {
        Console.WriteLine($"Delete file: failure");
    }
}

void ModemNetworkConnectionChanged(object sender, NetworkConnectionEventArgs e)
{
    Console.WriteLine($"Network connection changed to: {e.NetworkRegistration}");
}

void NetworkApplicationNetworkEvent(object sender, ApplicationNetworkEventArgs e)
{
    Console.WriteLine($"Application network event received, connection is: {e.IsConnected}");
}