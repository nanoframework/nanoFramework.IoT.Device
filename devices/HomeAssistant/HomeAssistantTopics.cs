// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Standard Home Assistant MQTT topic constants and helpers.
    /// </summary>
    public static class HomeAssistantTopics
    {
        /// <summary>
        /// Standard discovery prefix used by Home Assistant for MQTT discovery.
        /// Default: "homeassistant".
        /// </summary>
        public const string DiscoveryPrefix = "homeassistant";

        /// <summary>
        /// Standard Home Assistant status/birth topic where HA publishes online/offline events.
        /// Default: "homeassistant/status".
        /// </summary>
        public const string StatusTopic = "homeassistant/status";

        /// <summary>
        /// Generates the availability (LWT) topic for a given root topic.
        /// Example: topicRoot="home/sprinkler" → returns "home/sprinkler/availability".
        /// </summary>
        /// <param name="topicRoot">The root topic path (e.g., "home/sprinkler").</param>
        /// <returns>The availability topic.</returns>
        public static string GenerateAvailabilityTopic(string topicRoot)
        {
            if (string.IsNullOrEmpty(topicRoot))
            {
                return "/availability";
            }

            return topicRoot + "/availability";
        }
    }
}
