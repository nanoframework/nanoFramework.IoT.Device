// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Device.I2c;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Iot.Device.Ssd13xx;
using Iot.Device.Ssd13xx.Commands;
using Iot.Device.Ssd13xx.Samples;
using Ssd1306Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1306Commands;
using Ssd1327Cmnds = Iot.Device.Ssd13xx.Commands.Ssd1327Commands;

Debug.WriteLine("Hello Ssd1306 Sample!");

#if SSD1327
using Ssd1327 device = GetSsd1327WithI2c();
InitializeSsd1327(device);
ClearScreenSsd1327(device);
//SendMessage(device, "Hello .NET IoT!");
SendMessage(device, DisplayIpAddress());
#else
using Ssd1306 device = GetSsd1306WithI2c();
InitializeSsd1306(device);
ClearScreenSsd1306(device);
// SendMessage(device, "Hello .NET IoT!!!");
// SendMessage(device, DisplayIpAddress());
DisplayImages(device);
DisplayClock(device);
ClearScreenSsd1306(device);
#endif

I2cDevice GetI2CDevice()
{
    Debug.WriteLine("Using I2C protocol");

    I2cConnectionSettings connectionSettings = new(1, 0x3C);
    return I2cDevice.Create(connectionSettings);
}

Ssd1327 GetSsd1327WithI2c()
{
    return new Ssd1327(GetI2CDevice());
}

Ssd1306 GetSsd1306WithI2c()
{
    return new Ssd1306(GetI2CDevice());
}

// Display size 128x32.
void InitializeSsd1306(Ssd1306 device)
{
    device.SendCommand(new SetDisplayOff());
    device.SendCommand(new Ssd1306Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x00, 0x08));
    device.SendCommand(new SetMultiplexRatio(0x1F));
    device.SendCommand(new Ssd1306Cmnds.SetDisplayOffset(0x00));
    device.SendCommand(new Ssd1306Cmnds.SetDisplayStartLine(0x00));
    device.SendCommand(new Ssd1306Cmnds.SetChargePump(true));
    device.SendCommand(
        new Ssd1306Cmnds.SetMemoryAddressingMode(Ssd1306Cmnds.SetMemoryAddressingMode.AddressingMode
            .Horizontal));
    device.SendCommand(new Ssd1306Cmnds.SetSegmentReMap(true));
    device.SendCommand(new Ssd1306Cmnds.SetComOutputScanDirection(false));
    device.SendCommand(new Ssd1306Cmnds.SetComPinsHardwareConfiguration(false, false));
    device.SendCommand(new SetContrastControlForBank0(0x8F));
    device.SendCommand(new Ssd1306Cmnds.SetPreChargePeriod(0x01, 0x0F));
    device.SendCommand(
        new Ssd1306Cmnds.SetVcomhDeselectLevel(Ssd1306Cmnds.SetVcomhDeselectLevel.DeselectLevel.Vcc1_00));
    device.SendCommand(new Ssd1306Cmnds.EntireDisplayOn(false));
    device.SendCommand(new Ssd1306Cmnds.SetNormalDisplay());
    device.SendCommand(new SetDisplayOn());
    device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
    device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page1,
        Ssd1306Cmnds.PageAddress.Page3));
}

// Display size 96x96.
void InitializeSsd1327(Ssd1327 device)
{
    device.SendCommand(new Ssd1327Cmnds.SetUnlockDriver(true));
    device.SendCommand(new SetDisplayOff());
    device.SendCommand(new SetMultiplexRatio(0x5F));
    device.SendCommand(new Ssd1327Cmnds.SetDisplayStartLine());
    device.SendCommand(new Ssd1327Cmnds.SetDisplayOffset(0x5F));
    device.SendCommand(new Ssd1327Cmnds.SetReMap());
    device.SendCommand(new Ssd1327Cmnds.SetInternalVddRegulator(true));
    device.SendCommand(new SetContrastControlForBank0(0x53));
    device.SendCommand(new Ssd1327Cmnds.SetPhaseLength(0X51));
    device.SendCommand(new Ssd1327Cmnds.SetDisplayClockDivideRatioOscillatorFrequency(0x01, 0x00));
    device.SendCommand(new Ssd1327Cmnds.SelectDefaultLinearGrayScaleTable());
    device.SendCommand(new Ssd1327Cmnds.SetPreChargeVoltage(0x08));
    device.SendCommand(new Ssd1327Cmnds.SetComDeselectVoltageLevel(0X07));
    device.SendCommand(new Ssd1327Cmnds.SetSecondPreChargePeriod(0x01));
    device.SendCommand(new Ssd1327Cmnds.SetSecondPreChargeVsl(true));
    device.SendCommand(new Ssd1327Cmnds.SetNormalDisplay());
    device.SendCommand(new DeactivateScroll());
    device.SendCommand(new SetDisplayOn());
    device.SendCommand(new Ssd1327Cmnds.SetRowAddress());
    device.SendCommand(new Ssd1327Cmnds.SetColumnAddress());
}

