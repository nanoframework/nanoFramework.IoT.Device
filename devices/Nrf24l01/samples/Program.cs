// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.Nrf24l01;

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
SpiConnectionSettings senderSettings = new(1, 42)
{
    ClockFrequency = Nrf24l01.SpiClockFrequency,
    Mode = Nrf24l01.SpiMode
};

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(24, DeviceFunction.SPI2_MOSI);
//Configuration.SetPinFunction(25, DeviceFunction.SPI2_MISO);
//Configuration.SetPinFunction(26, DeviceFunction.SPI2_CLOCK);
// Make sure as well you are using the right chip select
SpiConnectionSettings receiverSettings = new(2, 44)
{
    ClockFrequency = Nrf24l01.SpiClockFrequency,
    Mode = Nrf24l01.SpiMode
};
using SpiDevice senderDevice = SpiDevice.Create(senderSettings);
using SpiDevice receiverDevice = SpiDevice.Create(receiverSettings);

// SPI Device, CE Pin, IRQ Pin, Receive Packet Size
using Nrf24l01 sender = new(senderDevice, 23, 24, 20);
using Nrf24l01 receiver = new(receiverDevice, 5, 6, 20);
// Set sender send address, receiver pipe0 address (Optional)
byte[] receiverAddress = Encoding.UTF8.GetBytes("NRF24");
sender.Address = receiverAddress;
receiver.Pipe0.Address = receiverAddress;

// Binding DataReceived event
receiver.DataReceived += Receiver_ReceivedData;

// Loop
while (true)
{
    sender.Send(Encoding.UTF8.GetBytes("Hello! .NET nanoFramework"));
    Thread.Sleep(2000);
}

void Receiver_ReceivedData(object sender, DataReceivedEventArgs e)
{
    var raw = e.Data;
    var res = Encoding.UTF8.GetString(raw, 0, raw.Length);

    Debug.Write("Received Raw Data: ");
    foreach (var item in raw)
    {
        Debug.Write($"{item} ");
    }

    Debug.WriteLine("");

    Debug.WriteLine($"Message: {res}");
    Debug.WriteLine("");
}
