using System;

namespace Ld2410
{
	public abstract class Frame
	{
		protected byte[] Header => new byte[4] { 0xFD, 0xFC, 0xFB, 0xFA };

		protected byte[] RawData { get; set; }

		protected byte[] End => new byte[4] { 0x04, 0x03, 0x02, 0x01 };

		public byte[] ToByteArray()
		{
			var buffer = new byte[this.Header.Length + this.RawData.Length + this.End.Length];

			Array.Copy(sourceArray: this.Header,
				sourceIndex: 0,
				destinationArray: buffer,
				destinationIndex: 0,
				length: this.Header.Length);

			Array.Copy(sourceArray: this.RawData,
				sourceIndex: 0,
				destinationArray: buffer,
				destinationIndex: this.Header.Length,
				length: this.RawData.Length);


			Array.Copy(sourceArray: this.End,
				sourceIndex: 0,
				destinationArray: buffer,
				destinationIndex: this.RawData.Length,
				length: this.End.Length);

			return buffer;
		}
	}

	public sealed class CommandFrame : Frame
	{
		public Command Command { get; set; }

		public byte[] Value { get; set; }

		public CommandFrame(Command command)
		{
			this.Command = command;
		}
	}

	public sealed class AckCommandFrame : Frame
	{
		public Command Command { get; }

		public byte[] Value { get; }

		public AckCommandFrame(Command command, byte[] value)
		{
			Command = command;
			Value = value;
		}
	}
}
