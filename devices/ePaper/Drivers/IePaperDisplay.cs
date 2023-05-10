// Copyright (c) 2022 The nanoFramework project contributors
// See LICENSE file in the project root for full license information.

using System;
using System.Drawing;

using Iot.Device.EPaper.Buffers;
using nanoFramework.UI;

namespace Iot.Device.EPaper.Drivers
{
    /// <summary>
    /// Represents an ePaper Display device.
    /// </summary>
    public interface IEPaperDisplay : IDisposable
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
        /// Gets a value indicating whether drawing to a frame is paged to save on memory space.
        /// </summary>
        bool PagedFrameDrawEnabled { get; }

        /// <summary>
        /// Clears the display frame buffer by resetting it to its default state.
        /// </summary>
        /// <param name="triggerPageRefresh"><see langword="true"/> to also flush the buffer to the display and trigger the refresh sequence.</param>
        void Clear(bool triggerPageRefresh = false);

        /// <summary>
        /// Flushes the content of the internal frame buffer to the display.
        /// </summary>
        /// <remarks>
        /// You will only need to use this method if you have disabled frame paged draws.
        /// </remarks>
        void Flush();

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
        /// Draws a single pixel to the appropriate frame buffer based on the specified color.
        /// </summary>
        /// <param name="x">The X Position.</param>
        /// <param name="y">The Y Position.</param>
        /// <param name="color">Pixel color. See the remarks for how a buffer is selected based on the color value.</param>
        void DrawPixel(int x, int y, Color color);

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

        /// <summary>
        /// Begins a frame draw operation with frame paging support.
        /// <code>
        /// BeginFrameDraw();
        /// do {
        ///     // Drawing calls
        /// } while (NextFramePage());
        /// EndFrameDraw();
        /// </code>
        /// </summary>
        void BeginFrameDraw();

        /// <summary>
        /// Moves the current buffers to the next frame page and returns true if successful.
        /// </summary>
        /// <returns>True if the next frame page is available and the internal buffers have moved to it, otherwise; false.</returns>
        bool NextFramePage();

        /// <summary>
        /// Ends the frame draw and flushes the current page to the device.
        /// </summary>
        void EndFrameDraw();
    }
}
