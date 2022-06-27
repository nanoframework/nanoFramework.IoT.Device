using System;

namespace Iot.Device.A4988
{
    /// <summary>
    /// Enum for microsteps settings
    /// </summary>
    public enum Microsteps : byte
    {
        /// <summary>
        /// No microsteps (full step)
        /// </summary>
        FullStep = 1,

        /// <summary>
        /// 1/2
        /// </summary>
        HalfStep = 2,
        /// <summary>
        /// 1/4
        /// </summary>
        QuaterStep = 4,
        /// <summary>
        /// 1/8
        /// </summary>
        EightStep = 8,
        /// <summary>
        /// 1/16
        /// </summary>
        SisteenthStep = 16
    }
}
