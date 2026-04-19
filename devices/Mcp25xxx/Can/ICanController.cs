// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Mcp25xxx.Can
{
    /// This interface is used to abstract the CAN controller
    public interface ICanController
    {
        /// <summary>
        /// Get next <see cref="CanMessage"/> available in the _<see cref="CanController"/> internal buffer.
        /// If there are no more messages available null will be returned.
        /// </summary>
        /// <returns>
        /// A <see cref="CanMessage"/> or null if there are no more messages available.
        /// </returns>
        CanMessage GetMessage();

        /// <summary>
        /// Clear all receive buffers.
        /// </summary>
        void Reset();

        /// <summary>
        /// Initializes the CAN controller.
        /// </summary>
        /// <param name="mcp25xxx"></param>
        void Initialize(Mcp25xxx mcp25xxx);

        /// <summary>
        /// Set the <paramref name="baudRate"/> and <paramref name="clockFrequency"/> of the CAN controller.
        /// </summary>
        /// <param name="baudRate">CAN baudrate. For example 250.000.</param>
        /// <param name="clockFrequency">mcp2515 clock frequency. For example 8.000.000.</param>
        void SetBitRate(int baudRate, int clockFrequency);

        /// <summary>
        /// Write message to CAN Bus.
        /// </summary>
        /// <param name="message">CAN mesage to write in CAN Bus.</param>
        void WriteMessage(CanMessage message);

        /// <summary>
        /// Set the filter for the CAN controller to only receive messages of that Id.
        /// </summary>
        /// <param name="num">The number of the filter. 6 is the maximum.</param>
        /// <param name="ext">Has extended Id.</param>
        /// <param name="ulData">The Id you want to filter on.</param>
        /// <returns></returns>
        bool SetFilter(RXF num, bool ext, uint ulData);
    }
}