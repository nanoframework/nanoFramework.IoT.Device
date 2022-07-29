// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Yx5300;
using nanoFramework.Hardware.Esp32;
using System.Threading;

const int FolderNumber = 1;
const int FileNumber = 1;
Yx5300 mp3Player;

// Set GPIO functions for COM2 (this is UART2 on ESP32)
Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);

// Open COM2 and instantiate player
mp3Player = new Yx5300("COM2");

// Start player and play some files
Thread.Sleep(1000);
mp3Player.Volume(Yx5300.MaxVolume / 2);

// Repeat a folder
mp3Player.PlayFolderRepeat(FolderNumber);
mp3Player.Play();

// Repeat a file
mp3Player.PlayTrackRepeat(FileNumber);
mp3Player.Play();

Thread.Sleep(Timeout.Infinite);
