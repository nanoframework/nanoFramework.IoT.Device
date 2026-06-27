// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Runtime switch entity for binary on/off control.
    /// </summary>
    public sealed class HomeAssistantSwitch : HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantSwitch" /> class.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="initialState">Initial state (ON or OFF).</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        public HomeAssistantSwitch(
            HomeAssistantDiscoveryEntity discovery,
            string initialState,
            HomeAssistantPublishDelegate publisher)
        {
            Initialize(discovery, initialState, publisher);
        }

        /// <summary>
        /// Gets a value indicating whether the switch is ON.
        /// </summary>
        public bool IsOn
        {
            get { return State == "ON"; }
        }

        /// <summary>
        /// Sets the switch to ON or OFF state.
        /// </summary>
        public void SetOn(bool on)
        {
            PublishState(on ? "ON" : "OFF");
        }
    }
}
