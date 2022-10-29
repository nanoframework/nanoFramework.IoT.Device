using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;

using Iot.Device.ePaper.Drivers;

namespace SSD1681Sample
{
    public class Program
    {
        public static void Main()
        {
            // Setup SPI connection with the display
            var spiConnectionSettings = new SpiConnectionSettings(1, 22)
            {
                ClockFrequency = 20_000,
                Mode = SpiMode.Mode0,
                ChipSelectLineActiveState = false,
                Configuration = SpiBusConfiguration.HalfDuplex,
                DataFlow = DataFlow.MsbFirst
            };

            using var spiDevice = new SpiDevice(spiConnectionSettings);


            // Setup required GPIO Pins
            using var gpioController = new GpioController();

            using var resetPin = gpioController.OpenPin(15, PinMode.Output);
            using var dataCommandPin = gpioController.OpenPin(5, PinMode.Output);
            using var busyPin = gpioController.OpenPin(4, PinMode.Input);
            using var ledPin = gpioController.OpenPin(2, PinMode.Output);


            // Create an instance of the display driver
            using var display = new Ssd1681(resetPin, busyPin, dataCommandPin,
                spiDevice, width: 200, height: 200, enableFramePaging: true);


            // Power on the display and initialize it
            display.PowerOn();
            display.Initialize();

            // clear the display
            display.Clear(triggerPageRefresh: true);

            // draw a frame using paging
            display.BeginFrameDraw();
            do
            {
                for (var y = 0; y < display.Height; y++)
                {
                    display.DrawPixel(100, y, inverted: true);
                }
            } while (display.NextFramePage());
            display.EndFrameDraw();
            display.PerformFullRefresh();

            // Put the display to sleep to reduce power consumption
            display.PowerDown(Ssd1681.SleepMode.DeepSleepModeTwo);
        }
    }
}
