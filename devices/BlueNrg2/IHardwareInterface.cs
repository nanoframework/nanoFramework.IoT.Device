using System;

namespace BlueNrg2
{
	public delegate bool NotifyAsync();

	public interface IHardwareInterface : IDisposable
	{
		public event NotifyAsync NotifyAsyncEvent;
		public int Reset();
		public int Receive(ref byte[] buffer, ushort size);
		public int Send(byte[] buffer, ushort size);
		public long GetTick();
	}
}
