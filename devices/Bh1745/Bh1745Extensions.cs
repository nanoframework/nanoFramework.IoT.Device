// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Bh1745
{
    /// <summary>
    /// Extension methods for the Bh1745 sensor.
    /// </summary>
    public static class Bh1745Extensions
    {
        /// <summary>
        /// Converts the enum Measurement time to an integer representing the measurement time in ms.
        /// </summary>
        /// <param name="time">The MeasurementTime.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a not supported MeasurementTime is used.</exception>
        /// <returns>Value in miliseconds.</returns>
        public static int ToMilliseconds(this MeasurementTime time)
        {
            switch (time)
            {
                case MeasurementTime.Ms160:
                    return 160;
                case MeasurementTime.Ms320:
                    return 320;
                case MeasurementTime.Ms640: 
                    return 640;
                case MeasurementTime.Ms1280: 
                    return 1280;
                case MeasurementTime.Ms2560: 
                    return 2560;
                case MeasurementTime.Ms5120: 
                    return 5120;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts the enum Measurement time to a TimeSpan.
        /// </summary>
        /// <param name="bh1745">The BH1745 device.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when a not supported MeasurementTime is used.</exception>
        /// <returns>Time as TimeSpan object.</returns>
        public static TimeSpan MeasurementTimeAsTimeSpan(this Bh1745 bh1745) => new TimeSpan(bh1745.MeasurementTime.ToMilliseconds());
    }
}