void ClearScreenSsd1306(Ssd1306 device)
{
    device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
    device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
        Ssd1306Cmnds.PageAddress.Page3));

    for (int cnt = 0; cnt < 32; cnt++)
    {
        byte[] data = new byte[16];
        device.SendData(data);
    }
}

void ClearScreenSsd1327(Ssd1327 device)
{
    device.ClearDisplay();
}

void SendMessageSsd1306(Ssd1306 device, string message)
{
    device.SendCommand(new Ssd1306Cmnds.SetColumnAddress());
    device.SendCommand(new Ssd1306Cmnds.SetPageAddress(Ssd1306Cmnds.PageAddress.Page0,
        Ssd1306Cmnds.PageAddress.Page3));

    foreach (char character in message)
    {
        device.SendData(BasicFont.GetCharacterBytes(character));
    }
}

void SendMessageSsd1327(Ssd1327 device, string message)
{
    device.SetRowAddress(0x00, 0x07);

    foreach (char character in message)
    {
        byte[] charBitMap = BasicFont.GetCharacterBytes(character);
        ListByte data = new ListByte();
        for (var i = 0; i < charBitMap.Length; i = i + 2)
        {
            for (var j = 0; j < 8; j++)
            {
                byte cdata = 0x00;
                int bit1 = (byte)((charBitMap[i] >> j) & 0x01);
                cdata |= (bit1 == 1) ? (byte)0xF0 : (byte)0x00;
                var secondBitIndex = i + 1;
                if (secondBitIndex < charBitMap.Length)
                {
                    int bit2 = (byte)((charBitMap[i + 1] >> j) & 0x01);
                    cdata |= (bit2 == 1) ? (byte)0x0F : (byte)0x00;
                }

                data.Add(cdata);
            }
        }

        device.SendData(data.ToArray());
    }
}

string DisplayIpAddress()
{
    string? ipAddress = GetIpAddress();

    if (ipAddress is null)
    {
        return $"IP:{ipAddress}";
    }
    else
    {
        return $"Error: IP Address Not Found";
    }
}

void DisplayImages(Ssd1306 ssd1306)
{
    Debug.WriteLine("Display Images");
    foreach (var image_name in Directory.GetFiles("images", "*.bmp").OrderBy(f => f))
    {
        using Image<L16> image = Image.Load<L16>(image_name);
        ssd1306.DisplayImage(image);
        Thread.Sleep(1000);
    }
}

void DisplayClock(Ssd1306 ssd1306)
{
    Debug.WriteLine("Display clock");
    var fontSize = 25;
    var font = "DejaVu Sans";
    var fontsys = SystemFonts.CreateFont(font, fontSize, FontStyle.Italic);
    var y = 0;

    foreach (var i in Enumerable.Range(0, 100))
    {
        using (Image<Rgba32> image = new Image<Rgba32>(128, 32))
        {
            if (image.TryGetSinglePixelSpan(out SpanRgba32 imageSpan))
            {
                imageSpan.Fill(Color.Black);
            }

            image.Mutate(ctx => ctx
                .DrawText(DateTime.UtcNow.ToString("HH:mm:ss"), fontsys, Color.White,
                    new SixLabors.ImageSharp.PointF(0, y)));

            using (Image<L16> image_t = image.CloneAs<L16>())
            {
                ssd1306.DisplayImage(image_t);
            }

            y++;
            if (y >= image.Height)
            {
                y = 0;
            }

            Thread.Sleep(100);
        }
    }
}

// Referencing https://stackoverflow.com/questions/6803073/get-local-ip-address
string? GetIpAddress()
{
    // Get a list of all network interfaces (usually one per network card, dialup, and VPN connection).
    NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

    foreach (NetworkInterface network in networkInterfaces)
    {
        // Read the IP configuration for each network
        IPInterfaceProperties properties = network.GetIPProperties();

        if (network.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
            network.OperationalStatus == OperationalStatus.Up &&
            !network.Description.ToLower().Contains("virtual") &&
            !network.Description.ToLower().Contains("pseudo"))
        {
            // Each network interface may have multiple IP addresses.
            foreach (IPAddressInformation address in properties.UnicastAddresses)
            {
                // We're only interested in IPv4 addresses for now.
                if (address.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                // Ignore loopback addresses (e.g., 127.0.0.1).
                if (IPAddress.IsLoopback(address.Address))
                {
                    continue;
                }

                return address.Address.ToString();
            }
        }
    }

    return null;
}
