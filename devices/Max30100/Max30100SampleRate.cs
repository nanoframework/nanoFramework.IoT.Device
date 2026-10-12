// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Max30100
{
    /// <summary>The MAX30100 sample rate.</summary>
    public enum Max30100SampleRate : byte
    {
        /// <summary>50 samples per second.</summary>
        SamplesPerSecond50 = 0,

        /// <summary>100 samples per second.</summary>
        SamplesPerSecond100 = 1,

        /// <summary>167 samples per second.</summary>
        SamplesPerSecond167 = 2,

        /// <summary>200 samples per second.</summary>
        SamplesPerSecond200 = 3,

        /// <summary>400 samples per second.</summary>
        SamplesPerSecond400 = 4,

        /// <summary>600 samples per second.</summary>
        SamplesPerSecond600 = 5,

        /// <summary>800 samples per second.</summary>
        SamplesPerSecond800 = 6,

        /// <summary>1000 samples per second.</summary>
        SamplesPerSecond1000 = 7,
    }
}
