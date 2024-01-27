// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.Ssd13xx.Commands.Ssd1306Commands
{
    /// <summary>
    /// Represents ContinuousVerticalAndHorizontalScrollSetup command.
    /// </summary>
    public class ContinuousVerticalAndHorizontalScrollSetup : ISsd1306Command
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContinuousVerticalAndHorizontalScrollSetup" /> class.
        /// This command consists of 6 consecutive bytes to set up the continuous vertical
        /// scroll parameters and determines the scrolling start page, end page, scrolling
        /// speed and vertical scrolling offset.
        /// </summary>
        /// <param name="scrollType">Vertical/Horizontal scroll type.</param>
        /// <param name="startPageAddress">Start page address with a range of 0-7.</param>
        /// <param name="frameFrequencyType">Frame frequency type with a range of 0-7.</param>
        /// <param name="endPageAddress">End page address with a range of 0-7.</param>
        /// <param name="verticalScrollingOffset">Vertical scrolling offset with a range of 0-63.</param>
        public ContinuousVerticalAndHorizontalScrollSetup(
            VerticalHorizontalScrollType scrollType,
            PageAddress startPageAddress,
            FrameFrequencyType frameFrequencyType,
            PageAddress endPageAddress,
            byte verticalScrollingOffset)
        {
            if (verticalScrollingOffset > 0x3F)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalScrollingOffset));
            }

            ScrollType = scrollType;
            StartPageAddress = startPageAddress;
            FrameFrequencyType = frameFrequencyType;
            EndPageAddress = endPageAddress;
            VerticalScrollingOffset = verticalScrollingOffset;
        }

        /// <summary>
        /// Gets the value that represents the command.
        /// </summary>
        public byte Id => (byte)ScrollType;

        /// <summary>
        /// Gets Vertical/Horizontal scroll type.
        /// </summary>
        public VerticalHorizontalScrollType ScrollType { get; }

        /// <summary>
        /// Gets start page address with a range of 0-7.
        /// </summary>
        public PageAddress StartPageAddress { get; }

        /// <summary>
        /// Gets frame frequency type with a range of 0-7.
        /// </summary>
        public FrameFrequencyType FrameFrequencyType { get; }

        /// <summary>
        /// Gets end page address with a range of 0-7.
        /// </summary>
        public PageAddress EndPageAddress { get; }

        /// <summary>
        /// Gets vertical scrolling offset with a range of 0-63.
        /// </summary>
        public byte VerticalScrollingOffset { get; }

        /// <summary>
        /// Gets the bytes that represent the command.
        /// </summary>
        /// <returns>The bytes that represent the command.</returns>
        public byte[] GetBytes()
        {
            return new byte[] { Id, 0x00, (byte)StartPageAddress, (byte)FrameFrequencyType, (byte)EndPageAddress, VerticalScrollingOffset };
        }

        /// <summary>
        /// Vertical and horizontal scroll.
        /// </summary>
        public enum VerticalHorizontalScrollType
        {
            /// <summary>
            /// Vertical and right horizontal scroll.
            /// </summary>
            Right = 0x29,

            /// <summary>
            /// Vertical and left horizontal scroll.
            /// </summary>
            Left = 0x2A
        }
    }
}
