// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Ft6xx6x
{
    /// <summary>
    /// The touch coordinates rotation relative to the default orientation.
    /// </summary>
    public enum Rotation
    {
        /// <summary>
        /// Default orientation.
        /// </summary>
        None = 0,
        /// <summary>
        /// Rotate left.
        /// </summary>
        Left = 1,
        /// <summary>
        /// Rotate right.
        /// </summary>
        Right = 2,
        /// <summary>
        /// Invert / flip diagonally.
        /// </summary>
        Invert = 3
    }
}
