// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ld2410.Commands
{
    internal sealed class ReadConfigurationsCommandAck : CommandAckFrame
    {
        internal byte MaxDistanceGate { get; set; }

        internal byte MotionRangeGate { get; set; }

        internal byte StaticRangeGate { get; set; }

        internal byte[] MotionSensitivityLevelPerGate { get; set; }

        internal byte[] RestingSensitivityLevelPerGate { get; set; }

        internal TimeSpan NoOneDuration { get; set; }

        internal ReadConfigurationsCommandAck(
            bool isSuccess,
            byte maxDistanceGate,
            byte motionRangeGate,
            byte staticRangeGate,
            byte[] motionSensitivityLevelPerGate,
            byte[] restingSensitivityLevelPerGate,
            TimeSpan noOneDuration) : base(CommandWord.ReadConfigurations, isSuccess)
        {
            MaxDistanceGate = maxDistanceGate;
            MotionRangeGate = motionRangeGate;
            StaticRangeGate = staticRangeGate;
            MotionSensitivityLevelPerGate = motionSensitivityLevelPerGate;
            RestingSensitivityLevelPerGate = restingSensitivityLevelPerGate;
            NoOneDuration = noOneDuration;
        }
    }
}
