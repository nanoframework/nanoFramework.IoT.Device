// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Runtime light entity.
    /// </summary>
    public sealed class HomeAssistantLight : HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantLight" /> class.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="initialState">Initial light state value.</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        public HomeAssistantLight(
            HomeAssistantDiscoveryEntity discovery,
            string initialState,
            HomeAssistantPublishDelegate publisher)
        {
            Initialize(discovery, initialState, publisher);
        }
    }
}
