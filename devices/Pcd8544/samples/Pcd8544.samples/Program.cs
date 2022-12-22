//
// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Device.Gpio;
using System.Device.Pwm;
using System.Device.Spi;
using System.Globalization;
using System.Threading;

using Iot.Device.CharacterLcd;

namespace Pcd8544.samples
{
    public class Program
    {
        public static void Main()
        {
            var resetPin = 32;
            var dataCommandPin = 33;
            var backlightPin = 21;

            var gpioController = new GpioController();
            var spiConnectionSettings = new SpiConnectionSettings(1, 5)
            {
                ClockFrequency = 5_000_000,
                Mode = SpiMode.Mode0,
                DataFlow = DataFlow.MsbFirst,
                ChipSelectLineActiveState = PinValue.Low
            };
            var spiDevice = new SpiDevice(spiConnectionSettings);
            var pwmChannel = PwmChannel.CreateFromPin(backlightPin);

            var lcd = new Iot.Device.Pcd8544(dataCommandPin, spiDevice, resetPin, pwmChannel, gpioController, false);
            lcd.Enabled = true;
            lcd.Bias = 6;
            lcd.Contrast = 40;

            BrightnessContrastTemperatureBias(lcd);

            lcd.Bias = 6;
            lcd.Contrast = 40;

            Thread.Sleep(5000);

            DisplayTextChangePositionBlink(lcd);

            Thread.Sleep(5000);

            LcdConsole(lcd);

            Thread.Sleep(5000);

            DisplayLinesPointsRectabngles(lcd);

            Thread.Sleep(5000);

            DisplayBitmap(lcd);

            while (true)
            {
                Thread.Sleep(10000);
            }
        }

        static void BrightnessContrastTemperatureBias(Iot.Device.Pcd8544 lcd)
        {
            lcd.Clear();

            Console.WriteLine("Adjusting brightness from 0 to 1.0");
            for (int i = 0; i < 10; i++)
            {
                lcd.SetCursorPosition(0, 0);
                lcd.WriteLine("Test for brightness");
                lcd.Write($"{i * 10} %");
                lcd.BacklightBrightness = i / 10.0f;
                Thread.Sleep(500);
            }
            lcd.Clear();

            Console.WriteLine("Reseting brightness to 1 (100%)");
            lcd.BacklightBrightness = 1f;

            Console.WriteLine("Adjusting contrast from 0 to 127");
            for (byte i = 0; i < 128; i++)
            {
                lcd.SetCursorPosition(0, 0);
                lcd.WriteLine("Test for contrast");
                lcd.Write($"{i}");
                lcd.Contrast = i;
                Thread.Sleep(100);
            }

            Console.WriteLine("Reseting contrast to recommended value 40");
            lcd.Contrast = 40;
            lcd.Clear();

            Console.WriteLine("Testing the 4 different temperature modes");
            lcd.WriteLine("Test temp 0");
            lcd.Temperature = Iot.Device.Pcd8544Enums.ScreenTemperature.Coefficient0;
            Thread.Sleep(1500);
            lcd.WriteLine("Test temp 1");
            lcd.Temperature = Iot.Device.Pcd8544Enums.ScreenTemperature.Coefficient1;
            Thread.Sleep(1500);
            lcd.WriteLine("Test temp 2");
            lcd.Temperature = Iot.Device.Pcd8544Enums.ScreenTemperature.Coefficient2;
            Thread.Sleep(1500);
            lcd.WriteLine("Test temp 3");
            lcd.Temperature = Iot.Device.Pcd8544Enums.ScreenTemperature.Coefficient3;
            Thread.Sleep(1500);
            lcd.Temperature = Iot.Device.Pcd8544Enums.ScreenTemperature.Coefficient0;
            lcd.Clear();

            Console.WriteLine("Adjusting bias from 0 to 6");
            // Adjusting the bias
            for (byte i = 0; i < 7; i++)
            {
                // Adjusting the bias
                lcd.Bias = i;
                lcd.SetCursorPosition(0, 0);
                lcd.WriteLine("Adjusting bias");
                lcd.Write($"bias = {i}");
                Thread.Sleep(2000);
                lcd.Clear();
            }

            Console.WriteLine("Reseting bias to recommended value 4");
            lcd.Bias = 4;
        }

