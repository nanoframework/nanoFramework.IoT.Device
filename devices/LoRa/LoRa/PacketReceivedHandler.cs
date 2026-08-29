// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.LoRa
{
    /// <summary>
    /// <para>Represents the method that will handle packet received events.</para>
    /// <para>.NET nanoFramework does not support generic delegates such as <c>EventHandler&lt;T&gt;</c>, so a custom non-generic delegate is used instead.</para>
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="message">The received message.</param>
    public delegate void PacketReceivedHandler(object sender, LoRaMessage message);
}
