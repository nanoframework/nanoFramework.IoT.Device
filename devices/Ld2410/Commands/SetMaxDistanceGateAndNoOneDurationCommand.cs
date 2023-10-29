using System;
using System.Buffers.Binary;

namespace Ld2410.Commands
{
	internal sealed class SetMaxDistanceGateAndNoOneDurationCommand : CommandFrame
	{
		internal SetMaxDistanceGateAndNoOneDurationCommand(
			uint maximumMovementDistanceGate = 8,
			uint maximumRestingDistanceGate = 8,
			TimeSpan noOneDuration = default) : base(CommandWord.SetMaxDistanceGateAndNoOneDuration)
		{
			if (maximumMovementDistanceGate < 2
				|| maximumRestingDistanceGate < 2
				|| maximumMovementDistanceGate > 8
				|| maximumRestingDistanceGate > 8
				|| noOneDuration.TotalSeconds < 0
				|| noOneDuration.TotalSeconds > ushort.MaxValue)
			{
				throw new ArgumentOutOfRangeException();
			}

			var valueSpan = new SpanByte(base.Value = new byte[18]);

			// the first 2 bytes represent the max moving distance gate word (0x0000)
			// we don't have to set those as the array is initialized with 0x00
			var currentIndex = 2;

			BinaryPrimitives.WriteUInt32LittleEndian(valueSpan.Slice(currentIndex), maximumMovementDistanceGate);
			currentIndex += 4; //size of maximumMovementDistanceGate

			// next, set the max resting distance gate
			base.Value[currentIndex] = 0x01;
			currentIndex += 2; // skip the next byte because it should 0x00
			BinaryPrimitives.WriteUInt32LittleEndian(valueSpan.Slice(currentIndex), maximumRestingDistanceGate);
			currentIndex += 4; //size of maximumRestingDistanceGate

			// next, set the no-one duration time
			base.Value[currentIndex] = 0x02;
			currentIndex += 2; // skip the next item because it should 0x00
			BinaryPrimitives.WriteUInt32LittleEndian(valueSpan.Slice(currentIndex), value: noOneDuration == default ? 5 : (uint)noOneDuration.TotalSeconds);
		}
	}

	internal sealed class SetMaxDistanceGateAndNoOneDurationCommandAck : CommandAckFrame
	{
		internal SetMaxDistanceGateAndNoOneDurationCommandAck(bool isSuccess)
			: base(CommandWord.SetMaxDistanceGateAndNoOneDuration, isSuccess)
		{
		}
	}
}
