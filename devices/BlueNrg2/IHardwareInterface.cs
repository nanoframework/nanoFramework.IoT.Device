using System;

namespace Iot.Device.BlueNrg2
{
	internal delegate bool NotifyAsync();

	internal interface IHardwareInterface : IDisposable
	{
		public event NotifyAsync NotifyAsyncEvent;
		public int Reset();
		public int Receive(ref byte[] buffer, ushort size);
		public int Send(byte[] buffer, ushort size);
		public long GetTick();
	}
}
