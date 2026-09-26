# M24SR - dynamic NFC tag

The M24SR is an NFC Forum Type 4 dynamic tag that exposes the same NDEF storage over NFC and I2C. It is included on boards such as the ST B-L475E-IOT01A Discovery kit. Unlike a register-based EEPROM, the I2C interface uses framed ISO 14443-4 APDU commands with CRC checking and session arbitration.

## Documentation

- [M24SR64-Y product page](https://www.st.com/en/nfc/m24sr64-y.html)
- [M24SR64-Y datasheet](https://www.st.com/resource/en/datasheet/m24sr64-y.pdf)
- [B-L475E-IOT01A Discovery kit](https://www.st.com/en/evaluation-tools/b-l475e-iot01a.html)

## Usage

**Important**: make sure you properly setup the I2C pins especially for ESP32 before creating the `I2cDevice`, make sure you install the `nanoFramework.Hardware.ESP32 nuget`:

```csharp
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
```

The on-board M24SR64-Y uses the 7-bit I2C address `0x56`. The sample uses I2C bus 2; adjust the bus number if the board firmware maps that peripheral differently.

An I2C session must be opened before accessing the tag and closed afterward. `OpenSession` retries normal session acquisition up to the configured polling limit and does not intentionally interrupt NFC activity. It throws `InvalidOperationException` if the session remains unavailable. Use `KillSession` only when taking ownership is more important than preserving an active RF session.

```csharp
using Iot.Device.M24Sr;
using Iot.Device.Ndef;
using System.Device.I2c;
using System.Diagnostics;

I2cConnectionSettings settings = new(2, M24Sr.DefaultI2cAddress);
using M24Sr tag = new(I2cDevice.Create(settings));

tag.OpenSession();
try
{
    M24SrCapabilityContainer capabilityContainer = tag.ReadCapabilityContainer();
    NdefMessage message = tag.ReadNdefMessage();

    Debug.WriteLine($"NDEF capacity: {capabilityContainer.MaximumNdefFileSize - 2} bytes");
    Debug.WriteLine($"NDEF length: {message.Length} bytes, records: {message.Records.Count}");
}
finally
{
    tag.CloseSession();
}
```

`ReadNdefMessage` and `WriteNdefMessage` reuse the repository's `Iot.Device.Ndef` message model. Writes first set the two-byte NDEF length to zero, update the payload in capability-container-sized chunks, and commit the final length last so an NFC reader does not observe a partially updated message.

The default answer polling limit is 80 attempts with a one-millisecond delay. Increase it with the constructor's `answerPollingAttempts` parameter if the target's I2C implementation or tag write timing requires more margin.
