using System;

namespace Ld2410.Commands
{
    internal sealed class ReadConfigurationsCommand : CommandFrame
    {
        internal ReadConfigurationsCommand()
            : base(CommandWord.ReadConfigurations)
        {
        }
    }

    internal sealed class ReadConfigurationsCommandAck : CommandAckFrame
    {
        internal byte MaxDistanceGate { get; set; }
        internal byte MaxMovingDistanceGate { get; set; }
        internal byte MaxStaticDistanceGate { get; set; }

        internal byte Gate0MotionSensitivity { get; set; }
        internal byte Gate1MotionSensitivity { get; set; }
        internal byte Gate2MotionSensitivity { get; set; }
        internal byte Gate3MotionSensitivity { get; set; }
        internal byte Gate4MotionSensitivity { get; set; }
        internal byte Gate5MotionSensitivity { get; set; }
        internal byte Gate6MotionSensitivity { get; set; }
        internal byte Gate7MotionSensitivity { get; set; }
        internal byte Gate8MotionSensitivity { get; set; }

        internal byte Gate0RestSensitivity { get; set; }
        internal byte Gate1RestSensitivity { get; set; }
        internal byte Gate2RestSensitivity { get; set; }
        internal byte Gate3RestSensitivity { get; set; }
        internal byte Gate4RestSensitivity { get; set; }
        internal byte Gate5RestSensitivity { get; set; }
        internal byte Gate6RestSensitivity { get; set; }
        internal byte Gate7RestSensitivity { get; set; }
        internal byte Gate8RestSensitivity { get; set; }

        internal TimeSpan NoOneDuration { get; set; }

        internal ReadConfigurationsCommandAck(bool isSuccess)
            : base(CommandWord.ReadConfigurations, isSuccess)
        {
        }
    }
}
