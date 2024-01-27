// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Reporting
{
    /// <summary>
    /// Represents the engineering more report frame produced by the radar when engineering mode is enabled.
    /// </summary>
    public sealed class EngineeringModeReportFrame : BasicReportFrame
    {
        /// <summary>
        /// Gets the maximum distance gate supported by the radar for moving targets.
        /// </summary>
        public byte MaxMovingDistanceGate { get; internal set; }

        /// <summary>
        /// Gets the maximum distance gate supported by the radar for static targets.
        /// </summary>
        public byte MaxStaticDistanceGate { get; internal set; }

        /// <summary>
        /// Gets the energy (movement and static) per distance gate.
        /// </summary>
        public GateEnergy[] GateData { get; internal set; }

        /// <summary>
        /// Gets the additional data the radar might have appended to the report.
        /// </summary>
        public byte[] AdditionalData { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EngineeringModeReportFrame"/> class.
        /// </summary>
        public EngineeringModeReportFrame()
        {
            DataType = ReportingType.EngineeringMode;
            GateData = new GateEnergy[9];
        }
    }
}
