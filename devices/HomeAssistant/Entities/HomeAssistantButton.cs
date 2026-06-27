// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Runtime button entity.
    /// </summary>
    public sealed class HomeAssistantButton : HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantButton" /> class.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        public HomeAssistantButton(
            HomeAssistantDiscoveryEntity discovery,
            HomeAssistantPublishDelegate publisher)
        {
            Initialize(discovery, string.Empty, publisher);
        }
    }
}
