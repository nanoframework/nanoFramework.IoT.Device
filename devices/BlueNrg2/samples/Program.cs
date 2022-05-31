using System.Device.Gpio;
using System.Device.Spi;
using System.Diagnostics;
using System.Threading;

namespace BlueNrg2.Samples
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

			var blueNrg2 = new Iot.Device.BlueNrg2.BlueNrg2(
				new SpiConnectionSettings(4, chipSelect),
				Utilities.GetPinNumber('C', 13),
				Utilities.GetPinNumber('E', 3),
				null,
				controller
			);

			blueNrg2.StartBluetoothThread();

			while (true)
			{
				Thread.Sleep(100);
			}

			Debug.WriteLine("Program End");
		}
	}
}
