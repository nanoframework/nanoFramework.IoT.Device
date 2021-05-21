// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Device.I2c;
using System.Device.Spi;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iot.Device.Card;
using Iot.Device.Card.CreditCardProcessing;
using Iot.Device.Card.Mifare;
using Iot.Device.Common;
using Iot.Device.Pn532;
using Iot.Device.Pn532.ListPassive;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

Pn532 pn532;

Debug.WriteLine("Welcome to Pn532 example.");
Debug.WriteLine("Which interface do you want to use with your Pn532?");
Debug.WriteLine("1. HSU: Hight Speed UART (high speed serial port)");
Debug.WriteLine("2. I2C");
Debug.WriteLine("3. SPI");
var choiceInterface = Console.ReadKey();
Debug.WriteLine();
if (choiceInterface is not { KeyChar: '1' or '2' or '3' })
{
    Debug.WriteLine($"You can only select 1, 2 or 3");
    return;
}

Debug.WriteLine("Do you want log level to Debug? Y/N");
var debugLevelConsole = Console.ReadKey();
Debug.WriteLine();
LogLevel debugLevel = debugLevelConsole is { KeyChar: 'Y' or 'y' } ? LogLevel.Debug : LogLevel.Information;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddFilter(x => x >= debugLevel);
    builder.AddConsole();
});

// Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
LogDispatcher.LoggerFactory = loggerFactory;

if (choiceInterface is { KeyChar: '3' })
{
    Debug.WriteLine("Which pin number do you want as Chip Select?");
    var pinSelectConsole = Console.ReadLine();
    int pinSelect;
    try
    {
        pinSelect = Convert.ToInt32(pinSelectConsole);
    }
    catch (Exception ex) when (ex is FormatException || ex is OverflowException)
    {
        Debug.WriteLine("Impossible to convert the pin number.");
        return;
    }

    pn532 = new Pn532(SpiDevice.Create(new SpiConnectionSettings(0) { DataFlow = DataFlow.LsbFirst, Mode = SpiMode.Mode0 }), pinSelect);
}
else if (choiceInterface is { KeyChar: '2' })
{
    pn532 = new Pn532(I2cDevice.Create(new I2cConnectionSettings(1, Pn532.I2cDefaultAddress)));
}
else
{
    Debug.WriteLine("Please enter the serial port to use. ex: COM3 on Windows or /dev/ttyS0 on Linux");

    var device = Console.ReadLine();
    pn532 = new Pn532(device!);
}

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
    ReadMiFare(pn532);
    // TestGPIO(pn532);

    // To read Credit Cards, uncomment the next line
    // ReadCreditCard(pn532);
}
else
{
    Debug.WriteLine($"Error");
}

pn532?.Dispose();

void DumpAllRegisters(Pn532 pn532)
{
    const int MaxRead = 16;
    SpanByte span = new byte[MaxRead];
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
            Console.Write($"Reg: {(i).ToString("X4")} ");
            for (int j = 0; j < MaxRead; j++)
            {
                Console.Write($"{span[j].ToString("X2")} ");
            }

            Debug.WriteLine();
        }
    }
}

void ReadMiFare(Pn532 pn532)
{
    byte[]? retData = null;
    while ((!Console.KeyAvailable))
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
        Console.Write($"{retData[i]:X} ");
    }

    Debug.WriteLine();

    var decrypted = pn532.TryDecode106kbpsTypeA(retData.AsSpan().Slice(1));
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
        Task.Delay(150).Wait();
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
    SpanByte redSfrs = new byte[regs.Length];
    var ret = pn532.ReadRegisterSfr(regs, redSfrs);
    for (int i = 0; i < regs.Length; i++)
    {
        Debug.WriteLine(
            $"Readregisters: {regs[i]}, value: {BitConverter.ToString(redSfrs.ToArray(), i, 1)} ");
    }

    // This should give the same result as
    ushort[] regus = new ushort[] { 0xFFAE, 0xFFAC, 0xFFAD, 0xFFAB };
    SpanByte redSfrus = new byte[regus.Length];
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

void ReadCreditCard(Pn532 pn532)
{
    byte[]? retData = null;
    while ((!Console.KeyAvailable))
    {
        retData = pn532.AutoPoll(5, 300, new PollingType[] { PollingType.Passive106kbpsISO144443_4B });
        if (retData is object)
        {
            if (retData.Length >= 3)
            {
                break;
            }
        }

        // Give time to PN532 to process
        Thread.Sleep(200);
    }

    if (retData is null)
    {
        return;
    }

    // Check how many tags and the type
    Debug.WriteLine($"Num tags: {retData[0]}, Type: {(PollingType)retData[1]}");
    var decrypted = pn532.TryDecodeData106kbpsTypeB(retData.AsSpan().Slice(3));
    if (decrypted is object)
    {
        Debug.WriteLine(
            $"{decrypted.TargetNumber}, Serial: {BitConverter.ToString(decrypted.NfcId)}, App Data: {BitConverter.ToString(decrypted.ApplicationData)}, " +
            $"{decrypted.ApplicationType}, Bit Rates: {decrypted.BitRates}, CID {decrypted.CidSupported}, Command: {decrypted.Command}, FWT: {decrypted.FrameWaitingTime}, " +
            $"ISO144443 compliance: {decrypted.ISO14443_4Compliance}, Max Frame size: {decrypted.MaxFrameSize}, NAD: {decrypted.NadSupported}");

        CreditCard creditCard = new CreditCard(pn532, decrypted.TargetNumber);
        creditCard.ReadCreditCardInformation();

        Debug.WriteLine("All Tags for the Credit Card:");
        DisplayTags(creditCard.Tags, 0);
    }
}

string AddSpace(int level)
{
    string space = string.Empty;
    for (int i = 0; i < level; i++)
    {
        space += "  ";
    }

    return space;
}

void DisplayTags(ListTag tagToDisplay, int levels)
{
    foreach (var tagparent in tagToDisplay)
    {
        Console.Write(AddSpace(levels) +
                        $"{tagparent.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == tagparent.TagNumber).FirstOrDefault()?.Description}");
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
                Console.Write(AddSpace(levels + 1) +
                                $"{dt.TagNumber.ToString("X4")}-{TagList.Tags.Where(m => m.TagNumber == dt.TagNumber).FirstOrDefault()?.Description}");
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
