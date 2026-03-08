// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.Card.Icode;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Ndef;
using Iot.Device.Pn5180;
using Iot.Device.Rfid;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Logging;
using nanoFramework.Logging.Debug;

Debug.WriteLine("Hello Pn5180!");

// This program has been tested for a EXP32-C3 super mini board
// For any other board, make sure to adjust the SPI GPIOs and the reset pin accordingly in the code below
// For anything else than ESP32, remove the ESP32 specific nuget and code below

// Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
// LogDispatcher.LoggerFactory = new DebugLoggerFactory();
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(5, DeviceFunction.SPI1_MOSI);
nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(6, DeviceFunction.SPI1_MISO);
nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(4, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(1, 10) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

// Reset the device
using GpioController gpioController = new();
gpioController.OpenPin(3, PinMode.Output);
gpioController.Write(3, PinValue.Low);
Thread.Sleep(10);
gpioController.Write(3, PinValue.High);
Thread.Sleep(10);

// Adjust the IO
Pn5180 pn5180 = new Pn5180(spi, 1, 2, null, true);


var versions = pn5180.GetVersions();
Debug.WriteLine($"Product: {versions.Product}, Firmware: {versions.Firmware}, EEPROM: {versions.Eeprom}");

// === RF / Antenna diagnostic ===
SpanByte rawStatus = new byte[4];

// Check after reset
pn5180.GetRawRfStatus(rawStatus);
Debug.WriteLine($"[After reset] RF_STATUS raw: {BitConverter.ToString(rawStatus.ToArray())}");
Debug.WriteLine($"  RF field on: {pn5180.RadioFrequencyField}, Status: {pn5180.GetRadioFrequencyStatus()}, External: {pn5180.IsRadioFrequencyFieldExternal()}");

// Load 14443A config and turn on field
pn5180.LoadRadioFrequencyConfiguration(
    TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106,
    ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106);
pn5180.RadioFrequencyField = true;
Thread.Sleep(50);
pn5180.GetRawRfStatus(rawStatus);
Debug.WriteLine($"[14443A RF_ON] RF_STATUS raw: {BitConverter.ToString(rawStatus.ToArray())}");
Debug.WriteLine($"  RF field on: {pn5180.RadioFrequencyField}, Status: {pn5180.GetRadioFrequencyStatus()}, External: {pn5180.IsRadioFrequencyFieldExternal()}");
pn5180.RadioFrequencyField = false;
Thread.Sleep(50);

// Load 15693 config and turn on field
pn5180.LoadRadioFrequencyConfiguration(
    TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
    ReceiverRadioFrequencyConfiguration.Iso15693_26);
pn5180.RadioFrequencyField = true;
Thread.Sleep(50);
pn5180.GetRawRfStatus(rawStatus);
Debug.WriteLine($"[15693 RF_ON] RF_STATUS raw: {BitConverter.ToString(rawStatus.ToArray())}");
Debug.WriteLine($"  RF field on: {pn5180.RadioFrequencyField}, Status: {pn5180.GetRadioFrequencyStatus()}, External: {pn5180.IsRadioFrequencyFieldExternal()}");
pn5180.RadioFrequencyField = false;

// Dump a Mifare ISO 14443 type A
//TypeA();

// EEPROM operations
//Eeprom();

//Radio Frequency operations
//RfConfiguration();

// Pull ISO 14443 Type A and B cards, display information
//PullDifferentCards();

// Pull ISO 14443 B cards, display information
//PullTypeBCards();

// Pull ISO 15693 cards, display information
//PullIso15693Cards();

// Detect and process an ICODE (ISO 15693) card
ProcessIcodeCard();

// Dump Ultralight card and various tests
//ProcessUltralight();

Thread.Sleep(Timeout.Infinite);

void Eeprom()
{
    SpanByte eeprom = new byte[255];
    var ret = pn5180.ReadAllEeprom(eeprom);
    Debug.WriteLine($"EEPROM dump: success: {ret}, Data: {BitConverter.ToString(eeprom.ToArray())}");
    ret = pn5180.ReadEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 16));
    Debug.WriteLine($"EEPROM read, unique identifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
    ret = pn5180.GetIdentifier(eeprom.Slice(0, 16));
    Debug.WriteLine($"GetIdentifier: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 16).ToArray())}");
    ret = pn5180.WriteEeprom(EepromAddress.DieIdentifier, eeprom.Slice(0, 1));
    Debug.WriteLine($"Trying to write a read only EEPROM, this should return false: {ret}");
    Debug.WriteLine($"EEPROM writing will not be immediate. Some are only active after a reboot");
    Debug.WriteLine($"changing second byte of UUID when acting as a card (first is always fix to 0x08)");
    ret = pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    eeprom[0]++;
    Debug.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
    Debug.WriteLine($"New value to write: {BitConverter.ToString(eeprom.Slice(0, 1).ToArray())}");
    ret = pn5180.WriteEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    Debug.WriteLine($"Wrote IRQ_PIN_CONFIG: {ret}");
    ret = pn5180.ReadEeprom(EepromAddress.NFCID1, eeprom.Slice(0, 3));
    Debug.WriteLine($"IRQ_PIN_CONFIG: success: {ret}, Data: {BitConverter.ToString(eeprom.Slice(0, 3).ToArray())}");
}

