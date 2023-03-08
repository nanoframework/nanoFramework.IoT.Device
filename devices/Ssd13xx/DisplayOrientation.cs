// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ssd13xx
{
    /// <summary>
    /// Display orientation. No all display drivers support every orientation.
    /// </summary>
    public enum DisplayOrientation : int
    {
        /// <summary>
        /// Portrait.
        /// </summary>
        Portrait = 1,

        /// <summary>
        /// Landscape.
        /// </summary>
        Landscape = 2,

        /// <summary>
        /// Portrait 180.
        /// </summary>
        Portrait180 = 3,

        /// <summary>
        /// Landscape 180.
        /// </summary>
        Landscape180 = 4
    }
}
