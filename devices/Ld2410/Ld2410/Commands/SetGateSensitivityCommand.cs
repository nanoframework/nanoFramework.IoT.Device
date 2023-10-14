using System;

using Ld2410.Extensions;

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

            var gateBytes = gate.ToLittleEndianBytes();
            var motionSensitivityBytes = motionSensitivity.ToLittleEndianBytes();
            var staticSensitivityBytes = staticSensitivity.ToLittleEndianBytes();

            base.Value = new byte[18];

            // the first 2 bytes represent the distance gate word (0x0000)
            // we don't have to set those as the array is initialized with 0x00
            var currentIndex = 2;

            gateBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );

            // set motion sensitivity for this gate
            base.Value[currentIndex] = 0x01;
            currentIndex++;
            motionSensitivityBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );

            // set static sensitivity for this gate
            base.Value[currentIndex] = 0x02;
            currentIndex++;
            staticSensitivityBytes.CopyToArrayWithIndexAndAdvance(
                destinationArray: base.Value,
                ref currentIndex
                );
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
