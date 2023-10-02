using System;

using Ld2410.Extensions;

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

            var maxMovementDistanceGateBytes = maximumMovementDistanceGate.ToLittleEndianBytes();
            var maxRestingDistanceGateBytes = maximumRestingDistanceGate.ToLittleEndianBytes();
            var noOneDurationBytes = noOneDuration == default
                ? 5.ToLittleEndianBytes()
                : ((uint)noOneDuration.TotalSeconds).ToLittleEndianBytes(); // the uint cast to force 4 bytes as per specs

            base.Value = new byte[18];

            // the first 2 bytes represent the max moving distance gate word (0x0000)
            // we don't have to set those as the array is initialized with 0x00
            var currentIndex = 2;

            // set the max movement distance gate
            maxMovementDistanceGateBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );

            // next, set the max resting distance gate
            base.Value[currentIndex] = 0x01;
            currentIndex++; // skip the next item because it should 0x00
            maxRestingDistanceGateBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );

            // next, set the no-one duration time
            base.Value[currentIndex] = 0x02;
            currentIndex++; // skip the next item because it should 0x00
            noOneDurationBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );
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
