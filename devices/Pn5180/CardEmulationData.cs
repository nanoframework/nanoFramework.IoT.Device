// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pn5180
{
    /// <summary>
    /// Holds the result of a successful Autocoll activation during
    /// card emulation mode.
    /// </summary>
    /// <remarks>
    /// After the PN5180 enters Autocoll mode and an external reader
    /// activates the device, this class provides the activated protocol
    /// and the first data frame received from the reader (e.g. RATS
    /// for ISO-DEP, ATR_REQ for NFC-DEP).
    /// </remarks>
    public class CardEmulationData
    {
        /// <summary>
        /// Creates a new <see cref="CardEmulationData"/> instance.
        /// </summary>
        /// <param name="activatedProtocol">The NFC technology used by the external reader.</param>
        /// <param name="rxData">The first data frame received from the reader after activation, or null if none.</param>
        public CardEmulationData(TargetProtocol activatedProtocol, byte[] rxData)
        {
            ActivatedProtocol = activatedProtocol;
            RxData = rxData;
        }

        /// <summary>
        /// Gets the NFC technology that the external reader used to
        /// activate the PN5180 during Autocoll.
        /// </summary>
        /// <remarks>
        /// Derived from RF_STATUS register (0x1D) bits [16:14].
        /// </remarks>
        public TargetProtocol ActivatedProtocol { get; }

        /// <summary>
        /// Gets the first data frame received from the external reader
        /// after the card emulation activation completed.
        /// </summary>
        /// <remarks>
        /// For NFC-A ISO-DEP this is typically the RATS command.
        /// For NFC-DEP this is typically the ATR_REQ.
        /// May be null if no data was available after activation.
        /// </remarks>
        public byte[] RxData { get; }
    }
}
