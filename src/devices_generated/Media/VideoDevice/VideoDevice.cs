// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;

namespace Iot.Device.Media
{
    /// <summary>
    /// The communications channel to a video device.
    /// </summary>
    public abstract partial class VideoDevice : IDisposable
    {
        /// <summary>
        /// Create a communications channel to a video device running on Unix.
        /// </summary>
        /// <param name="settings">The connection settings of a video device.</param>
        /// <returns>A communications channel to a video device running on Unix.</returns>
        public static VideoDevice Create(VideoConnectionSettings settings) => new UnixVideoDevice(settings);

        /// <summary>
        /// Path to video resources located on the platform.
        /// </summary>
        public abstract string DevicePath { get; set; }

        /// <summary>
        /// The connection settings of the video device.
        /// </summary>
        public abstract VideoConnectionSettings Settings { get; }

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <param name="path">Picture save path.</param>
        public abstract void Capture(string path);

        /// <summary>
        /// Capture a picture from the video device.
        /// </summary>
        /// <returns>Picture stream.</returns>
        public abstract Stream Capture();

        /// <summary>
        /// Start continuous capture
        /// </summary>
        public abstract void StartCaptureContinuous();

        /// <summary>
        /// The continuous capture stream
        /// </summary>
        /// <returns></returns>
        public abstract Stream CaptureContinuous();

        /// <summary>
        /// Stop the continuous capture
        /// </summary>
        public abstract void StopCaptureContinuous();

        /// <summary>
        /// Query controls value from the video device.
        /// </summary>
        /// <param name="type">The type of a video device's control.</param>
        /// <returns>The default and current values of a video device's control.</returns>
        public abstract VideoDeviceValue GetVideoDeviceValue(VideoDeviceValueType type);

        /// <summary>
        /// Get all the pixel formats supported by the device.
        /// </summary>
        /// <returns>Supported pixel formats.</returns>
        public abstract IEnumerable<PixelFormat> GetSupportedPixelFormats();

        /// <summary>
        /// Get all the resolutions supported by the specified pixel format.
        /// </summary>
        /// <param name="format">Pixel format.</param>
        /// <returns>Supported resolution.</returns>
        public abstract IEnumerable<Resolution> GetPixelFormatResolutions(PixelFormat format);

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the VideoDevice and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
