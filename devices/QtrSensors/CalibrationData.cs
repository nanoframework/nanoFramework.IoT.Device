// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.QtrSensors
{
    /// <summary>
    /// Contains calibration data for a channel.
    /// </summary>
    public class CalibrationData
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        public int MinimumValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        public int MaximumValue { get; set; }
    }
}
