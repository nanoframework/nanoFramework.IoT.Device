// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Iot.Device.Card;
using Iot.Device.Card.Mifare;
using Iot.Device.Card.Ultralight;
using Iot.Device.Ndef;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;
using nanoFramework.Logging.Debug;
using Sample;

Debug.WriteLine("Welcome to Pn532 example.");

// Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
// LogDispatcher.LoggerFactory = new DebugLoggerFactory();

// Uncomment for SPI
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the SPI GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.SPI1_MOSI);
//Configuration.SetPinFunction(22, DeviceFunction.SPI1_MISO);
//Configuration.SetPinFunction(23, DeviceFunction.SPI1_CLOCK);
// Make sure as well you are using the right chip select
// Adjust the GPIO you want for chip select
////int pinSelect = 22;
////Pn532 pn532 = new Pn532(SpiDevice.Create(new SpiConnectionSettings(0) { DataFlow = DataFlow.LsbFirst, Mode = SpiMode.Mode0 }), pinSelect);

// Uncomment for I2C
//////////////////////////////////////////////////////////////////////
// when connecting to an ESP32 device, need to configure the I2C GPIOs
// used for the bus
//Configuration.SetPinFunction(21, DeviceFunction.I2C1_DATA);
//Configuration.SetPinFunction(22, DeviceFunction.I2C1_CLOCK);
////Pn532 pn532 = new Pn532(I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress)));

// uncomment for Serial
nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(19, nanoFramework.Hardware.Esp32.DeviceFunction.COM2_TX);
nanoFramework.Hardware.Esp32.Configuration.SetPinFunction(21, nanoFramework.Hardware.Esp32.DeviceFunction.COM2_RX);
Pn532 pn532 = new Pn532("COM2");

if (pn532.FirmwareVersion is FirmwareVersion version)
{
    Debug.WriteLine(
        $"Is it a PN532!: {version.IsPn532}, Version: {version.Version}, Version supported: {version.VersionSupported}");
    // To adjust the baud rate, uncomment the next line
    // pn532.SetSerialBaudRate(BaudRate.B0921600);

    // To dump all the registers, uncomment the next line
    // DumpAllRegisters(pn532);

    // To run tests, uncomment the next line
    // RunTests(pn532);
    ProcessUltralight(pn532);
    // ReadMiFare(pn532);
    // TestGPIO(pn532);

    // To read Credit Cards, uncomment the next line
    // ReadCreditCard(pn532);
}
else
{
    Debug.WriteLine($"Error");
}

pn532.Dispose();

void DumpAllRegisters(Pn532 pn532)
{
    const int MaxRead = 16;
    Span<byte> span = new byte[MaxRead];
    for (int i = 0; i < 0xFFFF; i += MaxRead)
    {
        ushort[] reg = new ushort[MaxRead];
        for (int j = 0; j < MaxRead; j++)
        {
            reg[j] = (ushort)(i + j);
        }

        var ret = pn532.ReadRegister(reg, span);
        if (ret)
        {
            Debug.Write($"Reg: {(i).ToString("X4")} ");
            for (int j = 0; j < MaxRead; j++)
            {
                Debug.Write($"{span[j].ToString("X2")} ");
            }

            Debug.WriteLine("");
        }
    }
}

