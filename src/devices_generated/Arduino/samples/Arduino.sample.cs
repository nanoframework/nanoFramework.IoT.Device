// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.I2c;
using System.Device.Spi;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Iot.Device.Adc;
using Iot.Device.Arduino;
using Iot.Device.Bmxx80;
using Iot.Device.Bmxx80.PowerMode;
using Iot.Device.Common;
using Microsoft.Extensions.Logging;
using UnitsNet;

namespace Arduino.Samples
{
    /// <summary>
    /// Test application for Arduino/Firmata protocol
    /// </summary>
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Debug.WriteLine("Usage: Arduino.sample <PortName>");
                Debug.WriteLine("i.e.: Arduino.sample COM4");
                return;
            }

            string portName = args[0];

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

            // Statically register our factory. Note that this must be done before instantiation of any class that wants to use logging.
            LogDispatcher.LoggerFactory = loggerFactory;

            using (var port = new SerialPort(portName, 115200))
            {
                Debug.WriteLine($"Connecting to Arduino on {portName}");
                try
                {
                    port.Open();
                }
                catch (UnauthorizedAccessException x)
                {
                    Debug.WriteLine($"Could not open COM port: {x.Message} Possible reason: Arduino IDE connected or serial console open");
                    return;
                }

                ArduinoBoard board = new ArduinoBoard(port.BaseStream);
                try
                {
                    // This implicitly connects
                    Debug.WriteLine($"Connecting... Firmware version: {board.FirmwareVersion}, Builder: {board.FirmwareName}");
                    while (Menu(board))
                    {
                    }
                }
                catch (TimeoutException x)
                {
                    Debug.WriteLine($"No answer from board: {x.Message} ");
                }
                finally
                {
                    port.Close();
                    board?.Dispose();
                }
            }
        }

        private static void BoardOnLogMessages(string message, Exception? exception)
        {
            Debug.WriteLine("Log message: " + message);
            if (exception != null)
            {
                Debug.WriteLine(exception);
            }
        }

        private static bool Menu(ArduinoBoard board)
        {
            Debug.WriteLine("Hello I2C and GPIO on Arduino!");
            Debug.WriteLine("Select the test you want to run:");
            Debug.WriteLine(" 1 Run I2C tests with a BMP280");
            Debug.WriteLine(" 2 Run GPIO tests with a simple led blinking on GPIO6 port");
            Debug.WriteLine(" 3 Run polling button test on GPIO2");
            Debug.WriteLine(" 4 Run event wait test event on GPIO2 on Falling and Rising");
            Debug.WriteLine(" 5 Run callback event test on GPIO2");
            Debug.WriteLine(" 6 Run PWM test with a LED dimming on GPIO6 port");
            Debug.WriteLine(" 7 Blink the LED according to the input on A1");
            Debug.WriteLine(" 8 Read analog channel as fast as possible");
            Debug.WriteLine(" 9 Run SPI tests with an MCP3008 (experimental)");
            Debug.WriteLine(" 0 Detect all devices on the I2C bus");
            Debug.WriteLine(" H Read DHT11 Humidity sensor on GPIO 3 (experimental)");
            Debug.WriteLine(" X Exit");
            var key = Console.ReadKey();
            Debug.WriteLine();

            switch (key.KeyChar)
            {
                case '1':
                    TestI2c(board);
                    break;
                case '2':
                    TestGpio(board);
                    break;
                case '3':
                    TestInput(board);
                    break;
                case '4':
                    TestEventsDirectWait(board);
                    break;
                case '5':
                    TestEventsCallback(board);
                    break;
                case '6':
                    TestPwm(board);
                    break;
                case '7':
                    TestAnalogIn(board);
                    break;
                case '8':
                    TestAnalogCallback(board);
                    break;
                case '9':
                    TestSpi(board);
                    break;
                case '0':
                    ScanDeviceAddressesOnI2cBus(board);
                    break;
                case 'h':
                case 'H':
                    TestDht(board);
                    break;
                case 'x':
                case 'X':
                    return false;
            }

            return true;
        }

        private static void TestPwm(ArduinoBoard board)
        {
            int pin = 6;
            using (var pwm = board.CreatePwmChannel(0, pin, 100, 0))
            {
                Debug.WriteLine("Now dimming LED. Press any key to exit");
                while (!Console.KeyAvailable)
                {
                    pwm.Start();
                    for (double fadeValue = 0; fadeValue <= 1.0; fadeValue += 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 30 milliseconds to see the dimming effect
                        Thread.Sleep(30);
                    }

                    // fade out from max to min in increments of 5 points:
                    for (double fadeValue = 1.0; fadeValue >= 0; fadeValue -= 0.05)
                    {
                        // sets the value (range from 0 to 255):
                        pwm.DutyCycle = fadeValue;
                        // wait for 30 milliseconds to see the dimming effect
                        Thread.Sleep(30);
                    }

                }

                Console.ReadKey();
                pwm.Stop();
            }
        }

        private static void TestI2c(ArduinoBoard board)
        {
            var device = board.CreateI2cDevice(new I2cConnectionSettings(0, Bmp280.DefaultI2cAddress));

            var bmp = new Bmp280(device);
            bmp.StandbyTime = StandbyTime.Ms250;
            bmp.SetPowerMode(Bmx280PowerMode.Normal);
            Debug.WriteLine("Device open");
            while (!Console.KeyAvailable)
            {
                bmp.TryReadTemperature(out var temperature);
                bmp.TryReadPressure(out var pressure);
                Console.Write($"\rTemperature: {temperature.DegreesCelsius:F2}°C. Pressure {pressure.Hectopascals:F1} hPa                  ");
                Thread.Sleep(100);
            }

            bmp.Dispose();
            device.Dispose();
            Console.ReadKey();
            Debug.WriteLine();
        }

        private static void ScanDeviceAddressesOnI2cBus(ArduinoBoard board)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append("     0  1  2  3  4  5  6  7  8  9  a  b  c  d  e  f");
            stringBuilder.Append(Environment.NewLine);

            for (int startingRowAddress = 0; startingRowAddress < 128; startingRowAddress += 16)
            {
                stringBuilder.Append($"{startingRowAddress:x2}: ");  // Beginning of row.

                for (int rowAddress = 0; rowAddress < 16; rowAddress++)
                {
                    int deviceAddress = startingRowAddress + rowAddress;

                    // Skip the unwanted addresses.
                    if (deviceAddress < 0x3 || deviceAddress > 0x77)
                    {
                        stringBuilder.Append("   ");
                        continue;
                    }

                    var connectionSettings = new I2cConnectionSettings(0, deviceAddress);
                    using (var i2cDevice = board.CreateI2cDevice(connectionSettings))
                    {
                        try
                        {
                            i2cDevice.ReadByte();  // Only checking if device is present.
                            stringBuilder.Append($"{deviceAddress:x2} ");
                        }
                        catch
                        {
                            stringBuilder.Append("-- ");
                        }
                    }
                }

                stringBuilder.Append(Environment.NewLine);
            }

            Debug.WriteLine(stringBuilder.ToString());
        }

        public static void TestGpio(ArduinoBoard board)
        {
            // Use Pin 6
            const int gpio = 6;
            var gpioController = board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Debug.WriteLine("Blinking GPIO6");
            while (!Console.KeyAvailable)
            {
                gpioController.Write(gpio, PinValue.High);
                Thread.Sleep(500);
                gpioController.Write(gpio, PinValue.Low);
                Thread.Sleep(500);
            }

            Console.ReadKey();
            gpioController.Dispose();
        }

        public static void TestAnalogIn(ArduinoBoard board)
        {
            // Use Pin 6
            const int gpio = 6;
            int analogPin = GetAnalogPin1(board);
            var gpioController = board.CreateGpioController();
            var analogController = board.CreateAnalogController(0);

            var pin = analogController.OpenPin(analogPin);
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Output);

            Debug.WriteLine("Blinking GPIO6, based on analog input.");
            while (!Console.KeyAvailable)
            {
                ElectricPotential voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.High);
                Thread.Sleep((int)(voltage * 100).Volts);
                voltage = pin.ReadVoltage();
                gpioController.Write(gpio, PinValue.Low);
                Thread.Sleep((int)(voltage * 100).Volts);
            }

            pin.Dispose();
            Console.ReadKey();
            analogController.Dispose();
            gpioController.Dispose();
        }

        public static void TestAnalogCallback(ArduinoBoard board)
        {
            int analogPin = GetAnalogPin1(board);
            var analogController = board.CreateAnalogController(0);
            board.SetAnalogPinSamplingInterval(TimeSpan.FromMilliseconds(10));
            var pin = analogController.OpenPin(analogPin);
            pin.EnableAnalogValueChangedEvent(null, 0);

            pin.ValueChanged += (sender, args) =>
            {
                if (args.PinNumber == analogPin)
                {
                    Debug.WriteLine($"New voltage: {args.Value}.");
                }
            };

            Debug.WriteLine("Waiting for changes on the analog input");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            pin.DisableAnalogValueChangedEvent();
            pin.Dispose();
            analogController.Dispose();
        }

        private static int GetAnalogPin1(ArduinoBoard board)
        {
            int analogPin = 15;
            foreach (var pin in board.SupportedPinConfigurations)
            {
                if (pin.AnalogPinNumber == 1)
                {
                    analogPin = pin.Pin;
                    break;
                }
            }

            return analogPin;
        }

        public static void TestInput(ArduinoBoard board)
        {
            const int gpio = 2;
            var gpioController = board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(gpio);
            gpioController.SetPinMode(gpio, PinMode.Input);

            if (gpioController.GetPinMode(gpio) != PinMode.Input)
            {
                throw new InvalidOperationException("Couldn't set pin mode");
            }

            Debug.WriteLine("Polling input pin 2");
            var lastState = gpioController.Read(gpio);
            while (!Console.KeyAvailable)
            {
                var newState = gpioController.Read(gpio);
                if (newState != lastState)
                {
                    if (newState == PinValue.High)
                    {
                        Debug.WriteLine("Button pressed");
                    }
                    else
                    {
                        Debug.WriteLine("Button released");
                    }
                }

                lastState = newState;
                Thread.Sleep(10);
            }

            Console.ReadKey();
            gpioController.Dispose();
        }

        public static void TestEventsDirectWait(ArduinoBoard board)
        {
            const int Gpio2 = 2;
            var gpioController = board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Input);

            Debug.WriteLine("Waiting for both falling and rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    Debug.WriteLine($"Event on GPIO {Gpio2}, event type: {res.EventTypes}");
                }
            }

            Console.ReadKey();
            Debug.WriteLine("Waiting for only rising events");
            while (!Console.KeyAvailable)
            {
                var res = gpioController.WaitForEvent(Gpio2, PinEventTypes.Rising, new TimeSpan(0, 0, 0, 0, 50));
                if ((!res.TimedOut) && (res.EventTypes != PinEventTypes.None))
                {
                    MyCallback(gpioController, new PinValueChangedEventArgs(res.EventTypes, Gpio2));
                }
            }

            gpioController.Dispose();
        }

        public static void TestEventsCallback(ArduinoBoard board)
        {
            const int Gpio2 = 2;
            var gpioController = board.CreateGpioController();

            // Opening GPIO2
            gpioController.OpenPin(Gpio2);
            gpioController.SetPinMode(Gpio2, PinMode.Input);

            Debug.WriteLine("Setting up events on GPIO2 for rising and falling");

            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Falling | PinEventTypes.Rising, MyCallback);
            Debug.WriteLine("Event setup, press a key to remove the falling event");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
            gpioController.RegisterCallbackForPinValueChangedEvent(Gpio2, PinEventTypes.Rising, MyCallback);
            Debug.WriteLine("Now only waiting for rising events, press a key to remove all events and quit");
            while (!Console.KeyAvailable)
            {
                // Nothing to do
                Thread.Sleep(100);
            }

            Console.ReadKey();
            gpioController.UnregisterCallbackForPinValueChangedEvent(Gpio2, MyCallback);
            gpioController.Dispose();
        }

        private static void MyCallback(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            Debug.WriteLine($"Event on GPIO {pinValueChangedEventArgs.PinNumber}, event type: {pinValueChangedEventArgs.ChangeType}");
        }

        public static void TestSpi(ArduinoBoard board)
        {
            const double vssValue = 5; // Set this to the supply voltage of the arduino. Most boards have 5V, some newer ones run at 3.3V.
            SpiConnectionSettings settings = new SpiConnectionSettings(0, 10);
            using (var spi = board.CreateSpiDevice(settings))
            using (Mcp3008 mcp = new Mcp3008(spi))
            {
                Debug.WriteLine("SPI Device open");
                while (!Console.KeyAvailable)
                {
                    double vdd = mcp.Read(5);
                    double vss = mcp.Read(6);
                    double middle = mcp.Read(7);
                    Debug.WriteLine($"Raw values: VSS {vss} VDD {vdd} Average {middle}");
                    vdd = vssValue * vdd / 1024;
                    vss = vssValue * vss / 1024;
                    middle = vssValue * middle / 1024;
                    Debug.WriteLine($"Converted values: VSS {vss:F2}V, VDD {vdd:F2}V, Average {middle:F2}V");
                    Thread.Sleep(200);
                }
            }

            Console.ReadKey();
        }

        public static void TestDht(ArduinoBoard board)
        {
            Debug.WriteLine("Reading DHT11. Any key to quit.");

            while (!Console.KeyAvailable)
            {
                if (board.TryReadDht(3, 11, out var temperature, out var humidity))
                {
                    Debug.WriteLine($"Temperature: {temperature}, Humidity {humidity}");
                }
                else
                {
                    Debug.WriteLine("Unable to read DHT11");
                }

                Thread.Sleep(2500);
            }

            Console.ReadKey();
        }
    }
}
