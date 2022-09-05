using System;
using System.Device.Gpio;
using System.Threading;

namespace Iot.Device.Rtc.Samples
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello from nanoFramework!");

            GpioController controller = new GpioController(PinNumberingScheme.Logical);
            using Ds1302 rtc = new(13, 12, 14, controller);

            if (rtc.IsHalted())
            {
                Console.WriteLine("RTC is halted!");
            }
            else
            {
                rtc.Halt();
                Console.WriteLine("RTC was halted now!");
            }

            var currentTime = new DateTime(2022, 8, 5, 18, 31, 0);
            Console.WriteLine(currentTime.ToString());
            rtc.DateTime = currentTime;

            while (true)
            {
                // read time
                DateTime dt = rtc.DateTime;
                Console.WriteLine($"Time: {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
                Thread.Sleep(5000);
            }
        }
    }
}
