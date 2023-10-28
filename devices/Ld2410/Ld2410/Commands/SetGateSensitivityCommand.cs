using System;
using System.Buffers.Binary;

namespace Ld2410.Commands
{
	internal sealed class SetGateSensitivityCommand : CommandFrame
	{
		internal SetGateSensitivityCommand(int gate, uint motionSensitivity, uint staticSensitivity)
			: base(CommandWord.ConfigureGateSensitivity)
		{
			if (motionSensitivity > 100
				|| staticSensitivity > 100)
			{
				throw new ArgumentOutOfRangeException();
			}

			var valueSpan = new SpanByte(base.Value = new byte[18]);

			// set gate number
			// the first 2 bytes represent the distance gate word (0x0000)
			// we don't have to set those as the array is initialized with 0x00
			var currentIndex = 2;

			BinaryPrimitives.WriteInt32LittleEndian(valueSpan.Slice(currentIndex), gate);
			currentIndex += 4; // size of gate variable

			// set motion sensitivity for this gate
			valueSpan[currentIndex] = 0x01;
			currentIndex += 2;
			BinaryPrimitives.WriteUInt32LittleEndian(valueSpan.Slice(currentIndex), motionSensitivity);
			currentIndex += 4; // size of gate variable

			// set static sensitivity for this gate
			valueSpan[currentIndex] = 0x02;
			currentIndex += 2;
			BinaryPrimitives.WriteUInt32LittleEndian(valueSpan.Slice(currentIndex), staticSensitivity);
		}
	}

	internal sealed class SetGateSensitivityCommandAck : CommandAckFrame
	{
		public SetGateSensitivityCommandAck(bool isSuccess)
			: base(CommandWord.ConfigureGateSensitivity, isSuccess)
		{
		}
	}
}
