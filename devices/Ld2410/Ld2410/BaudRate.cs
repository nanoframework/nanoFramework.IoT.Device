namespace Ld2410
{
	/// <summary>
	/// Defines the set of Baud Rates that the device's serial port supports.
	/// </summary>
	public enum BaudRate: byte
	{
		/// <summary>
		/// 9600 baud.
		/// </summary>
		BaudRate9600 = 0x0001,

		/// <summary>
		/// 19200 baud.
		/// </summary>
		BaudRate19200 = 0x0002,

		/// <summary>
		/// 38400 baud.
		/// </summary>
		BaudRate38400 = 0x0003,

		/// <summary>
		/// 57600 baud.
		/// </summary>
		BaudRate57600 = 0x0004,

		/// <summary>
		/// 115200 baud.
		/// </summary>
		BaudRate115200 = 0x0005,

		/// <summary>
		/// 230400 baud.
		/// </summary>
		BaudRate230400 = 0x0006,

		/// <summary>
		/// 256000 baud.
		/// </summary>
		BaudRate256000 = 0x0007,

		/// <summary>
		/// 460800 baud.
		/// </summary>
		BaudRate460800 = 0x0008
	}
}
