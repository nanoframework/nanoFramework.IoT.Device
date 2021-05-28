using System;
using System.Diagnostics;
using System.Threading;

namespace Iot.Device.Bmxx80.sample
{
    public class Program
    {
        public static void Main()
        {
            // Choose your sensor and uncomment accordingly
            //Bme280_sample.RunSample();
            //Bme680_sample.RunSample();
            Bmp280_sample.RunSample();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
