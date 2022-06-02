// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// This is a class added speifically for nanoFramework so we dont need to include the full fat ImageSharp lib (as it is only properties)!

using System;

namespace Iot.Device.CharacterLcd
{
    /// <summary>
    /// Stores an ordered pair of integers, which specify a height and width.
    /// </summary>
    /// <remarks>
    /// This struct is fully mutable. This is done (against the guidelines) for the sake of performance,
    /// as it avoids the need to create new values for modification operations.
    /// </remarks>
    public struct Size
    {
        /// <summary>
        /// Gets or sets the height of this <see cref="Size"/>.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets or sets the width of this <see cref="Size"/>.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Size"/> struct.
        /// </summary>
        /// <param name="width">The width of the size.</param>
        /// <param name="height">The height of the size.</param>
        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}
