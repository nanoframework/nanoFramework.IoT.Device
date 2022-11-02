// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Mfrc522;
using Iot.Device.Ndef;
using Iot.Device.Rfid;

Debug.WriteLine("Hello MFRC522!");

GpioController gpioController = new GpioController();
// adjust the GPIO used for the hard reset
int pinReset = 21;

// Default on ESP32:
// GPIO23 = MOSI; GPIO25 = MISO; GPIO19 = Clock

//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(23, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(25, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(19, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
// Uncomment for SPI
SpiConnectionSettings connection = new(1, 22);
// Here you can use as well MfRc522.MaximumSpiClockFrequency which is 10_000_000
// Anything lower will work as well
connection.ClockFrequency = 5_000_000;
SpiDevice spi = SpiDevice.Create(connection);
MfRc522 mfrc522 = new(spi, pinReset, gpioController, false);

// Uncomment for I2C. WARNING: you need to know the correct address (it's programmable)
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
////int i2cAddress = 0x40;
////I2cDevice i2c = I2cDevice.Create(new I2cConnectionSettings(1, i2cAddress));
////MfRc522 mfrc522 = new(i2c, pinReset, gpioController, false);

// Uncomment for serial port, adjust the port name
////MfRc522 mfrc522 = new("COM1", pinReset, gpioController, false);

Debug.WriteLine($"Version: {mfrc522.Version}, version should be 1 or 2. Some clones may appear with version 0");
Debug.WriteLine("Place your Mifare or Ultralight card on the reader. The default B key for Mifare is set to 0xFF 0xFF 0xFF 0xFF 0xFF 0xFF will be used to read the card. The default password for Ultralight is set to 0xFF 0xFF 0xFF 0xFF and will be used if write permissions require authentication.");

bool res;
Data106kbpsTypeA card;
do
{
    res = mfrc522.ListenToCardIso14443TypeA(out card, TimeSpan.FromSeconds(2));
    Thread.Sleep(res ? 0 : 200);
}
while (!res);

Debug.WriteLine("");
if (UltralightCard.IsUltralightCard(card.Atqa, card.Sak))
{
    Debug.WriteLine("Ultralight card detected, running various tests.");
    ProcessUltralight();
}
else
{
    Debug.WriteLine("Mifare card detected, dumping the memory.");
    ProcessMifare();
}

void ProcessMifare()
{
    var mifare = new MifareCard(mfrc522!, 0);
    mifare.SerialNumber = card.NfcId;
    mifare.Capacity = MifareCardCapacity.Mifare1K;
    mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
    mifare.KeyB = MifareCard.DefaultKeyB.ToArray();
    int ret;

    for (byte block = 0; block < 64; block++)
    {
        mifare.BlockNumber = block;
        mifare.Command = MifareCardCommand.AuthenticationB;
        ret = mifare.RunMifareCardCommand();
        if (ret < 0)
        {
            // If you have an authentication error, you have to deselect and reselect the card again and retry
            // Those next lines shows how to try to authenticate with other known default keys
            mifare.ReselectCard();
            // Try the other key
            mifare.KeyA = MifareCard.DefaultKeyA.ToArray();
            mifare.Command = MifareCardCommand.AuthenticationA;
            ret = mifare.RunMifareCardCommand();
            if (ret < 0)
            {
                mifare.ReselectCard();
                mifare.KeyA = MifareCard.DefaultBlocksNdefKeyA.ToArray();
                mifare.Command = MifareCardCommand.AuthenticationA;
                ret = mifare.RunMifareCardCommand();
                if (ret < 0)
                {
                    mifare.ReselectCard();
                    mifare.KeyA = MifareCard.DefaultFirstBlockNdefKeyA.ToArray();
                    mifare.Command = MifareCardCommand.AuthenticationA;
                    ret = mifare.RunMifareCardCommand();
                    if (ret < 0)
                    {
                        mifare.ReselectCard();
                        Debug.WriteLine($"Error reading bloc: {block}");
                    }
                }
            }
        }

        if (ret >= 0)
        {
            mifare.BlockNumber = block;
            mifare.Command = MifareCardCommand.Read16Bytes;
            ret = mifare.RunMifareCardCommand();
            if (ret >= 0)
            {
                if (mifare.Data is object)
                {
                    Debug.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifare.Data)}");
                }
            }
            else
            {
                mifare.ReselectCard();
                Debug.WriteLine($"Error reading bloc: {block}");
            }

            if (block % 4 == 3)
            {
                if (mifare.Data != null)
                {
                    // Check what are the permissions
                    for (byte j = 3; j > 0; j--)
                    {
                        var access = mifare.BlockAccess((byte)(block - j), mifare.Data);
                        Debug.WriteLine($"Bloc: {block - j}, Access: {access}");
                    }

                    var sector = mifare.SectorTailerAccess(block, mifare.Data);
                    Debug.WriteLine($"Bloc: {block}, Access: {sector}");
                }
                else
                {
                    Debug.WriteLine("Can't check any sector bloc");
                }
            }
        }
        else
        {
            Debug.WriteLine($"Authentication error");
        }
    }
}

