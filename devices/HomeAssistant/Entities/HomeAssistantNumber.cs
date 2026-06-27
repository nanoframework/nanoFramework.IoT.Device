// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Runtime number entity for numeric values.
    /// </summary>
    public sealed class HomeAssistantNumber : HomeAssistantRuntimeEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HomeAssistantNumber" /> class.
        /// </summary>
        /// <param name="discovery">Discovery entity definition.</param>
        /// <param name="initialState">Initial numeric value as string.</param>
        /// <param name="publisher">Callback to publish MQTT messages.</param>
        public HomeAssistantNumber(
            HomeAssistantDiscoveryEntity discovery,
            string initialState,
            HomeAssistantPublishDelegate publisher)
        {
            Initialize(discovery, initialState, publisher);
        }

        /// <summary>
        /// Gets the numeric value, or 0 if state is not a valid number.
        /// </summary>
        public int Value
        {
            get
            {
                if (int.TryParse(State, out int result))
                {
                    return result;
                }

                return 0;
            }
        }

        /// <summary>
        /// Sets the numeric value.
        /// </summary>
        public void SetValue(int value)
        {
            SetState(value.ToString());
        }
    }
}
