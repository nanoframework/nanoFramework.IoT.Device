// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn532.AsTarget
{
    /// <summary>
    /// Information on the initialized target
    /// </summary>
    public class AsTargetInitialized
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modeInialized">Target mode initialized</param>
        /// <param name="initiator">The initiator bytes</param>
        public AsTargetInitialized(TargetModeInitialized modeInialized, byte[] initiator)
        {
            ModeInialized = modeInialized;
            Initiator = initiator;
        }

        /// <summary>
        /// Target mode initialized
        /// </summary>
        public TargetModeInitialized ModeInialized { get; set; }

        /// <summary>
        /// Initiator bytes
        /// </summary>
        public byte[] Initiator { get; set; }
    }
}
