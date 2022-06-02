// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
