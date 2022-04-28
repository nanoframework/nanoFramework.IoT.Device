using System;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Options for controlling the reader positioning light.
    /// </summary>
    public enum Positioninglight
    {
        /// <summary>
        /// Positioning light on when reading.
        /// </summary>
        OnWhenReading,

        /// <summary>
        /// Positioning light always on.
        /// </summary>
        AlwaysOn,

        /// <summary>
        /// Positioning light always off.
        /// </summary>
        AlwaysOff
    }
}
