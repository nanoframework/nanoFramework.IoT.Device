// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.LoRa
{
    /// <summary>
    /// <para>Abstraction for a LoRa radio transceiver.</para>
    /// <para>Implement this interface for each chip variant (SX1262, SX1276, RFM95, and similar).</para>
    /// </summary>
    public interface ILoRaDevice : IDisposable
    {
        /// <summary>
        /// <para>Performs a hardware reset and waits for the chip to become ready.</para>
        /// <para>Must be the first call after construction.</para>
        /// </summary>
        void Reset();

        /// <summary>
        /// Applies the full initialization sequence. Call once after <see cref="ILoRaDevice.Reset" />.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Sets the RF carrier frequency in Hz (for example 868000000).
        /// </summary>
        /// <param name="frequencyHz">Carrier frequency in hertz.</param>
        void SetRfFrequency(uint frequencyHz);

        /// <summary>
        /// <para>Sends <paramref name="payload" /> over LoRa, blocking until TxDone or until <paramref name="timeoutMs" /> elapses.</para>
        /// <para>Callers must supply a non-null, non-empty payload within the implementation's maximum length (for <see cref="Iot.Device.LoRa.Drivers.Sx1262.Sx1262" />, see <see cref="Iot.Device.LoRa.Drivers.Sx1262.Sx1262.MaxPayloadLength" />).</para>
        /// <para>Do not call from <see cref="ILoRaDevice.PacketReceived" /> (the poll thread); implementations throw <see cref="InvalidOperationException" /> if invoked on that thread.</para>
        /// </summary>
        /// <param name="payload">The bytes to transmit.</param>
        /// <param name="timeoutMs">Maximum time to wait for completion, in milliseconds. Must be greater than zero.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="payload" /> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="payload" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="payload" /> exceeds the device maximum or <paramref name="timeoutMs" /> is not positive.</exception>
        /// <exception cref="TimeoutException">Thrown when TX does not complete in time.</exception>
        /// <exception cref="InvalidOperationException">Thrown when called from the RX poll thread.</exception>
        void Send(byte[] payload, int timeoutMs);

        /// <summary>
        /// <para>Starts a background thread that polls for incoming packets and raises <see cref="ILoRaDevice.PacketReceived" /> for each valid frame.</para>
        /// <para>Wire up <see cref="ILoRaDevice.PacketReceived" /> before calling this method.</para>
        /// </summary>
        void StartPolling();

        /// <summary>
        /// <para>Signals the poll thread to stop. When called from another thread, blocks until the poll thread exits.</para>
        /// <para>When called from the poll thread itself (for example from <see cref="ILoRaDevice.PacketReceived" />), returns without blocking; the thread then exits.</para>
        /// </summary>
        void StopPolling();

        /// <summary>
        /// Raised on the poll thread when a valid packet is received.
        /// </summary>
        event PacketReceivedHandler PacketReceived;
    }
}