void ReadMiFare(Pn532 pn532)
{
    byte[] retData = null;
    while (true)
    {
        retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
        if (retData is object)
        {
            break;
        }

        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData is null)
    {
        return;
    }

    for (int i = 0; i < retData.Length; i++)
    {
        Debug.Write($"{retData[i]:X} ");
    }

    Debug.WriteLine("");

    var decrypted = pn532.TryDecode106kbpsTypeA(new Span<byte>(retData, 1, retData.Length - 1));
    if (decrypted is object)
    {
        Debug.WriteLine(
            $"Tg: {decrypted.TargetNumber}, ATQA: {decrypted.Atqa} SAK: {decrypted.Sak}, NFCID: {BitConverter.ToString(decrypted.NfcId)}");
        if (decrypted.Ats is object)
        {
            Debug.WriteLine($", ATS: {BitConverter.ToString(decrypted.Ats)}");
        }

        MifareCard mifareCard = new(pn532, decrypted.TargetNumber)
        {
            BlockNumber = 0,
            Command = MifareCardCommand.AuthenticationA
        };

        mifareCard.SetCapacity(decrypted.Atqa, decrypted.Sak);
        mifareCard.SerialNumber = decrypted.NfcId;
        mifareCard.KeyA = new byte[6] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        mifareCard.KeyB = new byte[6] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
        for (byte block = 0; block < 64; block++)
        {
            mifareCard.BlockNumber = block;
            mifareCard.Command = MifareCardCommand.AuthenticationB;
            var ret = mifareCard.RunMifareCardCommand();
            // This will reselect the card in case of issue
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
}

void TestGPIO(Pn532 pn532)
{
    Debug.WriteLine("Turning Off Port 7!");
    var ret = pn532.WriteGpio((Port7)0);

    // Access GPIO
    ret = pn532.ReadGpio(out Port3 p3, out Port7 p7, out OperatingMode l0L1);
    Debug.WriteLine($"P7: {p7}");
    Debug.WriteLine($"P3: {p3}");
    Debug.WriteLine($"L0L1: {l0L1} ");

    var on = true;
    for (var i = 0; i < 10; i++)
    {
        if (on)
        {
            p7 = Port7.P71;
        }
        else
        {
            p7 = 0;
        }

        ret = pn532.WriteGpio(p7);
        Thread.Sleep(150);
        on = !on;
    }
}

void RunTests(Pn532 pn532)
{
    Debug.WriteLine(
        $"{DiagnoseMode.CommunicationLineTest}: {pn532.RunSelfTest(DiagnoseMode.CommunicationLineTest)}");
    Debug.WriteLine($"{DiagnoseMode.ROMTest}: {pn532.RunSelfTest(DiagnoseMode.ROMTest)}");
    Debug.WriteLine($"{DiagnoseMode.RAMTest}: {pn532.RunSelfTest(DiagnoseMode.RAMTest)}");
    // Check couple of SFR registers
    SfrRegister[] regs = new SfrRegister[]
    {
        SfrRegister.HSU_CNT, SfrRegister.HSU_CTR, SfrRegister.HSU_PRE, SfrRegister.HSU_STA
    };
    Span<byte> redSfrs = new byte[regs.Length];
    var ret = pn532.ReadRegisterSfr(regs, redSfrs);
    for (int i = 0; i < regs.Length; i++)
    {
        Debug.WriteLine(
            $"Readregisters: {regs[i]}, value: {BitConverter.ToString(redSfrs.ToArray(), i, 1)} ");
    }

    // This should give the same result as
    ushort[] regus = new ushort[] { 0xFFAE, 0xFFAC, 0xFFAD, 0xFFAB };
    Span<byte> redSfrus = new byte[regus.Length];
    ret = pn532.ReadRegister(regus, redSfrus);
    for (int i = 0; i < regus.Length; i++)
    {
        Debug.WriteLine(
            $"Readregisters: {regus[i]}, value: {BitConverter.ToString(redSfrus.ToArray(), i, 1)} ");
    }

    Debug.WriteLine($"Are results same: {redSfrus.SequenceEqual(redSfrs)}");
    // Access GPIO
    ret = pn532.ReadGpio(out Port3 p3, out Port7 p7, out OperatingMode l0L1);
    Debug.WriteLine($"P7: {p7}");
    Debug.WriteLine($"P3: {p3}");
    Debug.WriteLine($"L0L1: {l0L1} ");
}

void ProcessUltralight(Pn532 pn532)
{
    byte[]? retData = null;
    while (true)
    {
        retData = pn532.ListPassiveTarget(MaxTarget.One, TargetBaudRate.B106kbpsTypeA);
        if (retData is object)
        {
            break;
        }

        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData is null)
    {
        return;
    }

    for (int i = 0; i < retData.Length; i++)
    {
        Debug.Write($"{retData[i]:X2} ");
    }

    Debug.WriteLine("");

    var card = pn532.TryDecode106kbpsTypeA(new Span<byte>(retData, 1, retData.Length - 1));
    if (card is not object)
    {
        Debug.WriteLine("Not a valid card, please try again.");
        return;
    }

    var ultralight = new UltralightCard(pn532!, card.TargetNumber);
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
    if (sign != null)
    {
        Debug.WriteLine("Signature: ");
        for (int i = 0; i < sign.Length; i++)
        {
            Debug.Write($"{sign[i]:X2} ");
        }

        Debug.WriteLine("");
    }

    // The ReadFast feature can be used as well, note that the PN532 has a limited buffer out of 262 bytes
    // So maximum 64 pages can be read as once.
    Debug.WriteLine("Fast read example:");
    //var buff = ultralight.ReadFast(0, (byte)(ultralight.NumberBlocks > 64 ? 64 : ultralight.NumberBlocks - 1));
    //if (buff != null)
    //{
    //    for (int i = 0; i < buff.Length / 4; i++)
    //    {
    //        Debug.WriteLine($"  Block {i} - {buff[i * 4]:X2} {buff[i * 4 + 1]:X2} {buff[i * 4 + 2]:X2} {buff[i * 4 + 3]:X2}");
    //    }
    //}

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
    var res = ultralight.TryGetConfiguration(out Configuration configuration);
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
    newMessage.Records.Add(new TextRecord("I ❤ .NET nanoFramework", "en", Encoding.UTF8));
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
