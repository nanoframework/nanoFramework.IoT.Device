using System;

namespace Iot.Device.Lp3943.Samples
{
	internal static class Utilities
	{
		public static int GetPinNumber(char port, byte pin)
		{
			if (port is < 'A' or > 'J')
				throw new ArgumentException();

			return (port - 'A') * 16 + pin;
		}
	}
}
