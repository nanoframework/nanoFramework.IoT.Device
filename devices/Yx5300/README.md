# YX5200/YX5300/YX6300 Serial MP3 Players

Use this binding to control UART MP3 player modules based on the YX5200, YX5300, and YX6300 command protocol. These modules play MP3, WAV, and, where supported by the module, WMA files from a microSD/TF card.

Known compatible modules and product names include:

- Catalex Serial MP3 Player modules based on the YX5300 or YX6300.
- Keyestudio KS0387 YX5200-24SS MP3 Module.
- Jaycar XC3748 Arduino Compatible Serial MP3 Player Module.
- Altronics Z6334 Arduino Compatible Serial MP3 Player Module.
- Serial MP3 Music Player Module V1.3.2, the YX5300-based board also sold under generic UART MP3 player names.

Generic modules with similar names or layouts are compatible only when they implement the same 10-byte YX5200/YX5300/YX6300 UART protocol.

## Documentation

See the [Keyestudio YX5200-24SS documentation](https://wiki.keyestudio.com/KS0387_keyestudio_YX5200-24SS_MP3_Module) and the [YX5300/YX6300 protocol documentation](https://majicdesigns.github.io/MD_YX5300/) for module details.

## Usage

```csharp
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
```
