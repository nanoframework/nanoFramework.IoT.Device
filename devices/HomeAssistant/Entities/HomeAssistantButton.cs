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

        /// <summary>
        /// Buttons are stateless commands; every command payload should trigger.
        /// </summary>
        /// <param name="oldState">Previous state value.</param>
        /// <param name="newState">Incoming state value.</param>
        /// <param name="changed">True when incoming state differs from previous.</param>
        /// <returns>Always <c>true</c> for button command handling.</returns>
        protected override bool ShouldNotifyOnSetState(string oldState, string newState, bool changed)
        {
            return true;
        }
    }
}
