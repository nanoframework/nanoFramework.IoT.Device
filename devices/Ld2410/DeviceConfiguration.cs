// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ld2410
{
    /// <summary>
    /// Defines the configurations for a <see cref="Ld2410"/> device.
    /// </summary>
    public sealed class DeviceConfiguration
    {
        /// <summary>
        /// The maximum allowd 'No-One' duration in seconds.
        /// </summary>
        public const ushort MaxSupportedNoOneDuration = ushort.MaxValue;

        private TimeSpan noOneDuration;

        /// <summary>
        /// Gets the max gate index value of distance gates available on the radar module.
        /// </summary>
        /// <remarks>
        /// Gate numbers start from 0. So a radar reporting <see cref="MaxDistanceGateIndex"/> of 8 means it has 9 gates.
        /// Each gate covers a distance of 75cm.
        /// If a module reports max distance gate of 8, then it covers 9x75=675cm or 6.75m.
        /// </remarks>
        public byte MaxDistanceGateIndex { get; internal set; }

        /// <summary>
        /// Gets the number of available distance gates on the radar module.
        /// </summary>
        public int NumberOfDistanceGatesAvailable => MaxDistanceGateIndex + 1;

        /// <summary>
        /// Gets or sets the fathest detectable distance of moving targets.
        /// The distance is measured by number of radar distance gates where each gate is equal 
        /// to 75cm.
        /// </summary>
        public byte MaximumMovementDetectionDistanceGate { get; set; }

        /// <summary>
        /// Gets or sets the fatherst detectable distance of static objects.
        /// The distance is measured by number of radar distance gates where each gate is equal
        /// to 75cm.
        /// </summary>
        public byte MaximumRestingDetectionDistanceGate { get; set; }

        /// <summary>
        /// Gets or sets the duration to wait before no movement is confirmed.
        /// </summary>
        public TimeSpan NoOneDuration
        {
            get => noOneDuration;
            set
            {
                if (value.TotalSeconds < 0 || value.TotalSeconds > MaxSupportedNoOneDuration)
                {
                    throw new ArgumentOutOfRangeException();
                }

                noOneDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets the serial port baud rate.
        /// </summary>
        public int BaudRate { get; set; } = -1;

        /// <summary>
        /// Gets or sets all available gate configurations.
        /// </summary>
        public GateConfiguration[] GateConfiguration { get; set; }
    }
}
