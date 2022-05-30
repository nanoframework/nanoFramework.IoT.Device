//
// Copyright (c) 2017 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.
//

using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using Iot.Device.SPS30.Entities;
using Iot.Device.SPS30.SHDLC;
using nanoFramework.Hardware.Esp32;
using nanoFramework.Sensirion.SPS30;

namespace SPS30.Sample
{
    public class Program
    {
        public static void Main()
        {
            // This sample runs on a Pycom WiPy (ESP32), where P3 (=GPIO 4) and P4 (=GPIO 15) are used for communication.
            Configuration.SetPinFunction(4, DeviceFunction.COM2_TX);
            Configuration.SetPinFunction(15, DeviceFunction.COM2_RX);

            // Setup the SPS30 communication
            var serial = new SerialPort("COM2", 115200, Parity.None, 8, StopBits.One);
            var shdlc = new SHDLCProtocol(serial, timeoutInMillis: 10000);
            var sps30 = new SPS30Sensor(shdlc);

            // Query the SPS30
            var identifier = sps30.GetDeviceInfoProductType();
            var serialnr = sps30.GetDeviceInfoSerialNumber();
            var version = sps30.ReadVersion();
            var statusreg = sps30.ReadDeviceStatusRegister();
            var cleaninginterval = sps30.GetAutoCleaningInterval();
            Debug.WriteLine($"SPS30 detected: ID={identifier}, serial={serialnr}, version={version}, status={statusreg}, cleaninginterval={cleaninginterval}");

            // Measurement loop
            try { sps30.StopMeasurement(); } catch { } // Stop measurement mode if SPS30 is still in it. Ignore any errors if it was already stopped.
            sps30.StartMeasurement(MeasurementOutputFormat.Float);
            while (true)
            {
                Thread.Sleep(5000);
                var measurement = sps30.ReadMeasuredValues();
                Debug.WriteLine($"Measurement: {measurement}");
            }

            // Example output:
            //
            // SPS30 detected: ID=00080000, serial=4E1AD1BB796C64C5, version=Firmware V2.1, Hardware V7, SHDLC V2.0, status=RawRegister: 0, FanSpeedOutOfRange: False, LaserFailure: False, FanFailureBlockedOrBroken: False, cleaninginterval=604800
            // Measurement: MassConcentration PM1.0=0.71818184, PM2.5=0.75944983, PM4.0=0.75944983, PM10.0=0.75944983, NumberConcentration PM0.5=4.89496469, PM1.0=5.69449424, PM2.5=5.73169803, PM4.0=5.73438549, PM10.0=0, TypicalParticleSize=0.38276255
        }
    }
}
