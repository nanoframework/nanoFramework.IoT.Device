// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.AtomQrCode;
using nanoFramework.Hardware.Esp32;
using System.Diagnostics;
using System.Threading;

///////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the UART GPIOs
// the GPIOs below are the ones from the ATOM device that's part of the
// ATOM QR Code reader kit from M5Stack.
Configuration.SetPinFunction(22, DeviceFunction.COM2_RX);
Configuration.SetPinFunction(19, DeviceFunction.COM2_TX);

// reader is connected to COM2
QrCodeReader reader = new QrCodeReader("COM2");

// set scan mode to AUTOSENSING 
reader.TriggerMode = TriggerMode.Autosensing;

// setup handler
reader.BarcodeDataAvailable += Reader_BarcodeDataAvailable;

// start scanning
reader.StartScanning();

Thread.Sleep(Timeout.Infinite);

void Reader_BarcodeDataAvailable(object sender, BarcodeDataAvailableEventArgs e)
{
    Debug.WriteLine("");
    Debug.WriteLine("*** barcode data received ***");
    Debug.WriteLine("");
    Debug.WriteLine(e.BarcodeData);
    Debug.WriteLine("");
    Debug.WriteLine("*****************************");
    Debug.WriteLine("");
}
