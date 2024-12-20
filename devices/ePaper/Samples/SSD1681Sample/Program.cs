// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System.Device.Gpio;
using System.Device.Spi;
using System.Drawing;

using Iot.Device.EPaper;
using Iot.Device.EPaper.Buffers;
using Iot.Device.EPaper.Drivers.Ssd168x;
using Iot.Device.EPaper.Drivers.Ssd168x.Ssd1681;
using Iot.Device.EPaper.Enums;
using Iot.Device.EPaper.Fonts;
using nanoFramework.UI;

namespace SSD1681Sample
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
                ClockFrequency = Ssd1681.SpiClockFrequency,
                Mode = Ssd1681.SpiMode,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            using var spiDevice = new SpiDevice(spiConnectionSettings);

            // Create an instance of the display driver
            using var display = new Ssd1681(
                spiDevice,
                resetPin: 15,
                busyPin: 4,
                dataCommandPin: 5,
                width: 200,
                height: 200,
                gpioController,
                enableFramePaging: true,
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
                // the icons are known to be contained within a single frame page
                // this speeds up generating the frame as we don't process the bitmaps multiple times
                if (display.FrameBuffer.IsRangeWithinFrameBuffer(new System.Drawing.Point(0, 0), new System.Drawing.Point(199, 36)))
                {
                    gfx.DrawBitmap(cloudBitmap, new System.Drawing.Point(8, 4), rotate: false);
                    gfx.DrawBitmap(cloudBitmap, new System.Drawing.Point(44, 4), rotate: false);
                    gfx.DrawBitmap(cloudBitmap, new System.Drawing.Point(80, 4), rotate: false);
                    gfx.DrawBitmap(cloudBitmap, new System.Drawing.Point(116, 4), rotate: false);
                    gfx.DrawBitmap(cloudBitmap, new System.Drawing.Point(150, 4), rotate: false);
                }

                // this text spans 2 frame pages
                gfx.DrawText("20c", font, 12, 34, Color.Black);
                gfx.DrawText("24c", font, 46, 34, Color.Red);
                gfx.DrawText("21c", font, 88, 34, Color.Black);
                gfx.DrawText("22c", font, 120, 34, Color.Red);
                gfx.DrawText("22c", font, 154, 34, Color.Black);

                // the days of week are known to be within a single frame page
                // this speeds up generating the frame as we don't process the text multiple times
                if (display.FrameBuffer.IsRangeWithinFrameBuffer(new System.Drawing.Point(0, 50), new System.Drawing.Point(199, 70)))
                {
                    gfx.DrawText("Mon", font, 12, 50, Color.Black);
                    gfx.DrawText("Tue", font, 46, 50, Color.Red);
                    gfx.DrawText("Wed", font, 88, 50, Color.Black);
                    gfx.DrawText("Thu", font, 120, 50, Color.Red);
                    gfx.DrawText("Fri", font, 154, 50, Color.Black);

                    gfx.DrawLine(0, 65, 200, 65, Color.Black);
                }

                gfx.DrawText("Daily Chuck Norris Joke:", font, 4, 75, Color.Black);
                gfx.DrawText("Chuck Norris's email is   gmail@chucknorris.com", font, 4, 90, Color.Red);

                gfx.DrawLine(0, 120, 200, 120, Color.Black);

                gfx.DrawBitmap(musicBitmap, new System.Drawing.Point(84, 125), rotate: false);
                gfx.DrawText("Now Playing:", font, 8, 160, Color.Black);
                gfx.DrawText("Megadeth - Dystopia", font, 16, 175, Color.Black);

            } while (display.NextFramePage());
            display.EndFrameDraw();

            // at this point, all frame pages have been pushed to the display
            // this will execute the refresh sequence and display the frame data
            display.PerformPartialRefresh();

            // Done! now put the display to sleep to reduce power consumption
            display.PowerDown(SleepMode.DeepSleepModeTwo);
        }
    }
}
