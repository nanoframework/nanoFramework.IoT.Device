// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Bmi270
{
    /// <summary>
    /// Activity type recognized by the BMI270 activity recognition feature.
    /// Read from the WR_GEST_ACT register (0x20), bits [3:0].
    /// </summary>
    public enum ActivityType : byte
    {
        /// <summary>Device is stationary.</summary>
        Still = 0x00,

        /// <summary>User is walking.</summary>
        Walking = 0x01,

        /// <summary>User is running.</summary>
        Running = 0x02,

        /// <summary>Activity could not be determined.</summary>
        Unknown = 0x03,
    }
}
