# YX5200/YX5300/YX6300 Serial MP3 Players

Use this binding to control UART MP3 player modules based on the YX5200, YX5300, and YX6300 command protocol. These modules play MP3, WAV, and, where supported by the module, WMA files from a microSD/TF card.

Known compatible modules and product names include:

- Catalex Serial MP3 Player modules based on the YX5300 or YX6300.
- Keyestudio KS0387 YX5200-24SS MP3 Module.
- Jaycar XC3748 Arduino Compatible Serial MP3 Player Module.
- Altronics Z6334 Arduino Compatible Serial MP3 Player Module.
- Serial MP3 Music Player Module V1.3.2, the YX5300-based board also sold under generic UART MP3 player names.

Generic modules with similar names or layouts are compatible only when they implement the same 10-byte YX5200/YX5300/YX6300 UART protocol.

GD3300D-based boards can be sold as Serial MP3 Player V1.3.2, but should not be assumed to implement every command. Use separate volume and playback commands rather than relying on combined play-with-volume support.

## Documentation

See the [Keyestudio YX5200-24SS documentation](https://wiki.keyestudio.com/KS0387_keyestudio_YX5200-24SS_MP3_Module) and the [YX5300/YX6300 protocol documentation](https://majicdesigns.github.io/MD_YX5300/) for module details.

## Usage

Folders should be named `01`, `02`, `03`, etc., with files named `001.mp3`, `002.mp3`, etc.

The sample enables `SelectFolderBeforeCounting` to work around repeated per-folder counts on clone modules: mute playback, play the first file in each folder, wait for selection, and query its count. This behavior is documented for [GD3200B clones](https://github.com/Makuna/DFMiniMp3/discussions/142), and has also been confirmed on a GD3300D-based Serial MP3 Player V1.3.2 board. It changes playback selection and requires a playable first file in every folder. Playback is stopped and the original volume restored after counting. Set the flag to `false` on modules that honor the folder query parameter directly. The driver's `GetFolderFileCount` method only queries the module; it does not change playback.

After counting, the sample uses `PlaySpecific(FolderNumber, FileNumber)` to address a file in a numbered folder explicitly, rather than `PlayTrack`'s module-wide index. Command failures include the operation name, status, and device error data; the `FileNotFound` status (`0x40`) represents a device error packet and does not by itself establish that a file is missing.

The sample leaves 100 ms settling gaps before stopping, before restoring volume, and before the following volume change. These gaps are intended to avoid back-to-back cleanup commands on clone modules; an acknowledgement may arrive before the module is ready for its next command. Failures are logged before throwing so a cleanup failure does not hide an earlier command's diagnostics.

```csharp
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
```