void RfConfiguration()
{
    var sizeConfig = pn5180.GetRadioFrequencyConfigSize(TransmitterRadioFrequencyConfiguration.Iso14443B_106);
    SpanByte configBuff = new byte[Pn5180.RadioFrequencyConfigurationSize * sizeConfig];
    pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
    for (int i = 0; i < sizeConfig; i++)
    {
        Debug.WriteLine($"Register: {configBuff[Pn5180.RadioFrequencyConfigurationSize * i]}, Data: {BitConverter.ToString(configBuff.Slice(Pn5180.RadioFrequencyConfigurationSize * i + 1, Pn5180.RadioFrequencyConfigurationSize - 1).ToArray())}");
    }
}

void TypeA()
{
    // Let's pull for 20 seconds and see the result
    if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out Data106kbpsTypeA? cardTypeA, 20000))
    {
        Debug.WriteLine($"ATQA: {cardTypeA.Atqa}");
        Debug.WriteLine($"SAK: {cardTypeA.Sak}");
        Debug.WriteLine($"UID: {BitConverter.ToString(cardTypeA.NfcId)}");

        MifareCard mifareCard = new MifareCard(pn5180, cardTypeA.TargetNumber)
        {
            BlockNumber = 0,
            Command = MifareCardCommand.AuthenticationA
        };

        mifareCard.SetCapacity(cardTypeA.Atqa, cardTypeA.Sak);
        mifareCard.SerialNumber = cardTypeA.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        for (byte block = 0; block < 64; block++)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            var ret = mifareCard.RunMifareCardCommand();
            mifareCard.ReselectCard();
            if (ret < 0)
            {
                // Try another one
                mifareCard.Command = MifareCardCommand.AuthenticationA;
                ret = mifareCard.RunMifareCardCommand();
            }

            if (ret >= 0)
            {
                mifareCard.BlockNumber = block;
                mifareCard.Command = MifareCardCommand.Read16Bytes;
                ret = mifareCard.RunMifareCardCommand();
                if (ret >= 0 && mifareCard.Data is object)
                {
                    Debug.WriteLine($"Bloc: {block}, Data: {BitConverter.ToString(mifareCard.Data)}");
                }
                else
                {
                    Debug.WriteLine($"Error reading bloc: {block}");
                }

                if (block % 4 == 3 && mifareCard.Data is object)
                {
                    // Check what are the permissions
                    for (byte j = 3; j > 0; j--)
                    {
                        var access = mifareCard.BlockAccess((byte)(block - j), mifareCard.Data);
                        Debug.WriteLine($"Bloc: {block - j}, Access: {access}");
                    }

                    var sector = mifareCard.SectorTailerAccess(block, mifareCard.Data);
                    Debug.WriteLine($"Bloc: {block}, Access: {sector}");
                }
            }
            else
            {
                Debug.WriteLine($"Authentication error");
            }
        }
    }
    else
    {
        Debug.WriteLine($"{nameof(cardTypeA)} card cannot be read");
    }
}

void PullDifferentCards()
{
    do
    {
        if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out Data106kbpsTypeA? cardTypeA, 1000))
        {
            Debug.WriteLine($"ISO 14443 Type A found:");
            Debug.WriteLine($"  ATQA: {cardTypeA.Atqa}");
            Debug.WriteLine($"  SAK: {cardTypeA.Sak}");
            Debug.WriteLine($"  UID: {BitConverter.ToString(cardTypeA.NfcId)}");
        }
        else
        {
            Debug.WriteLine($"{nameof(cardTypeA)} is not configured correctly.");
        }

        if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 1000))
        {
            Debug.WriteLine($"ISO 14443 Type B found:");
            Debug.WriteLine($"  Target number: {card.TargetNumber}");
            Debug.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
            Debug.WriteLine($"  App type: {card.ApplicationType}");
            Debug.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
            Debug.WriteLine($"  Bit rates: {card.BitRates}");
            Debug.WriteLine($"  Cid support: {card.CidSupported}");
            Debug.WriteLine($"  Command: {card.Command}");
            Debug.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
            Debug.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
            Debug.WriteLine($"  Max frame size: {card.MaxFrameSize}");
            Debug.WriteLine($"  Nad support: {card.NadSupported}");
        }
        else
        {
            Debug.WriteLine($"{nameof(card)} is not configured correctly.");
        }

        // Wait a bit to avoid watchdog reset
        Thread.Sleep(500);
    }
    while (true);
}

