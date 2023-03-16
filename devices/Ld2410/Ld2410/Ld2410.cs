using System;
using System.IO.Ports;

namespace Ld2410
{
	public sealed class Ld2410 : IDisposable
	{
		private readonly SerialPort serialPort;

		public Ld2410(string serialPortName, BaudRate baudRate = BaudRate.BaudRate256000)
		{
			this.serialPort = new SerialPort(portName: serialPortName,
				baudRate: (int)baudRate,
				stopBits: StopBits.One,
				parity: Parity.None);
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			serialPort?.Dispose();
		}
	}
}
