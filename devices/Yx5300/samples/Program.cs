// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Yx5300;
using nanoFramework.Hardware.Esp32;
using System.Diagnostics;
using System.Threading;

const int FolderNumber = 1;
const int FileNumber = 1;
Yx5300 mp3Player;

// Set GPIO functions for COM2 (this is UART2 on ESP32)
Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);

// Open COM2 and instantiate player
mp3Player = new Yx5300("COM2");

// Start player and inspect the TF card
Thread.Sleep(1000);
Debug.WriteLine($"Volume: {mp3Player.GetVolume()}");
Debug.WriteLine($"Folders: {mp3Player.GetFolderCount()}");
Debug.WriteLine($"Files on card: {mp3Player.GetTotalFileCount()}");
Debug.WriteLine($"Files in folder {FolderNumber}: {mp3Player.GetFolderFileCount(FolderNumber)}");

// Play a file at half volume
mp3Player.PlayTrackWithVolume(FileNumber, Yx5300.MaxVolume / 2);
Debug.WriteLine($"Playing file: {mp3Player.GetPlayingFile()}");

// Repeat the current file
mp3Player.Repeat(true);

Thread.Sleep(Timeout.Infinite);
