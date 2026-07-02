// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Publishes an MQTT message for Home Assistant topics.
    /// </summary>
    /// <param name="topic">MQTT topic to publish.</param>
    /// <param name="payload">Payload to publish.</param>
    /// <param name="retain">Whether MQTT retain flag is enabled.</param>
    public delegate void HomeAssistantPublishDelegate(string topic, string payload, bool retain);
}
