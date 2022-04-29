﻿using System;

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Options for the terminator to be appended to the data.
    /// </summary>
    public enum Terminator
    {
        /// <summary>
        /// No terminator.
        /// </summary>
        None,

        /// <summary>
        /// Carriage Return plus Line Feed.
        /// </summary>
        CrLf,

        /// <summary>
        /// Carriage Return.
        /// </summary>
        Cr,

        /// <summary>
        /// Tab.
        /// </summary>
        Tab,

        /// <summary>
        /// A double Carriage Return.
        /// </summary>
        DoubleCr,

        /// <summary>
        /// Double Line Feed.
        /// </summary>
        DoubleClLf
    }
}
