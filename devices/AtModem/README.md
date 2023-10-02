# Generic AT Modem SIM800 and SIM7070, SIM7080, SIM7090 - Dual Mode Wireless Module CatM, LTE modems

This binding is a generic AT Modem handler that can be extended for different usage. The first implementation is for the `Sim7080` which supports both `CAT-M` and `NB-IoT`. It can be controlled through AT command via a Serial/UART interface. There is as well a partial implementation of `SIM800`.

> Note: The module is tested on a `Sim7080` but is also compatible with Sim7070 and Sim7090 and with a `SIM800H`.

## Documentation

* [SIM7080 Product details](https://www.simcom.com/index.php/product/SIM7080G.html)
* [SIM800 Product details](https://www.simcom.com/product/SIM800C.html)

Supported features for the different modems are the following:

| Feature | SIM800 | SIM7080 |
| --- | --- | --- |
|SMS|✅|✅|
|Call|✅|✅|
|Storage|✅|✅|
|HTTP and HTTPS|✅|✅|
|MQTT and MQTTS|❌|✅|
|IP Network connection|✅|✅|

Most modems supports other features like MMS, Sockets with TCP/UDP, FTP, email, GNSS and more. So far, those features are not implemented. They can be added later. All the mechanism is present to add them and described in the documentation bellow especially regarding the [Channel property](#the-channel-property).

Note that a more complete implementation for the SIM800H is available in [Eclo Solutions github repository](https://github.com/Eclo/nanoFramework.SIM800H).

## Usage

**Important**: make sure you properly setup the RT/RX pins especially for ESP32 before creating the `SerialPort`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
SerialPort _serialPort;
OpenSerialPort("COM3");

_serialPort.NewLine = "\r\n";
AtChannel atChannel = AtChannel.Create(_serialPort);
Sim7080 modem = new(atChannel);

private static void OpenSerialPort(
    string port = "COM3",
    int baudRate = 115200,
    Parity parity = Parity.None,
    StopBits stopBits = StopBits.One,
    Handshake handshake = Handshake.None,
    int dataBits = 8,
    int readTimeout = Timeout.Infinite,
    int writeTimeout = Timeout.Infinite)
{
    // This section is specific to ESP32 targets
    // Configure GPIOs 16 and 17 to be used in UART2 (that's refered as COM3)
    Configuration.SetPinFunction(32, DeviceFunction.COM3_RX);
    Configuration.SetPinFunction(33, DeviceFunction.COM3_TX);

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
```

Note that each modem can support multiple baud rates and have automatic baud rates. You can set it up once the modem is awake using the `SetBaudRate` function.

Some modems like the SIM800 can have an external pin to set high/low. This binding does not handle this behavior. You should use [GpioController](https://github.com/nanoframework/System.Device.Gpio) to adjust those pins. It is also recommended for those to have a proper hardware reset pattern in case.

Each modem derives from a `ModemBase`. It does offers to access the different features through properties. For example, accessing to the File Storage will be through the property `FileStorage` once the modem is open. If your modem does not implement this feature, it will throw a non implemented exception.

All modems should implement the `ISmsProver` and `ICall` as those are standards. So you should be safe using them. Those properties `SmsProvider` and `Call` implements, by default a standard class that will work with the standard. Each modem can later derive and override those classes to add more features for example.

## The Channel property

The `Channel` property which is an `AtChannel` class allows you to control and write your own features when not implemented already. The class implement specific functions allowing to send and received AT commands but also raw data. There is an always running thread to read continuously the modem output that can be stopped and started on demand as well.

Most of the time, you will be using simple commands that allow you to send just a command and wait for a OK or Error or for a specific text. This is done through the following functions:

```csharp
// This command is sending a simple command and waiting for OR or ERROR.
// Internally it uses a default timeout. A specific one can be passed as well.
AtResponse response = Channel.SendCommand($"AT+IPR={baudRate}");
if (response.Success)
{
    Console.WriteLine("The command returned OK");
    // You can proceed and do something else
}
else
{
    Console.WriteLine("The command returned ERROR!");
    // You may retry or do anything else
}
```

You can wait as well to get a specific partial answer with one or multiple lines:

```csharp
AtResponse response = Channel.SendCommandReadSingleLine($"AT+CSCS=?", "+CSCS:");
if (response.Success)
{
    // This means we have read 1 line stating with "+CSCS:"
    // We have ignore all the rest up to a OK
}
else
{
    // In this case, there is ERROR at the end or it does not contains "+CSCS:" or a timeout
}
```

This is an example for reading multiple lines:

```csharp
AtResponse response = Channel.SendCommandReadMultiline("ATI", null);
// Here, we don't wait for a specific answer, any will be taken up to OK or ERROR or a timeout
if (response.Success)
{
    // This means that there is a OK at the end
    StringBuilder builder = new StringBuilder();
    foreach (string line in response.Intermediates)
    {
        builder.AppendLine(line);
    }

    Console.WriteLine(builder.ToString());
}
```

As an example of something more advance, you can stop the main receiving thread, add your own logic and restart it after, you can do something like this:

```csharp
Modem.Channel.Stop();
Modem.Channel.SendBytesWithoutAck(Encoding.UTF8.GetBytes($"AT+SHREAD=0,{_httpActionResult.DataLenght}\r\n"));

// Do a lot of things here like reading manually
var chunk = Modem.Channel.ReadRawBytes(42);

// Then clean the output and restart the receiving thread
Modem.Channel.Clear();
Modem.Channel.Start();
```

## Internal storage

Each modem may contain an internal storage. Some does have basic file support only, some may contains directory as well and or multiple drives/storage area.

The implementation should be done through the `IFileStorage` interface and can be accessed with the property `FileStorage`. Some devices will have support for folder, this will be indicated by the property `HasDirectorySupport`.

You can create, read files, delete and rename them and have the size of a file and storage:

```csharp
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
```

Assuming we have directory support, you can create, list and delete directories:

```csharp
const string DirectoryName = "Test";
const string Content = " nanoFramework!";
// Create a directory
var respCreateDir = modem.FileStorage.CreateDirectory(DirectoryName);
Console.WriteLine($"Create directory: {(respCreateDir ? "success" : "failure")}");

// You can create file in it
res = modem.FileStorage.WriteFile($"{DirectoryName}\\test.txt", Content);
if (!res)
{
    Console.WriteLine($"Create file: failure");
}
else
{
    Console.WriteLine($"Create file: success");
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
```

## SMS

SMS features are almost fully supported. This implementation does not *yet* support UCS2. All the rest is supported. The property `SmsProvider` allow to access the SMS features.

> **Important**: you need to make sure you have a proper network connection before being able to send or receive SMS. See the [networking section](#networking).

Even when the connection is available, you may not have the SMS engine ready, check it with the `IsSmsReady` property:

```csharp
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
```

You can list the SMS like this:

```csharp
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
}
```

You can read a specific SMS knowing its index:

```csharp
// Assuming in this example that the SMS index is 1
var respSmsRead = modem.SmsProvider.ReadSms(1, SmsTextFormat.PDU);
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
```

You can send an SMS using the Text or the PDU format. PDU will allow you to specify a specific encoding (note: UCS2 is not *yet* supported):

```csharp
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
```

## Networking

Your SIM card may be with a pin code. You can check this with the `GetSimStatus` function.

```csharp
var pinStatus = modem.GetSimStatus();
if (pinStatus.IsSuccess)
{
    Console.WriteLine($"SIM status: {(SimStatus)pinStatus.Result}");
    // Do we need a pin code?
    if ((SimStatus)pinStatus.Result == SimStatus.PinRequired)
    {
        // Provide the pin code
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
```

Depending on your provider, you may have to provide APN details and have a manual way of connecting to the network. You can achieve this like this:

```csharp
var network = modem.Network;
// If you have a pin code, you should pass it
// var connectRes = network.Connect(new PersonalIdentificationNumber("1234"), new AccessPointConfiguration("free"));
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
```

A high level function is also available to wait for a connection to happen. You should test how things work with your provider.

```csharp
// Wait for network registration for 2 minutes max, if not connected, then something is most likely very wrong
var isConnected = modem.WaitForNetworkRegistration(new CancellationTokenSource(120_000).Token);
```

And you also have functions to understand how you are connected to the network:

```csharp
var networkReg = GetNetworkRegistration();
if (networkReg.IsSuccess)
{
    if ((NetworkRegistration)networkReg.Result == NetworkRegistration.RegisteredHomeNetwork || (NetworkRegistration)networkReg.Result == NetworkRegistration.RegisteredRoaming)
    {
        // We are connected
    }
}
```

Once connected to the network, you have also an event that gives you date and time update. This is convenient to set your device date time:

```csharp
var network = modem.Network;
network.DateTimeChanged += NetworkDateTimeChanged;

void NetworkDateTimeChanged(object sender, DateTimeEventArgs e)
{
    // Set the native date time
    Rtc.SetSystemTime(e.DateTime);
    Console.WriteLine($"Date and time received, it is now {DateTime.UtcNow}");
}
```

Other events are available related to the state of the connection. The network connection is the connection to the operator while the application network is typically the IP connection:

```csharp
modem.NetworkConnectionChanged += ModemNetworkConnectionChanged;
// This will set the modem to reconnect when disconnected
modem.Network.AutoReconnect = true;
modem.Network.ApplicationNetworkEvent += NetworkApplicationNetworkEvent;

void ModemNetworkConnectionChanged(object sender, NetworkConnectionEventArgs e)
{
    Console.WriteLine($"Network connection changed to: {e.NetworkRegistration}");
}

void NetworkApplicationNetworkEvent(object sender, ApplicationNetworkEventArgs e)
{
    Console.WriteLine($"Application network event received, connection is: {e.IsConnected}");
}
```

Note that you may have to handle reconnection mechanism also even if a lot is already done for you in the modem and in the code.

You can also list the operators. Like when you do this on your phone, this may take minutes!

```csharp
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
```

You have additional functions to execute USSD codes, get the battery level, get the signal strength, get the subscriber number and more!

## Call

You can manage calls with any modem. A generic implementation is provided through the `Call` property. You can be notified with events on the call status and take action on them:

```csharp
var call = modem.Call;
call.IncomingCall += CallIncomingCall;
call.CallStarted += CallCallStarted;
call.CallEnded += CallCallEnded;
call.MissedCall += CallMissedCall;

// Let's do a call now and being anonymous ;-)
call.Dial(new PhoneNumber("+33123456789"), true);

void CallCallStarted(object sender, CallStartedEventArgs e)
{
    Console.WriteLine("A call has started");
}

void CallMissedCall(object sender, MissedCallEventArgs e)
{
    Console.WriteLine($"Missed call from {e.PhoneNumber} at {e.Time}");
}

void CallIncomingCall(object sender, IncomingCallEventArgs e)
{
    Console.WriteLine($"Incoming!");
    // We have an incoming call, we answer it
    modem.Call.AnswerIncomingCall();
}

void CallCallEnded(object sender, CallEndedEventArgs e)
{
    Console.WriteLine($"Call ended!");
}
```

> Note: You may have to add a microphone and speakers in order to use those features. They may not be exposed in your hardware depending on what you are getting as board.

## MQTT and MQTT Secured

You can use a MQTT client implementing the [IMqttClient](https://github.com/nanoframework/nanoFramework.m2mqtt/blob/main/nanoFramework.M2Mqtt.Core/IMqttClient.cs) interface.

> **Important**: Each modem may not support all the features or have limitations with the topic and payload length. Make sure you check the limitations especially if you want to use those with Azure or AWS IoT.

Because of the interface, you can use the MQTT client provided by the `MqttClient` client property like you can do with the existing nanoFramework MQTT Client.

As an example, you can use the [Azure IoT fully managed nuget](https://github.com/nanoframework/nanoFramework.Azure.Devices) and pass the MqttClient property plus the certificate:

```csharp
// This specific modem only support binary DER certificate format (not PEM):
DeviceClient azureIoT = new DeviceClient(modem.MqttClient, IotBrokerAddress, DeviceID, SasKey, azureCert: Resource.GetBytes(Resource.BinaryResources.DigiCertGlobalRootG2));
```

The fact that the `MqttClient` uses this `IMqttClient` interface will allow you to reuse the almost same code between different implementations, though Wifi, Ethernet and AT Modem. A full Azure sample is provided in the [Samples](https://github.com/nanoframewrok/Samples) repository.

### MQTTS Certificate validation

The certificate validation for MQTT secured connections happens only if you provide a certificate. If you pass a null server certificate, the server certificate verification will be ignored.

In the SIM7070/7080/7090, the only certificate format supported is DER, meaning, the binary representation of the certificate (usually with .crt extension) and not the PEM format (that is a base64 text encoded representation).

> **Important**: A `X509Certificate` fully managed code implementation is available for convenience. It does **not** provide any property like getting the certificate validate time. It is just a wrapper to make code reuse simpler. It does **not** parse the certificate neither or transform it. If you pass a PEM certificate, getting the data will always be a PEM certificate, it is not going to transform it as a DER and vice versa.

## HTTP and HTTPS

A code compatible implementation of `HttpClient` that is present in the nanoFramework System.Net is provided through the `HttpClient` property. This will allow you to reuse code across different implementations though Wifi, Ethernet and AT Modem.

You will find below few examples of Get and Post below:

```csharp
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
```

### HTTPS Certificate validation

The certificate validation for HTTPS secured connections happens only if you provide a certificate. If you pass a null server certificate, the server certificate verification will be ignored.

In the SIM7070/7080/7090, the only certificate format supported is DER, meaning, the binary representation of the certificate (usually with .crt extension) and not the PEM format (that is a base64 text encoded representation).

> **Important**: A `X509Certificate` fully managed code implementation is available for convenience. It does **not** provide any property like getting the certificate validate time. It is just a wrapper to make code reuse simpler. It does **not** parse the certificate neither or transform it. If you pass a PEM certificate, getting the data will always be a PEM certificate, it is not going to transform it as a DER and vice versa.

## Articles

Part of this code is adaptation and port of [ATLib](https://github.com/hbjorgo/ATLib).

* [Starting a Narrowband IoT project with nanoFramework](https://medium.com/itnext/when-machines-talk-bccba9a8c049)
* [Establishing a connection to Azure IoT Hub using an MQTT client with nanoFramework](https://medium.com/itnext/establishing-a-connection-to-azure-iot-hub-using-an-mqtt-client-with-nanoframework-d9c2e1b4ebbe)
* [Creating a tracking device with nanoFramework](https://medium.com/itnext/creating-a-tracking-device-with-nanoframework-6d27b5b4e7ab)