void PullTypeBCards()
{
    do
    {
        if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 1000))
        {
            Debug.WriteLine($"ISO 14443 Type B found:");
            Debug.WriteLine($"  Target number: {card.TargetNumber}");
            Debug.WriteLine($"  App data: {BitConverter.ToString(card.ApplicationData)}");
            Debug.WriteLine($"  App type: {card.ApplicationType}");
            Debug.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
            Debug.WriteLine($"  Bit rates: {card.BitRates}");
            Debug.WriteLine($"  Cid support: {card.CidSupported}");
            Debug.WriteLine($"  Command: {card.Command}");
            Debug.WriteLine($"  Frame timing: {card.FrameWaitingTime}");
            Debug.WriteLine($"  Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
            Debug.WriteLine($"  Max frame size: {card.MaxFrameSize}");
            Debug.WriteLine($"  Nad support: {card.NadSupported}");
        }
        else
        {
            Debug.WriteLine($"{nameof(card)} is not configured correctly.");
        }

        // Wait a bit
        Thread.Sleep(500);
    }
    while (true);
}

void PullIso15693Cards()
{
    do
    {
        if (pn5180.ListenToCardIso15693(
            TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
            ReceiverRadioFrequencyConfiguration.Iso15693_26,
            out ArrayList cards,
            1000))
        {
            Debug.WriteLine($"ISO 15693 card(s) found: {cards.Count}");
            foreach (Data26_53kbps card in cards)
            {
                Debug.WriteLine($"  Slot: {card.TargetNumber}");
                Debug.WriteLine($"  UID: {BitConverter.ToString(card.NfcId)}");
                Debug.WriteLine($"  DSFID: 0x{card.Dsfid:X2}");
            }
        }
        else
        {
            Debug.WriteLine("No ISO 15693 card detected.");
        }

        Thread.Sleep(500);
    }
    while (true);
}

