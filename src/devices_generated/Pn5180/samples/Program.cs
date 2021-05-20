// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Mifare;
using Iot.Device.Ft4222;
using Iot.Device.Pn5180;
using Iot.Device.Rfid;

Pn5180 pn5180;

Debug.WriteLine("Hello Pn5180!");
Debug.WriteLine($"Choose the device you want to use");
Debug.WriteLine($"1 for hardware Spi like on a Raspberry Pi");
Debug.WriteLine($"2 for FT4222");
char choice = Console.ReadKey().KeyChar;
Debug.WriteLine();
Debug.WriteLine();
if (choice == '1')
{
    pn5180 = HardwareSpi();
}
else if (choice == '2')
{
    pn5180 = Ft4222();
}
else
{
    Debug.WriteLine($"Not a correct choice, please choose the device you want to use");
    return;
}

var (product, firmware, eeprom) = pn5180.GetVersions();
Debug.WriteLine($"Product: {product}, Firmware: {firmware}, EEPROM: {eeprom}");

Debug.WriteLine($"Choose what you want to test");
Debug.WriteLine($"1 dump a full credit card ISO 14443 type B");
Debug.WriteLine($"2 dump a Mifare IS 14443 type A");
Debug.WriteLine($"3 EEPROM operations");
Debug.WriteLine($"4 Radio Frequency operations");
Debug.WriteLine($"5 Pull ISO 14443 Type A and B cards, display information");
Debug.WriteLine($"6 Pull ISO 14443 B cards, display information");
choice = Console.ReadKey().KeyChar;
Debug.WriteLine();
Debug.WriteLine();

if (choice == '1')
{
    TypeB();
}
else if (choice == '2')
{
    TypeA();
}
else if (choice == '3')
{
    Eeprom();
}
else if (choice == '4')
{
    RfConfiguration();
}
else if (choice == '5')
{
    PullDifferentCards();
}
else if (choice == '6')
{
    PullTypeBCards();
}
else
{
    Debug.WriteLine($"Not a valid choice, please choose the test you want to run");
}

