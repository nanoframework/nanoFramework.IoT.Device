//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Iot.Device.Scd30;
using nanoFramework.Hardware.Esp32;

namespace SCD30.Sample
{
    public class Program
    {
        public static void Main()
        {
            // This sample runs on a Pycom WiPy (ESP32), where P5 (=GPIO 5) and P6 (=GPIO 27) are used for communication.
            Configuration.SetPinFunction(5, DeviceFunction.COM3_TX);
            Configuration.SetPinFunction(27, DeviceFunction.COM3_RX);

            // Setup the SCD30 communication
            var scd30 = new Scd30Sensor(new SerialPort("COM3"));

            // Query the SCD30
            var firmware = scd30.ReadFirmwareVersion();
            Debug.WriteLine($"SCD30 detected: Firmware version={firmware}");

            // Measurement loop
            scd30.SetMeasurementInterval(TimeSpan.FromSeconds(2));
            scd30.StartContinuousMeasurement();
            while (true)
            {
                Thread.Sleep(5000);
                if (scd30.GetDataReadyStatus())
                {
                    var measurement = scd30.ReadMeasurement();
                    Debug.WriteLine($"Measurement: {measurement}");
                }
            }

            // Example output:
            //
            // SCD30 detected: Firmware version=3.66
            // Measurement: Co2ConcentrationInPpm=400.8741 ppm, Temperature=27.10421752 °C, RelativeHumidity=32.75756835 %RH
        }
    }
}