void ProcessIcodeCard()
{
    string lastUid = string.Empty;
    int scanCount = 0;

    do
    {
        scanCount++;
        // Detect an ISO 15693 card
        ArrayList detectedCards;
        Data26_53kbps detectedCard;

        Debug.WriteLine($"--- Scan #{scanCount} ---");
        if (!pn5180.ListenToCardIso15693(
            TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
            ReceiverRadioFrequencyConfiguration.Iso15693_26,
            out detectedCards,
            2000))
        {
            Debug.WriteLine("Waiting for ISO 15693 card...");
            continue;
        }

        Debug.WriteLine($"  Detected {detectedCards.Count} card(s)");
        detectedCard = (Data26_53kbps)detectedCards[0];
        string currentUid = BitConverter.ToString(detectedCard.NfcId);
        Debug.WriteLine($"ISO 15693 card detected:");
        Debug.WriteLine($"  UID: {currentUid}");
        Debug.WriteLine($"  DSFID: 0x{detectedCard.Dsfid:X2}");
        Debug.WriteLine($"  Slot: {detectedCard.TargetNumber}");

        if (lastUid != string.Empty && lastUid != currentUid)
        {
            Debug.WriteLine($"  *** Card changed! Previous UID: {lastUid} ***");
        }

        lastUid = currentUid;

        // Reset the RF configuration for addressed-mode communication
        pn5180.ResetPN5180Configuration(
            TransmitterRadioFrequencyConfiguration.Iso15693_ASK100_26,
            ReceiverRadioFrequencyConfiguration.Iso15693_26);

        // Create IcodeCard instance
        var icodeCard = new IcodeCard(pn5180, detectedCard.TargetNumber)
        {
            Uid = detectedCard.NfcId,
            Capacity = IcodeCardCapacity.Unknown
        };

        // Get system information
        Debug.WriteLine("Getting system information...");
        var sysRet = icodeCard.GetSystemInformation();
        if (sysRet && icodeCard.Data != null && icodeCard.Data.Length > 0)
        {
            Debug.WriteLine($"  DSFID: 0x{icodeCard.Dsfid:X2}");
            Debug.WriteLine($"  AFI: 0x{icodeCard.Afi:X2}");
            Debug.WriteLine($"  System info data: {BitConverter.ToString(icodeCard.Data)}");
        }
        else
        {
            Debug.WriteLine("Error getting system information.");
        }

        // Parse memory capacity from system info response.
        // Response: flags(1) + info_flags(1) + UID(8) + DSFID(1) + AFI(1) + numBlocks(1, 0-based) + blockSize(1, 0-based) + IC_ref(1)
        // Data already has flags stripped by RunIcodeCardCommand trimming, so raw offsets apply on the full response.
        // After trimming, Data starts at the first byte received: flags byte.
        // Full response layout (from VICC): [0]=flags, [1]=info_flags, [2..9]=UID, [10]=DSFID, [11]=AFI, [12]=numBlocks(0-based), [13]=blockSize(0-based), [14]=IC_ref
        int totalBlocks = 28;  // default for ICODE SLIX (28 blocks x 4 bytes = 112 bytes = 896 bits)
        int blockSize = 4;

        if (icodeCard.Data != null && icodeCard.Data.Length >= 14)
        {
            // numBlocks is 0-based in the response: value 0x1B means 28 blocks
            totalBlocks = icodeCard.Data[12] + 1;
            blockSize = (icodeCard.Data[13] & 0x1F) + 1;
            Debug.WriteLine($"  Memory: {totalBlocks} blocks x {blockSize} bytes = {totalBlocks * blockSize} bytes ({totalBlocks * blockSize * 8} bits)");
        }
        else
        {
            Debug.WriteLine($"  Could not parse memory info, using default: {totalBlocks} blocks x {blockSize} bytes");
        }

        // Dump full memory
        Debug.WriteLine($"--- Full memory dump ({totalBlocks} blocks) ---");

        // Read in chunks to stay within transceiver limits.
        // ReadMultipleBlocks count parameter is the actual number of blocks to read.
        const int maxBlocksPerRead = 16;
        for (int startBlock = 0; startBlock < totalBlocks; startBlock += maxBlocksPerRead)
        {
            int remaining = totalBlocks - startBlock;
            int chunkSize = remaining > maxBlocksPerRead ? maxBlocksPerRead : remaining;

            sysRet = icodeCard.ReadMultipleBlocks((byte)startBlock, (byte)chunkSize);
            if (sysRet && icodeCard.Data != null && icodeCard.Data.Length > 0)
            {
                // Skip the first byte (flags) if present, then print block-by-block
                // Data from ReadMultipleBlocks: flags(1) + blockData(chunkSize * blockSize)
                int dataOffset = 0;
                byte[] blockData = icodeCard.Data;

                // Check if first byte is a flags byte (0x00 = no error)
                if (blockData.Length > chunkSize * blockSize)
                {
                    dataOffset = 1;
                }

                for (int b = 0; b < chunkSize; b++)
                {
                    int offset = dataOffset + (b * blockSize);
                    if (offset + blockSize <= blockData.Length)
                    {
                        byte[] oneBlock = new byte[blockSize];
                        Array.Copy(blockData, offset, oneBlock, 0, blockSize);

                        // Build ASCII representation
                        string ascii = string.Empty;
                        for (int c = 0; c < oneBlock.Length; c++)
                        {
                            ascii += (oneBlock[c] >= 0x20 && oneBlock[c] <= 0x7E)
                                ? (char)oneBlock[c]
                                : '.';
                        }

                        Debug.WriteLine($"  Block {(startBlock + b):D3}: {BitConverter.ToString(oneBlock)}  |{ascii}|");
                    }
                }
            }
            else
            {
                Debug.WriteLine($"  Error reading blocks {startBlock}-{startBlock + chunkSize - 1}");
                break;
            }
        }

        Debug.WriteLine("--- End of memory dump ---");

        // Turn off the RF field to power-reset the card before the next inventory
        pn5180.RadioFrequencyField = false;
        Thread.Sleep(500);
    }
    while (true);
}

void ProcessUltralight()
{
    Data106kbpsTypeA? card;
    do
    {
        if (pn5180.ListenToCardIso14443TypeA(TransmitterRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, ReceiverRadioFrequencyConfiguration.Iso14443A_Nfc_PI_106_106, out card, 20000))
        {
            Debug.WriteLine($"ATQA: {card.Atqa}");
            Debug.WriteLine($"SAK: {card.Sak}");
            Debug.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
            break;
        }
        else
        {
            Debug.WriteLine("Error polling the card.");
        }
    }
    while (true);

    var ultralight = new UltralightCard(pn5180!, 0);
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
        var ret = ultralight.RunUltralightCommand();
        if (ret > 0)
        {
            Debug.Write($"  Block: {ultralight.BlockNumber:X2} - ");
            for (int i = 0; i < 4; i++)
            {
                Debug.Write($"{ultralight.Data[i]:X2} ");
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
    var res = ultralight.TryGetConfiguration(out Iot.Device.Card.Ultralight.Configuration configuration);
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