        static void DisplayTextChangePositionBlink(Iot.Device.Pcd8544 lcd)
        {
            lcd.Clear();

            Console.WriteLine("Display is on and will switch on and off");
            lcd.Write("Display is on and will switch on and off");
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(1000);
                lcd.Enabled = !lcd.Enabled;
            }

            lcd.Enabled = true;
            lcd.Clear();

            Console.WriteLine("Displaying multi line with WriteLine");
            lcd.SetCursorPosition(0, 0);
            lcd.Write("First line");
            lcd.SetCursorPosition(0, 1);
            lcd.Write("Second one");
            lcd.SetCursorPosition(0, 2);
            lcd.Write("3rd");
            lcd.SetCursorPosition(0, 3);
            lcd.Write("Guess!");
            lcd.SetCursorPosition(0, 4);
            lcd.Write("One more...");
            lcd.SetCursorPosition(0, 5);
            lcd.Write("last line");
            Thread.Sleep(1500);

            Console.WriteLine("Inverting the color screen");
            // this will blink the screen
            for (int i = 0; i < 6; i++)
            {
                lcd.InvertedColors = !lcd.InvertedColors;
                Thread.Sleep(1000);
            }

            Console.WriteLine("Activating the cursor, writting numbers in a raw");
            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.UnderlineCursorVisible = true;
            for (int i = 0; i < 50; i++)
            {
                lcd.Write($"{i}");
                Thread.Sleep(500);
            }

            lcd.Clear();
            Console.WriteLine("Testing backspace to remove character");
            lcd.Write("Basckspace");
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(2000);
                lcd.Write("\b");
            }

            Console.WriteLine("Displaying more text and moving the cursor around");
            lcd.Clear();
            lcd.Write("More text");
            Thread.Sleep(1500);
            lcd.SetCursorPosition(0, 0);
            Thread.Sleep(1500);
            lcd.SetCursorPosition(5, 0);
            Thread.Sleep(1500);
            lcd.SetCursorPosition(0, 5);
            Thread.Sleep(1500);
            lcd.UnderlineCursorVisible = false;

            Console.WriteLine("This will display a line of random bits");
            lcd.Clear();
            lcd.WriteLine("This will display a line of random characters");
            Thread.Sleep(1500);
            char[] textToSend = new char[lcd.Size.Height * lcd.Size.Width];
            var rand = new Random(123456);
            for (int i = 0; i < textToSend.Length; i++)
            {
                textToSend[i] = (char)rand.Next(255);
            }

