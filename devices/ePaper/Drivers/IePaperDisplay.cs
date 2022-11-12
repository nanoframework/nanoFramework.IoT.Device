// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;

using Iot.Device.ePaper.Buffers;

namespace Iot.Device.ePaper.Drivers
{
    /// <summary>
    /// Represents an ePaper Display device.
    /// </summary>
    public interface IePaperDisplay : IDisposable
    {
        /// <summary>
        /// Gets the width of the drawable area on the display.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the height of the drawable area on the display.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the frame buffer.
        /// </summary>
        /// <remarks>
        /// This frame buffer instance could be pointing to a portion of the full display frame
        /// as a measure to reduce memory footprint. See the ctor documentation for more info.
        /// </remarks>
        IFrameBuffer FrameBuffer { get; }

        /// <summary>
        /// Clears the display frame buffer by resetting it to its default state.
        /// </summary>
        /// <param name="triggerPageRefresh"><see langword="true"/> to also flush the buffer to the display and trigger the refresh sequence.</param>
        void Clear(bool triggerPageRefresh = false);

        /// <summary>
        /// Initiates the full refresh sequence on the display.
        /// </summary>
        void PerformFullRefresh();

        /// <summary>
        /// Initiates the partial refresh sequence on the display if the panel supports it.
        /// </summary>
        void PerformPartialRefresh();

        /// <summary>
        /// Sets the drawing position on the display.
        /// </summary>
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        void SetPosition(int x, int y);

        /// <summary>
        /// Sends a command byte(s) to the display.
        /// </summary>
        /// <param name="command">The command bytes to send. See the command enum within the driver implementation and consult the datasheet.</param>
        void SendCommand(params byte[] command);

        /// <summary>
        /// Send frame data to the display.
        /// </summary>
        /// <param name="data">The frame data to send.</param>
        void SendData(params byte[] data);

        /// <summary>
        /// Blocks the current thread until the display is in idle mode again.
        /// </summary>
        void WaitReady();
    }
}
