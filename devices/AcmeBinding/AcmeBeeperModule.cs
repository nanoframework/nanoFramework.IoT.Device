// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Device.Model;

namespace Iot.Device.AcmeBinding
{
    /// <summary>
    /// Synthetic sub-device exposed as a <see cref="ComponentAttribute" /> of <see cref="AcmeDevice" />,
    /// used to exercise nested/composed interfaces (no real binding in this repo uses [Component] yet).
    /// </summary>
    [Interface("Acme synthetic beeper module")]
    public class AcmeBeeperModule
    {
        private bool _isBeeping;

        /// <summary>
        /// Gets a value indicating whether the beeper is currently sounding.
        /// </summary>
        [Telemetry]
        public bool IsBeeping => _isBeeping;

        /// <summary>
        /// Start sounding the beeper.
        /// </summary>
        /// <param name="durationMilliseconds">How long the beeper should sound for, in milliseconds.</param>
        [Command]
        public void Beep(int durationMilliseconds)
        {
            _isBeeping = true;
        }

        /// <summary>
        /// Stop sounding the beeper.
        /// </summary>
        [Command]
        public void StopBeep()
        {
            _isBeeping = false;
        }
    }
}