void ProcessUltralight()
{
    var ultralight = new UltralightCard(mfrc522!, 0);
    ultralight.SerialNumber = card.NfcId;
    Debug.WriteLine($"Type: {ultralight.UltralightCardType}, Ndef capacity: {ultralight.NdefCapacity}");

    var version = ultralight.GetVersion();
    if ((version != null) && (version.Length > 0))
    {
        Debug.WriteLine("Get Version details: ");
        for (int i = 0; i < version.Length; i++)
        {
            Debug.Write($"{version[i]:X2} ");
        }

        Debug.WriteLine("");
    }
    else
    {
        Debug.WriteLine("Can't read the version.");
    }

    var sign = ultralight.GetSignature();
    if ((sign != null) && (sign.Length > 0))
    {
        Debug.WriteLine("Signature: ");
        for (int i = 0; i < sign.Length; i++)
        {
            Debug.Write($"{sign[i]:X2} ");
        }

        Debug.WriteLine("");
    }
    else
    {
        Debug.WriteLine("Can't read the signature.");
    }

    // The ReadFast feature can be used as well, note that the MFRC522 has a very limited FIFO
    // So maximum 9 pages can be read as once.
    Debug.WriteLine("Fast read example:");
    var buff = ultralight.ReadFast(0, 8);
    if (buff != null)
    {
        for (int i = 0; i < buff.Length / 4; i++)
        {
            Debug.WriteLine($"  Block {i} - {buff[i * 4]:X2} {buff[i * 4 + 1]:X2} {buff[i * 4 + 2]:X2} {buff[i * 4 + 3]:X2}");
        }
    }

    Debug.WriteLine("Dump of all the card:");
    for (int block = 0; block < ultralight.NumberBlocks; block++)
    {
        ultralight.BlockNumber = (byte)block; // Safe cast, can't be more than 255
        ultralight.Command = UltralightCommand.Read16Bytes;
        var res = ultralight.RunUltralightCommand();
        if (res > 0)
        {
            Debug.Write($"  Block: {ultralight.BlockNumber:X2} - ");
            for (int i = 0; i < 4; i++)
            {
                Debug.Write($"{ultralight.Data![i]:X2} ");
            }

            var isReadOnly = ultralight.IsPageReadOnly(ultralight.BlockNumber);
            Debug.Write($"- Read only: {isReadOnly} ");

            Debug.WriteLine("");
        }
        else
        {
            Debug.WriteLine("Can't read card");
            break;
        }
    }

    Debug.WriteLine("Configuration of the card");
    // Get the Configuration
    res = ultralight.TryGetConfiguration(out Iot.Device.Card.Ultralight.Configuration configuration);
    if (res)
    {
        Debug.WriteLine("  Mirror:");
        Debug.WriteLine($"    {configuration.Mirror.MirrorType}, page: {configuration.Mirror.Page}, position: {configuration.Mirror.Position}");
        Debug.WriteLine("  Authentication:");
        Debug.WriteLine($"    Page req auth: {configuration.Authentication.AuthenticationPageRequirement}, Is auth req for read and write: {configuration.Authentication.IsReadWriteAuthenticationRequired}");
        Debug.WriteLine($"    Is write lock: {configuration.Authentication.IsWritingLocked}, Max num tries: {configuration.Authentication.MaximumNumberOfPossibleTries}");
        Debug.WriteLine("  NFC Counter:");
        Debug.WriteLine($"    Enabled: {configuration.NfcCounter.IsEnabled}, Password protected: {configuration.NfcCounter.IsPasswordProtected}");
        Debug.WriteLine($"  Is strong modulation: {configuration.IsStrongModulation}");
    }
    else
    {
        Debug.WriteLine("Error getting the configuration");
    }

    NdefMessage message;
    res = ultralight.TryReadNdefMessage(out message);
    if (res && message.Length != 0)
    {
        foreach (NdefRecord record in message.Records)
        {
            Debug.WriteLine($"Record length: {record.Length}");
            if (TextRecord.IsTextRecord(record))
            {
                var text = new TextRecord(record);
                Debug.WriteLine(text.Text);
            }
        }
    }
    else
    {
        Debug.WriteLine("No NDEF message in this ");
    }

    res = ultralight.IsFormattedNdef();
    if (!res)
    {
        Debug.WriteLine("Card is not NDEF formated, we will try to format it");
        res = ultralight.FormatNdef();
        if (!res)
        {
            Debug.WriteLine("Impossible to format in NDEF, we will still try to write NDEF content.");
        }
        else
        {
            res = ultralight.IsFormattedNdef();
            if (res)
            {
                Debug.WriteLine("Formating successful");
            }
            else
            {
                Debug.WriteLine("Card is not NDEF formated.");
            }
        }
    }

    NdefMessage newMessage = new NdefMessage();
    newMessage.Records.Add(new TextRecord("I ❤ .NET IoT", "en", Encoding.UTF8));
    res = ultralight.WriteNdefMessage(newMessage);
    if (res)
    {
        Debug.WriteLine("NDEF data successfully written on the card.");
    }
    else
    {
        Debug.WriteLine("Error writing NDEF data on card");
    }
}