Pn5180 HardwareSpi()
{
    SpiDevice spi = SpiDevice.Create(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    // Reset the device
    using GpioController gpioController = new();
    gpioController.OpenPin(4, PinMode.Output);
    gpioController.Write(4, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(4, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(spi, 2, 3, null, true);
}

Pn5180 Ft4222()
{
    List<FtDevice> devices = FtCommon.GetDevices();
    Debug.WriteLine($"{devices.Count} FT4222 elements found");
    foreach (var device in devices)
    {
        Debug.WriteLine($"  Description: {device.Description}");
        Debug.WriteLine($"  Flags: {device.Flags}");
        Debug.WriteLine($"  Id: {device.Id}");
        Debug.WriteLine($"  Location Id: {device.LocId}");
        Debug.WriteLine($"  Serial Number: {device.SerialNumber}");
        Debug.WriteLine($"  Device type: {device.Type}");
        Debug.WriteLine();
    }

    var (chip, dll) = FtCommon.GetVersions();
    Debug.WriteLine($"Chip version: {chip}");
    Debug.WriteLine($"Dll version: {dll}");

    Ft4222Spi ftSpi = new Ft4222Spi(new SpiConnectionSettings(0, 1) { ClockFrequency = Pn5180.MaximumSpiClockFrequency, Mode = Pn5180.DefaultSpiMode, DataFlow = DataFlow.MsbFirst });

    GpioController gpioController = new(PinNumberingScheme.Board, new Ft4222Gpio());

    // Reset the device
    gpioController.OpenPin(0, PinMode.Output);
    gpioController.Write(0, PinValue.Low);
    Thread.Sleep(10);
    gpioController.Write(0, PinValue.High);
    Thread.Sleep(10);

    return new Pn5180(ftSpi, 2, 3, gpioController, true);
}

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
    var ret = pn5180.RetrieveRadioFrequencyConfiguration(TransmitterRadioFrequencyConfiguration.Iso14443B_106, configBuff);
    for (int i = 0; i < sizeConfig; i++)
    {
        Debug.WriteLine($"Register: {configBuff[Pn5180.RadioFrequencyConfigurationSize * i]}, Data: {BitConverter.ToString(configBuff.Slice(Pn5180.RadioFrequencyConfigurationSize * i + 1, Pn5180.RadioFrequencyConfigurationSize - 1).ToArray())}");
    }
}

void TypeB()
{
    Debug.WriteLine();
    // Poll the data for 20 seconds
    if (pn5180.ListenToCardIso14443TypeB(TransmitterRadioFrequencyConfiguration.Iso14443B_106, ReceiverRadioFrequencyConfiguration.Iso14443B_106, out Data106kbpsTypeB? card, 20000))
    {
        Debug.WriteLine($"Target number: {card.TargetNumber}");
        Debug.WriteLine($"App data: {BitConverter.ToString(card.ApplicationData)}");
        Debug.WriteLine($"App type: {card.ApplicationType}");
        Debug.WriteLine($"UID: {BitConverter.ToString(card.NfcId)}");
        Debug.WriteLine($"Bit rates: {card.BitRates}");
        Debug.WriteLine($"Cid support: {card.CidSupported}");
        Debug.WriteLine($"Command: {card.Command}");
        Debug.WriteLine($"Frame timing: {card.FrameWaitingTime}");
        Debug.WriteLine($"Iso 14443-4 compliance: {card.ISO14443_4Compliance}");
        Debug.WriteLine($"Max frame size: {card.MaxFrameSize}");
        Debug.WriteLine($"Nad support: {card.NadSupported}");

        var creditCard = new CreditCard(pn5180, card.TargetNumber, 2);
        ReadAndDisplayData(creditCard);

        // Halt card
        if (pn5180.DeselectCardTypeB(card))
        {
            Debug.WriteLine($"Card unselected properly");
        }
        else
        {
            Debug.WriteLine($"ERROR: Card can't be unselected");
        }
    }
    else
    {
        Debug.WriteLine($"{nameof(card)} card cannot be read");
    }
}

void TypeA()
{
    Debug.WriteLine();
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
    }
    while (!Console.KeyAvailable);
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
    while (!Console.KeyAvailable);
}

void ReadAndDisplayData(CreditCard creditCard)
{
    creditCard.ReadCreditCardInformation();
    DisplayTags(creditCard.Tags, 0);
    // Display Log Entries
    var format = Tag.SearchTag(creditCard.Tags, 0x9F4F).FirstOrDefault();
    if (format is object)
    {
        DisplayLogEntries(creditCard.LogEntries, format.Tags);
    }
}

string AddSpace(int level)
{
    string space = String.Empty;
    for (int i = 0; i < level; i++)
    {
        space += "  ";
    }

    return space;
}

void DisplayTags(List<Tag> tagToDisplay, int levels)
{
    foreach (var tagparent in tagToDisplay)
    {
        Console.Write(AddSpace(levels) + $"{tagparent.TagNumber.ToString(tagparent.TagNumber > 0xFFFF ? "X8" : "X4")}-{TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault()?.Description}");
        var isTemplate = TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault();
        if ((isTemplate?.IsTemplate == true) || (isTemplate?.IsConstructed == true))
        {
            Debug.WriteLine();
            DisplayTags(tagparent.Tags, levels + 1);
        }
        else if (isTemplate?.IsDol == true)
        {
            // In this case, all the data inside are 1 byte only
            Debug.WriteLine(", Data Object Length elements:");
            foreach (var dt in tagparent.Tags)
            {
                Console.Write(AddSpace(levels + 1) + $"{dt.TagNumber.ToString(dt.TagNumber > 0xFFFF ? "X8" : "X4")}-{TagList.Tags.Where(m => m.TagNumber == dt.TagNumber).FirstOrDefault()?.Description}");
                Debug.WriteLine($", data length: {dt.Data[0]}");
            }
        }
        else
        {
            TagDetails tg = new TagDetails(tagparent);
            Debug.WriteLine($": {tg.ToString()}");
        }
    }
}

void DisplayLogEntries(List<byte[]> entries, List<Tag> format)
{
    for (int i = 0; i < format.Count; i++)
    {
        Console.Write($"{TagList.Tags.Where(m => m.TagNumber == format[i].TagNumber).FirstOrDefault()?.Description} | ");
    }

    Debug.WriteLine();

    foreach (var entry in entries)
    {
        int index = 0;
        for (int i = 0; i < format.Count; i++)
        {
            var data = entry.AsSpan().Slice(index, format[i].Data[0]);
            var tg = new TagDetails(new Tag() { TagNumber = format[i].TagNumber, Data = data.ToArray() });
            Console.Write($"{tg.ToString()} | ");
            index += format[i].Data[0];
        }

        Debug.WriteLine();
    }
}
