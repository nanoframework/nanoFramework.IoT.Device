// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ld2410.Reporting
{
    /// <summary>
    /// Gate static and movement energy level
    /// </summary>
    public readonly struct GateEnergy
    {
        /// <summary>
        /// Creates a new instance of <see cref="GateEnergy"/>.
        /// </summary>
        /// <param name="movementEnergy">The energy level of the movement target in this distance gate.</param>
        /// <param name="staticEnergy">The energy level of the static target in this distance gate.</param>
        public GateEnergy(byte movementEnergy, byte staticEnergy)
        {
            MovementEnergy = movementEnergy;
            StaticEnergy = staticEnergy;
        }

        /// <summary>
        /// Gets the movement energy in this gate.
        /// </summary>
        public byte MovementEnergy { get; }

        /// <summary>
        /// Gets the static energy in this gate.
        /// </summary>
        public byte StaticEnergy { get; }
    }
}
