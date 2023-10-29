// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using UnitsNet;

namespace Iot.Device.Ld2410.Reporting
{
    /// <summary>
    /// Represents a measurement report frame with basic information only.
    /// </summary>
    public class BasicReportFrame : ReportFrame
    {
        /// <summary>
        /// Gets a value indicating the state of the target.
        /// </summary>
        public TargetState TargetState { get; internal set; }

        /// <summary>
        /// Gets the distance of the moving target.
        /// </summary>
        public Length MovementTargetDistance { get; internal set; }

        /// <summary>
        /// Gets the moving target energy level.
        /// </summary>
        public byte MovementTargetEnergy { get; internal set; }

        /// <summary>
        /// Gets the distance of the stationary target.
        /// </summary>
        public Length StationaryTargetDistance { get; internal set; }

        /// <summary>
        /// Gets the stationary target energy level.
        /// </summary>
        public byte StationaryTargetEnergy { get; internal set; }

        /// <summary>
        /// Gets the distance at which the target was detected.
        /// </summary>
        public Length DetectionDistance { get; internal set; }

        internal BasicReportFrame()
        {
            DataType = ReportingType.BasicMode;
        }
    }
}
