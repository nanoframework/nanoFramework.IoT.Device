using System;
using System.IO;

namespace Ld2410
{
	public abstract class Frame
	{
		protected byte[] Header => new byte[4] { 0xFD, 0xFC, 0xFB, 0xFA };

		public Command Command { get; protected set; }

		public byte[] Value { get; protected set; }

		protected byte[] End => new byte[4] { 0x04, 0x03, 0x02, 0x01 };
	}

	public sealed class CommandFrame : Frame
	{
		public CommandFrame(Command command, byte[] value)
		{
			base.Command = command;
			base.Value = value;
		}

		public void WriteToStream(Stream stream)
		{
			/*
			 * The structure of the command frame in the stream is:
			 * 
			 * FRAME HEADER | FRAME DATA LENGTH | FRAME DATA (COMMAND + VALUE) | FRAME END
			 * 
			 * All in little-endian.
			 * 
			 */

			// FRAME HEADER
			stream.Write(Header, offset: 0, count: Header.Length);

			/*
			 * FRAME DATA LENGTH = Command Word Length + Command Value Length.
			 * 
			 * Command Word Length is always 2 Bytes.
			 * 
			 * The Length value has to be 2 bytes long so we cast to C# Short.
			 * 
			 */
			var frameDataLength = BitConverter.GetBytes((short)(2 + base.Value.Length));
			stream.Write(frameDataLength, offset: 0, count: frameDataLength.Length);

			// FRAME DATA: COMMAND
			var commandBytes = ((ushort)Command).ToLittleEndianBytes();
			stream.Write(commandBytes, offset: 0, count: commandBytes.Length);

			// FRAME DATA: VALUE
			stream.Write(Value, offset: 0, count: Value.Length);

			// FRAME END
			stream.Write(End, offset: 0, count: End.Length);
		}
	}

	public sealed class AckCommandFrame : Frame
	{
		public AckCommandFrame(Command command, byte[] value)
		{
			base.Command = command;
			base.Value = value;
		}
	}
}
