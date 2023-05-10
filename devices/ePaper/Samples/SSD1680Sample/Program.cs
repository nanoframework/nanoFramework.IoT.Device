// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;

using Iot.Device.EPaper;
using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Drivers.Ssd168x;
using Iot.Device.EPaper.Drivers.Ssd168x.Ssd1680;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Fonts;
using nanoFramework.UI;

namespace SSD1680Sample
{
    public class Program
    {
        // 32x32
        private static readonly byte[] cloud = {
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x80, 0xff, 0xff,
            0xfe, 0x00, 0x7f, 0xff, 0xfc, 0x00, 0x35, 0xff, 0xfc, 0x00, 0x00, 0x7f, 0xf8, 0x00, 0x00, 0x3f,
            0xf8, 0x00, 0x00, 0x1f, 0xf8, 0x00, 0x00, 0x1f, 0xf0, 0x00, 0x00, 0x0f, 0xc0, 0x00, 0x00, 0x03,
            0x80, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x80, 0x00, 0x00, 0x01, 0x80, 0x00, 0x00, 0x01,
            0xe0, 0x00, 0x00, 0x07, 0xfe, 0xdb, 0x6d, 0xbf, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
            0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
        };

        // 32x32
        private static readonly byte[] music =
        {
            0xfd, 0xff, 0xff, 0xff, 0xfc, 0xff, 0xff, 0xff, 0xfc, 0x7f, 0xff, 0xff, 0xfc, 0x3f, 0xff, 0xff,
            0xfc, 0x1f, 0xff, 0xff, 0xfd, 0x8f, 0xff, 0xff, 0xfd, 0xc7, 0xff, 0xff, 0xfd, 0xe3, 0xff, 0xff,
            0xfd, 0xf1, 0xff, 0xf0, 0xfd, 0xf9, 0xff, 0x00, 0xfd, 0xf9, 0xf8, 0x00, 0xfd, 0xf9, 0x80, 0x00,
            0xfd, 0xf3, 0x80, 0x00, 0xfd, 0xc3, 0x80, 0x00, 0xfd, 0x87, 0x80, 0x1c, 0xc1, 0xff, 0x81, 0xfc,
            0x81, 0xff, 0x8f, 0xfc, 0x01, 0xff, 0x9f, 0xfc, 0x01, 0xff, 0x9f, 0xfc, 0x01, 0xff, 0x9f, 0xfc,
            0x01, 0xff, 0x9f, 0xfc, 0x83, 0xff, 0x9f, 0xc0, 0xe7, 0xff, 0x9f, 0x80, 0xff, 0xff, 0x9f, 0x80,
            0xff, 0xff, 0x9f, 0x00, 0xff, 0xf0, 0x1f, 0x80, 0xff, 0xf0, 0x1f, 0x81, 0xff, 0xe0, 0x1f, 0xc3,
            0xff, 0xe0, 0x1f, 0xff, 0xff, 0xe0, 0x3f, 0xff, 0xff, 0xf0, 0x3f, 0xff, 0xff, 0xf8, 0x7f, 0xff
        };

        public static void Main()
        {
            // Create an instance of the GPIO Controller.
            // The display driver uses this to open pins to the display device.
            // You could also pass null to Ssd1681 instead of a GpioController instance and it will make one for you.
            using var gpioController = new GpioController();

            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(busId: 1, chipSelectLine: 22)
            {
                ClockFrequency = Ssd1680.SpiClockFrequency,
                Mode = Ssd1680.SpiMode,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            using var spiDevice = new SpiDevice(spiConnectionSettings);

            // Create an instance of the display driver
            using var display = new Ssd1680(
                spiDevice,
                resetPin: 15,
                busyPin: 4,
                dataCommandPin: 5,
                width: 160,
                height: 152,
                gpioController,
                enableFramePaging: false,
                shouldDispose: false);

            // Power on the display and initialize it
            display.PowerOn();
            display.Initialize();

            // clear the display
            display.Clear(triggerPageRefresh: true);

            // initialize the graphics library
            using var gfx = new Graphics(display)
            {
                DisplayRotation = Rotation.Default
            };

            // initialize the bitmaps we will render later on
            var cloudBitmap = new FrameBuffer1BitPerPixel(32, 32, cloud);
            var musicBitmap = new FrameBuffer1BitPerPixel(32, 32, music);

            // a simple font to use
            // you can make use your own font by implementing IFont interface
            var font = new Font8x12();

            // draw a frame to the display by using paging to save on memory usage
            // paging can be slower than having the full frame in memory
            // it is a compromise: speed vs memory space.
            display.BeginFrameDraw();
            do
            {
                gfx.DrawBitmap(cloudBitmap, new Point(16, 4), rotate: false);
                gfx.DrawText("Cloudy & Windy.", font, x: 16, y: 38, Color.Black);
                gfx.DrawText("Temp. ", font, x: 16, y: 52, Color.Black);
                gfx.DrawText("24c", font, x: 64, y: 52, Color.Red);

                gfx.DrawLine(startX: 0, startY: 68, endX: 160, endY: 68, Color.Black);

                gfx.DrawBitmap(musicBitmap, new Point(x: 64, y: 72));
                gfx.DrawText("Now Playing:", font, x: 16, y: 108, Color.Black);
                gfx.DrawText("Megadeth - Lying in State", font, x: 16, y: 122, Color.Red);

            } while (display.NextFramePage());
            display.EndFrameDraw();

            // at this point, all frame pages have been pushed to the display
            // this will execute the refresh sequence and display the frame data
            display.PerformFullRefresh();

            // Done! now put the display to sleep to reduce power consumption
            display.PowerDown(SleepMode.DeepSleepModeTwo);
        }
    }
}
