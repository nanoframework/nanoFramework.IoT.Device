// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Eclo Solutions
// See LICENSE file in the project root for full license information.

namespace Iot.Device.Swarm
{
    /// <summary>
    /// Information about GPS jamming and spoofing.
    /// </summary>
    public class JammingSpoofingIndication
    {
        /// <summary>
        /// Gets information about the spoof state.
        /// </summary>
        /// <remarks>
        /// 0: Spoofing unknown or deactivated.
        /// 1: No spoofing indicated.
        /// 2: Spoofing indicated.
        /// 3: Multiple spoofing indications.
        /// </remarks>
        public byte SpoofState { get; internal set; } = 0;

        /// <summary>
        /// Gets indication of how much carrier wave (CW) jamming is detected.
        /// </summary>
        /// <remarks>
        /// Ranges from 0 to 255. Being 0 no CW jamming and 255 strong CW jamming.
        /// </remarks>
        public byte JammingLevel { get; internal set; } = 0;
    }
}
