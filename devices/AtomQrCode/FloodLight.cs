using System;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Options for controlling the reader flood light.
    /// </summary>
    public enum FloodLight
    {
        /// <summary>
        /// Flood light on when reading.
        /// </summary>
        OnWhenReading,

        /// <summary>
        /// Flood light always on.
        /// </summary>
        AlwaysOn,

        /// <summary>
        /// Flood light always off.
        /// </summary>
        AlwaysOff
    }
}
