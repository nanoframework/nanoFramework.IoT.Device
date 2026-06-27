// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Supported Home Assistant MQTT discovery component types.
    /// </summary>
    public enum HomeAssistantComponentType
    {
        /// <summary>
        /// Switch entity.
        /// </summary>
        Switch,

        /// <summary>
        /// Number entity.
        /// </summary>
        Number,

        /// <summary>
        /// Sensor entity.
        /// </summary>
        Sensor,

        /// <summary>
        /// Binary sensor entity.
        /// </summary>
        BinarySensor,

        /// <summary>
        /// Button entity.
        /// </summary>
        Button,

        /// <summary>
        /// Select entity.
        /// </summary>
        Select,

        /// <summary>
        /// Light entity.
        /// </summary>
        Light,

        /// <summary>
        /// Cover entity.
        /// </summary>
        Cover,

        /// <summary>
        /// Climate entity.
        /// </summary>
        Climate,

        /// <summary>
        /// Text entity.
        /// </summary>
        Text
    }
}
