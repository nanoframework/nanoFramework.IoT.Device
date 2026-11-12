// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Yx5300;
using nanoFramework.Hardware.Esp32;
using System;
using System.Diagnostics;
using System.Threading;

const int FolderNumber = 1;
const int FileNumber = 1;
const bool SelectFolderBeforeCounting = true;
Yx5300 mp3Player;

// Set GPIO functions for COM2 (this is UART2 on ESP32)
Configuration.SetPinFunction(Gpio.IO17, DeviceFunction.COM2_TX);
Configuration.SetPinFunction(Gpio.IO16, DeviceFunction.COM2_RX);

// Open COM2 and instantiate player
mp3Player = new Yx5300("COM2");

// Start player and inspect the TF card
Thread.Sleep(1000);
int volume = mp3Player.GetVolume();
Debug.WriteLine($"Volume: {volume}");
int folderCount = mp3Player.GetFolderCount();
Debug.WriteLine($"Folders: {folderCount}");
Debug.WriteLine($"Files on card: {mp3Player.GetTotalFileCount()}");
if (SelectFolderBeforeCounting)
{
    mp3Player.Volume(0);
    EnsureCommandAccepted(mp3Player, nameof(Yx5300.Volume));
    Thread.Sleep(100);
}

try
{
    for (int folderNumber = 1; folderNumber <= folderCount; folderNumber++)
    {
        if (SelectFolderBeforeCounting)
        {
            // Some clones report the last-played folder's count regardless of the query parameter.
            mp3Player.PlaySpecific(folderNumber, FileNumber);
            EnsureCommandAccepted(mp3Player, $"PlaySpecific(folder {folderNumber}, file {FileNumber})");
            Thread.Sleep(500);
        }

        Debug.WriteLine($"Files in folder {folderNumber}: {mp3Player.GetFolderFileCount(folderNumber)}");
    }
}
finally
{
    if (SelectFolderBeforeCounting)
    {
        try
        {
            Thread.Sleep(100);
            mp3Player.Stop();
            EnsureCommandAccepted(mp3Player, nameof(Yx5300.Stop));
        }
        finally
        {
            // Allow the module to finish stopping before changing the volume.
            Thread.Sleep(100);
            mp3Player.Volume(volume);
            EnsureCommandAccepted(mp3Player, "Volume(restore)");
            Thread.Sleep(100);
        }
    }
}

// Play a file from a numbered folder at half volume
mp3Player.Volume(Yx5300.MaxVolume / 2);
EnsureCommandAccepted(mp3Player, nameof(Yx5300.Volume));
Thread.Sleep(100);
mp3Player.PlaySpecific(FolderNumber, FileNumber);
EnsureCommandAccepted(mp3Player, $"PlaySpecific(folder {FolderNumber}, file {FileNumber})");
Thread.Sleep(500);
Debug.WriteLine($"Playing file: {mp3Player.GetPlayingFile()}");

// Repeat the current file
mp3Player.Repeat(true);
EnsureCommandAccepted(mp3Player, nameof(Yx5300.Repeat));

while (true)
{
    Thread.Sleep(1000);
}

static void EnsureCommandAccepted(Yx5300 player, string command)
{
    if (player.GetStatusCode() != Yx5300.StatusCode.AcknowledgeOk)
    {
        string message = $"{command} failed: status {player.GetStatusCode()}, data {player.GetStatusData()}.";
        Debug.WriteLine(message);
        throw new InvalidOperationException(message);
    }
}
