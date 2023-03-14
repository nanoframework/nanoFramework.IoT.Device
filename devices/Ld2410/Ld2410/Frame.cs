namespace Ld2410
{
	public abstract class Frame
	{
		public byte[] Header => new byte[4] { 0xFD, 0xFC, 0xFB, 0xFA  };

		public byte[] RawData { get; set; }

		public byte[] End => new byte[4] { 0x04, 0x03, 0x02, 0x01 };
	}

	public sealed class CommandFrame : Frame
	{
	}
}
