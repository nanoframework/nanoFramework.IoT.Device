# YX5200/YX5300 - MP3 Player

Use this driver to control the Keyestudio YX5200-24SS MP3/Jaycar XC3748 Music Player Module

This MP3 module is a MP3/WAV/WMA music player. It uses YX5200/YX5300 at its core and it plays files from an integrated SD card reader. It is connected with Serial port.

## Documentation

See [this article](https://wiki.keyestudio.com/KS0387_keyestudio_YX5200-24SS_MP3_Module) for a good oversight.

## Usage

```csharp
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
```
