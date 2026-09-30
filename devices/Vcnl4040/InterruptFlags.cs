// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Vcnl4040
{
    /// <summary>Contains the interrupt flags read from the VCNL4040.</summary>
    public class InterruptFlags
    {
        /// <summary>Initializes a new instance of the <see cref="InterruptFlags"/> class.</summary>
        /// <param name="psProtectionMode">The psProtectionMode value.</param>
        /// <param name="alsLow">The alsLow value.</param>
        /// <param name="alsHigh">The alsHigh value.</param>
        /// <param name="psClose">The psClose value.</param>
        /// <param name="psAway">The psAway value.</param>
        public InterruptFlags(bool psProtectionMode, bool alsLow, bool alsHigh, bool psClose, bool psAway)
        {
            PsProtectionMode = psProtectionMode;
            AlsLow = alsLow;
            AlsHigh = alsHigh;
            PsClose = psClose;
            PsAway = psAway;
        }

        /// <summary>Gets a value indicating whether the undocumented proximity protection flag is set.</summary>
        public bool PsProtectionMode { get; private set; }

        /// <summary>Gets a value indicating whether the ambient light low-threshold flag is set.</summary>
        public bool AlsLow { get; private set; }

        /// <summary>Gets a value indicating whether the ambient light high-threshold flag is set.</summary>
        public bool AlsHigh { get; private set; }

        /// <summary>Gets a value indicating whether the proximity close-event flag is set.</summary>
        public bool PsClose { get; private set; }

        /// <summary>Gets a value indicating whether the proximity away-event flag is set.</summary>
        public bool PsAway { get; private set; }
    }
}
