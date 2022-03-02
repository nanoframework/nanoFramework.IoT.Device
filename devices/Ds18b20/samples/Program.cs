using System;
using System.Diagnostics;
using System.Threading;
using nanoFramework.Device.OneWire;
using nanoFramework.Hardware.Esp32;

namespace Iot.Device.Ds18b20.Samples
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello from Ds18b20!");
            Configuration.SetPinFunction(16, DeviceFunction.COM2_RX);
            Configuration.SetPinFunction(17, DeviceFunction.COM2_TX);
            OneWireHost oneWire = new OneWireHost();


            Ds18b20 ds18b20 = new Ds18b20(oneWire, null, true, /* Multidrop, network*/TemperatureResolution.VeryHigh);

            ds18b20.SetSearchMode = Ds18b20.NORMAL;
            ds18b20.Initialize(); //Initialize sensors | search for 18B20 devices

            ds18b20.Reset();
            ds18b20.Initialize(); //again, if initialization is successful, object will have valid address (see above)
            int i = 3;
            while (i-- > 0)
            {
                ds18b20.PrepareToRead();
                ds18b20.Read();

                Debug.WriteLine($"Temperature: {ds18b20.Temperature.DegreesCelsius.ToString("F")}\u00B0C");
                Thread.Sleep(5000);
            }
            oneWire.Dispose();
        }
    }
}