            lcd.Clear();
            lcd.SetCursorPosition(0, 0);
            lcd.Write(textToSend);
            Thread.Sleep(1000);
            lcd.Clear();
        }

        static void DisplayBitmap(Iot.Device.Pcd8544 lcd)
        {
            lcd.Clear();

            //var nokaiHomePage = new byte[] { 0x00, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC0, 0xC0, 0x00,
            //    0x00, 0xC0, 0x00, 0xC0, 0x40, 0x40, 0xC0, 0x00, 0xC0, 0x80, 0x80, 0xC0, 0x40, 0x00, 0xC0, 0x00, 0x00, 0x00, 0xC0,
            //    0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x00, 0x00, 0xF7, 0xF7, 0xF7, 0xF7, 0xF7, 0x07,
            //    0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x1F, 0x01, 0x03, 0x0E, 0x1F, 0x00, 0x1F, 0x10, 0x10, 0x1F, 0x00, 0x1F, 0x07, 0x0C,
            //    0x18, 0x10, 0x00, 0x1F, 0x00, 0x1C, 0x07, 0x05, 0x05, 0x07, 0x1C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
            //    0x00, 0x00, 0xEF, 0xEF, 0xEF, 0xEF, 0x0F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0x00, 0x00, 0xF7, 0xF7, 0xF7, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7E, 0x7E, 0x7E, 0x7E, 0x00, 0x00, 0xBD,
            //    0xBD, 0x81, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x80, 0xDF, 0xDF, 0x9F, 0x00, 0x00, 0x00, 0x01, 0x0F, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x7C, 0x0C, 0x18, 0x0C, 0x7C, 0x00, 0x7C, 0x54, 0x54, 0x44, 0x00, 0x7C, 0x08, 0x10, 0x7C, 0x00, 0x7C, 0x40, 0x40,
            //    0x7C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            //    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F, 0x08, 0x08, 0x0F, 0x00 };


            //lcd.SetByteMap(nokaiHomePage);
            //lcd.Draw();

            var nanoFramewokrLogo = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80,
                0x80, 0x80, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0,
                0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0,
                0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0xC0, 0x80, 0x80, 0x80, 0x80,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x1F, 0x0F, 0x0F, 0x07, 0x87,
                0xC7, 0xC7, 0xC7, 0xC7, 0xC7, 0x87, 0x07, 0x0F, 0x0F, 0x1F, 0x1F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F, 0x3F,
                0x3F, 0x3F, 0x3F, 0x3F, 0x1F, 0x0F, 0x0F, 0x07, 0x07, 0x87, 0xC3, 0xC3, 0xC3, 0xC3, 0xC3, 0xC7, 0x07, 0x07, 0x0F,
                0x0F, 0x1F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFC, 0xF8, 0xF0, 0xF0, 0xE0, 0xC1, 0x81, 0x03, 0x03, 0x03, 0x03, 0x01, 0xE0, 0xF0, 0xF0, 0xF8, 0xF8,
                0xFC, 0xFC, 0xFC, 0xFC, 0xFE, 0xFE, 0xFE, 0xFE, 0xFE, 0xFC, 0xFC, 0xFC, 0xFC, 0xF8, 0xF0, 0xF0, 0xF0, 0xE0, 0xE1,
                0xE1, 0xE1, 0xE1, 0xE1, 0xE1, 0xF0, 0xF0, 0xF0, 0xF8, 0xFC, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x3F, 0x1F, 0x0F, 0x0F, 0x07, 0x01, 0x80, 0x80,
                0x80, 0x80, 0x80, 0x07, 0x0F, 0x0F, 0x1F, 0x1F, 0x3F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0x7F, 0x1F, 0x1F, 0x0F, 0x0F, 0x0F, 0x87, 0x87, 0x87, 0x87, 0x87, 0x0F, 0x0F, 0x0F, 0x1F, 0x1F, 0x3F, 0x7F,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
                0xF8, 0xF0, 0xE0, 0xE0, 0xE0, 0xE3, 0xC3, 0xC3, 0xC3, 0xC3, 0xC3, 0xE1, 0xE0, 0xE0, 0xF0, 0xF8, 0xF8, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xF8, 0xF0, 0x70, 0x60, 0x60, 0x41, 0x43, 0x43, 0x43, 0x43,
                0x03, 0x01, 0x00, 0x20, 0x00, 0x10, 0x08, 0x04, 0x03, 0x03, 0x03, 0x01, 0x01, 0x01, 0x07, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x01, 0x01,
                0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            lcd.SetByteMap(nanoFramewokrLogo);
            lcd.Draw();
        }

        static void DisplayLinesPointsRectabngles(Iot.Device.Pcd8544 lcd)
        {
            lcd.Clear();

            Console.WriteLine("Drawing point, line and rectangles");
            lcd.DrawPoint(5, 5, true);
            lcd.DrawLine(0, 0, 15, 35, true);
            lcd.DrawRectangle(10, 30, 10, 20, true, true);
            lcd.DrawRectangle(12, 32, 6, 16, false, true);
            // You should not forget to refresh to draw everything
            lcd.Draw();
            Thread.Sleep(2000);
            lcd.Clear();
            Console.WriteLine("Drawing 4 points at the 4 edges");
            lcd.DrawPoint(0, 0, true);
            lcd.DrawPoint(Iot.Device.Pcd8544.PixelScreenSize.Width - 1, 0, true);
            lcd.DrawPoint(Iot.Device.Pcd8544.PixelScreenSize.Width - 1, Iot.Device.Pcd8544.PixelScreenSize.Height - 1, true);
            lcd.DrawPoint(0, Iot.Device.Pcd8544.PixelScreenSize.Height - 1, true);
            lcd.Draw();
            Thread.Sleep(2000);
            Console.WriteLine("Drawing a rectangle at 2 pixels from the edge");
            lcd.DrawRectangle(2, 2, Iot.Device.Pcd8544.PixelScreenSize.Width - 4, Iot.Device.Pcd8544.PixelScreenSize.Height - 4, true, false);
            lcd.Draw();
            Thread.Sleep(2000);
            lcd.Clear();
            Console.WriteLine("Drawing 2 diagonal lines");
            lcd.DrawLine(0, 0, Iot.Device.Pcd8544.PixelScreenSize.Width - 1, Iot.Device.Pcd8544.PixelScreenSize.Height - 1, true);
            lcd.DrawLine(0, Iot.Device.Pcd8544.PixelScreenSize.Height - 1, Iot.Device.Pcd8544.PixelScreenSize.Width - 1, 0, true);
            lcd.Draw();
            Thread.Sleep(2000);
        }

        static void LcdConsole(Iot.Device.Pcd8544 lcd)
        {
            lcd.Clear();

            LcdConsole console = new LcdConsole(lcd, string.Empty, false);
            console.LineFeedMode = LineWrapMode.Truncate;
            Console.WriteLine("Nowrap test:");
            console.Write("This is a long text that should not wrap and just extend beyond the display");
            console.WriteLine("This has CRLF\r\nin it and should \r\n wrap.");
            console.Write("This goes to the last line of the display");
            console.WriteLine("This isn't printed, because it's off the screen");
            Thread.Sleep(1500);
            Console.WriteLine("Autoscroll test:");
            console.LineFeedMode = LineWrapMode.Wrap;
            console.WriteLine();
            console.WriteLine("Now the display should move up.");
            console.WriteLine("And more up.");
            for (int i = 0; i < 20; i++)
            {
                console.WriteLine($"This is line {i + 1}/{20}, but longer than the screen but you really have to add a lot of text to make it big enough");
                Thread.Sleep(500);
            }

            console.LineFeedMode = LineWrapMode.Wrap;
            console.WriteLine("Same again, this time with full wrapping.");
            for (int i = 0; i < 20; i++)
            {
                console.Write($"This is string {i + 1}/{20} longer than the screen but you really have to add a lot of text to make it big enough");
                Thread.Sleep(500);
            }

            Thread.Sleep(1500);
            Console.WriteLine("Intelligent wrapping test");
            console.LineFeedMode = LineWrapMode.WordWrap;
            console.WriteLine("Now intelligent wrapping should wrap this long sentence at word borders and ommit spaces at the start of lines.");
            Console.WriteLine("Not wrappable test");
            Thread.Sleep(1500);
            console.WriteLine("NowThisIsOneSentenceInOneWordThatCannotBeWrappedButStillAppearAllOverUpToTheEnd");
            Thread.Sleep(1500);
            Console.WriteLine("Individual line test");
            Thread.Sleep(1500);
            console.Clear();
            console.Dispose();
        }
    }
}
