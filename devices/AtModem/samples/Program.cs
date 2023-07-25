// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using IoT.Device.AtModem;
using IoT.Device.AtModem.DTOs;
using IoT.Device.AtModem.Modem;
//using nanoFramework.Hardware.Esp32;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;

Console.WriteLine("Hello SIM7080!");

SerialPort _serialPort;
//OpenSerialPort("COM3");
OpenSerialPort("COM10");

_serialPort.NewLine = "\r\n";
AtChannel atChannel = AtChannel.Create(_serialPort);
atChannel.DebugEnabled = true;
Sim7080 sim7080 = new(atChannel);

var respDeviceInfo = sim7080.GetDeviceInformation();
if (respDeviceInfo.IsSuccess)
{
    Console.WriteLine($"Device info: {respDeviceInfo.Result}");
}
else
{
    Console.WriteLine($"Device info failed: {respDeviceInfo.ErrorMessage}");
}

var pinStatus = sim7080.GetSimStatus();
if (pinStatus.IsSuccess)
{
    Console.WriteLine($"SIM status: {(SimStatus)pinStatus.Result}");
    if ((SimStatus)pinStatus.Result == SimStatus.SIM_PIN)
    {
        var pinRes = sim7080.EnterSimPin(new PersonalIdentificationNumber("1234"));
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

ConnectToNetwork();
////TestStorage();
////GetNetworkOperators();
////TestStorageSmsAndCharSet();
//TestSms();

var signalRes = sim7080.GetSignalStrength();
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
    int readTimeout = Timeout.Infinite,
    int writeTimeout = Timeout.Infinite)
{
    // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
    //Configuration.SetPinFunction(32, DeviceFunction.COM3_RX);
    //Configuration.SetPinFunction(33, DeviceFunction.COM3_TX);

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
    var network = sim7080.Network;
    var connectRes = network.Connect(new PersonalIdentificationNumber("1234"), new AccessPointConfiguration("free"));
    ////var connectRes = network.Connect(apn: new AccessPointConfiguration("orange"));
    if (connectRes)
    {
        Console.WriteLine($"Connected to network.");
    }
    else
    {
        Console.WriteLine($"Connected to network failed!");
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
    var resp = sim7080.GetAvailableCharacterSets();
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

    var respChar = sim7080.GetCurrentCharacterSet();
    if (respChar.IsSuccess)
    {
        Console.WriteLine($"Current character set: {respChar.Result}");
    }
    else
    {
        Console.WriteLine($"Current character set failed: {respChar.ErrorMessage}");
    }

    var respCpms = sim7080.SmsProvider.GetSupportedPreferredMessageStorages();
    if (respCpms.IsSuccess)
    {
        Console.WriteLine($"Supported preferred message storages: {respCpms.Result}");
        var respPrefStorageSet = sim7080.SmsProvider.SetPreferredMessageStorage(((SupportedPreferredMessageStorages)respCpms.Result).Storage1[0], ((SupportedPreferredMessageStorages)respCpms.Result).Storage2[0], ((SupportedPreferredMessageStorages)respCpms.Result).Storage3[0]);
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

    var respPRefStor = sim7080.SmsProvider.GetPreferredMessageStorages();
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
    var resplistSms = sim7080.SmsProvider.ListSmss(SmsStatus.ALL);
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
            sim7080.SmsProvider.SetSmsMessageFormat(SmsTextFormat.PDU);
            var respSmsRead = sim7080.SmsProvider.ReadSms(0, SmsTextFormat.PDU);
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

    var respSmsSend = sim7080.SmsProvider.SendSmsInTextFormat(new PhoneNumber("+33664404676"), "Hello from nanoFramework");
    if (respSmsSend.IsSuccess)
    {
        Console.WriteLine($"SMS sent successfully: {respSmsSend.Result}");
    }
    else
    {
        Console.WriteLine($"SMS sent failed: {respSmsSend.ErrorMessage}");
    }

    respSmsSend = sim7080.SmsProvider.SendSmsInPduFormat(new PhoneNumber("+33664404676"), "Hello from nanoFramework", IoT.Device.AtModem.CodingSchemes.CodingScheme.Gsm7, true);
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
    var network = sim7080.Network;

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
    var size = sim7080.FileStorage.GetAvailableStorage();
    Console.WriteLine($"Available storage: {size}");

    // Create a file
    var respCreate = sim7080.FileStorage.WriteFile(FileName, Content);
    Console.WriteLine($"Create file: {(respCreate ? "success" : "failure")}");

    // Get file size
    Console.WriteLine($"File size: {sim7080.FileStorage.GetFileSize(FileName)}");

    // Read file
    var respRead = sim7080.FileStorage.ReadFile(FileName);
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
    var respRename = sim7080.FileStorage.RenameFile(FileName, "test2.txt");
    if (respRename)
    {
        Console.WriteLine($"Rename file: success");
        // Delete file
        respDelete = sim7080.FileStorage.DeleteFile("test2.txt");
    }
    else
    {
        Console.WriteLine($"Rename file: failure");
        // Delete file
        respDelete = sim7080.FileStorage.DeleteFile(FileName);
    }

    Console.WriteLine($"Delete file: {(respDelete ? "success" : "failure")}");
}
