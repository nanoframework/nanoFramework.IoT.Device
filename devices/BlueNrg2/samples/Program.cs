// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;
using BlueNrg2.Samples;

namespace Iot.Device.BlueNrg2.Samples
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Debug.WriteLine("Program Start");

            var chipSelect = Utilities.GetPinNumber('E', 4);
            var ledPin = Utilities.GetPinNumber('G', 13);

            var controller = new GpioController();

            controller.OpenPin(ledPin, PinMode.Output);

            var blueNrg2 = new BlueNrg2(
                new SpiConnectionSettings(4, chipSelect),
                Utilities.GetPinNumber('C', 13),
                Utilities.GetPinNumber('E', 3),
                controller
            );

            blueNrg2.StartBluetoothThread();

            blueNrg2.Gatt.Init();

            while (true)
            {
                Thread.Sleep(100);
            }

            Debug.WriteLine("Program End");
        }
    }
}
