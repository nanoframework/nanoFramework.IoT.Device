// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

using UnitsNet;

namespace Iot.Device.Ld2410
{
    /// <summary>
    /// Defines a specific gate's sensitivity configurations.
    /// </summary>
    public sealed class GateConfiguration
    {
        /// <summary>
        /// The distance, in centimeters, per each radar distance gate.
        /// </summary>
        public static readonly Length DistancePerGateInCm = Length.FromCentimeters(75);

        private Ratio _restSensitivity;

        /// <summary>
        /// Gets the gate number of this configuration instance.
        /// </summary>
        public byte Gate { get; }

        /// <summary>
        /// Gets or sets the motion sensitivity threshold. Any target with a movement "energy" greater than the specified range will be detected, otherwise it will be ignored.
        /// Setting a gate's sensitivity to 100 will effectively disable detection of movement within that distance gate.
        /// This can enable fine control over the range of distance to detect movements in. 
        /// For example, All gates except 4 and 7 are set to 100. This means movement will only be detected in gates 4 and 5 (between 3 meters and 5.25 meters from the radar).
        /// </summary>
        /// <remarks>There is no defined unit of measurement for this and should be treated as a percentage. It is recommended to experiment with the values to find the right threshold for the use-case at hand.</remarks>
        public Ratio MotionSensitivity { get; set; }

        /// <summary>
        /// Gets or sets the resting sensitivity threshold. Any target without movement "energy" (resting) greater than the specified range will be detected, otherwise it will be ignored.
        /// Setting a gate's sensitivity to 100 will effectively disable detection of resting targets within that distance gate.
        /// This can enable fine control over the range of distance to detect resting targets in. 
        /// For example, All gates except 4 and 7 are set to 100. This means resting targets will only be detected in gates 4 and 5 (between 3 meters and 5.25 meters from the radar).
        /// </summary>
        /// <remarks>There is no defined unit of measurement for this and should be treated as a percentage. It is recommended to experiment with the values to find the right threshold for the use-case at hand.</remarks>
        /// <exception cref="InvalidOperationException">Cannot set this value for Gate 0 and 1.</exception>
        public Ratio RestSensitivity 
        { 
            get => _restSensitivity; 
            set
            {
                if (Gate == 0 || Gate == 1)
                {
                    throw new InvalidOperationException();
                }

                _restSensitivity = value;
            }
        }

        /// <summary>
        /// Gets the detection distance of this gate.
        /// </summary>
        public Length DetectionDistance
            => Length.FromCentimeters(Gate * DistancePerGateInCm.Centimeters);

        /// <summary>
        /// Initializes a new instance of the <see cref="GateConfiguration"/> class with the specified gate number.
        /// </summary>
        /// <param name="gate">The gate number to initialize the <see cref="GateConfiguration"/> class for.</param>
        public GateConfiguration(byte gate)
        {
            Gate = gate;
        }

#if DEBUG
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Gate: {Gate}, Motion Sensitivity: {MotionSensitivity}, Rest Sensitivity: {RestSensitivity}";
        }
#endif
    }
}
