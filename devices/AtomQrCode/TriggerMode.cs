// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.AtomQrCode
{
    /// <summary>
    /// Trigger mode for the scanner.
    /// </summary>
    public enum TriggerMode
    {
        /// <summary>
        /// No scan mode selected.
        /// </summary>
        None = 0,

        /// <summary>
        /// Host mode. Scanning has to be started and stopped programatically.
        /// </summary>
        Host,

        /// <summary>
        /// Press a button to start the reading and release the button to stop reading.
        /// Reading will also stop on successful reading or upon successful reading timeout.
        /// </summary>
        KeyHold,

        /// <summary>
        /// Press a button momentarily to start reading. Press it again momentarily to stop reading.
        /// Reading will also stop on successful reading or upon successful reading timeout.
        /// </summary>
        KeyPress,

        /// <summary>
        /// The reader will start decoding continuously.
        /// </summary>
        Continuous,

        /// <summary>
        /// The scan engine will detect the brightness of the surrounding area.
        /// A reading will be triggered when the brightness changes.
        /// </summary>
        Autosensing
    }
}
