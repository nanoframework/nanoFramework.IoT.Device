using Iot.Device.XPT2046;
using nanoFramework.Hardware.Esp32;
using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;

namespace TouchDemo
{

    
    public class Program
    {
        static bool touchDetected = false;

        public static void Main()
        {
            Debug.WriteLine("Hello from nanoFramework!");

            const int XPT2046_CS = Gpio.IO33; // Chip Select
            const int XPT2046_PenIRQ = Gpio.IO36; // Touch detected interupt
            const int XPT2046_COPI = Gpio.IO32;
            const int XPT2046_CIPO = Gpio.IO39;
            const int XPT2046_CLK = Gpio.IO25;

            try
            {

                // If you're using an ESP32, use nanoFramework.Hardware.Esp32 to remap the SPI pins
                // For the screen
                Configuration.SetPinFunction(13, DeviceFunction.SPI1_MOSI);
                Configuration.SetPinFunction(14, DeviceFunction.SPI1_CLOCK);
                Configuration.SetPinFunction(12, DeviceFunction.SPI1_MISO);

                // For the XPT2046 Touch controller
                Configuration.SetPinFunction(XPT2046_COPI, DeviceFunction.SPI2_MOSI);
                Configuration.SetPinFunction(XPT2046_CIPO, DeviceFunction.SPI2_MISO);
                Configuration.SetPinFunction(XPT2046_CLK, DeviceFunction.SPI2_CLOCK);

                SpiDevice spiDevice0;
                SpiDevice spiDevice;
                SpiConnectionSettings connectionSettings;
                SpiConnectionSettings connectionSettings0;

                //Bus 0 and 1 are implemented.?? Or is it 1 and 2? 
                connectionSettings0 = new SpiConnectionSettings(1, 15);
                connectionSettings0.ClockFrequency = 1_000_000;      //Set clock speed to slowest device on bus
                connectionSettings0.DataBitLength = 8;
                connectionSettings0.DataFlow = DataFlow.MsbFirst;
                connectionSettings0.Mode = SpiMode.Mode0;

                connectionSettings = new SpiConnectionSettings(2, XPT2046_CS);
                connectionSettings.ClockFrequency = 1_000_000;      //Set clock speed to slowest device on bus
                connectionSettings.DataBitLength = 8;
                connectionSettings.DataFlow = DataFlow.MsbFirst;
                connectionSettings.Mode = SpiMode.Mode0;

                // Then you create your SPI device by passing your settings
                spiDevice0 = SpiDevice.Create(connectionSettings0); // For screen
                spiDevice = SpiDevice.Create(connectionSettings);   // For XPT2046 Touch controller

                using GpioController gpio = new();
                using Xpt2046 sensor = new(spiDevice);
                var ver = sensor.GetVersion();
                Debug.WriteLine($"version: {ver}");

                gpio.OpenPin(XPT2046_PenIRQ, PinMode.InputPullUp);
                // This will enable an event on GPIO36 on falling edge when the screen if touched
                gpio.RegisterCallbackForPinValueChangedEvent(XPT2046_PenIRQ, PinEventTypes.Falling, TouchInterrupCallback);


                while (true)
                {
                    if (touchDetected)
                    {
                        var point = sensor.GetPoint();
                        Debug.WriteLine($"point: {point.X},{point.Y},{point.Weight} ");
                        Thread.Sleep(30);
                        touchDetected = false;
                    }

                    Thread.Sleep(300);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Critical Exception, Terminating: {ex.Message}");

                Thread.Sleep(Timeout.Infinite);
            }

        }
        static void TouchInterrupCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            touchDetected = true;
        }
    }
}
