// Copyright (c) .NET Foundation and Contributors
// Portions Copyright (c) Microsoft Corporation.  All rights reserved.
// See LICENSE file in the project root for full license information.

namespace Iot.Device.XPT2046
{
    /// <summary>
    /// A touch point.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Gets or sets X.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the amount of pressure of the touch.
        /// </summary>
        public int Weight { get; set; }
    }
}