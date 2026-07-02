// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Common presets and configurations for Home Assistant sensor entities.
    /// </summary>
    public static class HomeAssistantSensorPresets
    {
        /// <summary>
        /// Common temperature sensor preset (°C).
        /// </summary>
        public static void ApplyTemperaturePreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Temperature;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "°C";
        }

        /// <summary>
        /// Common humidity sensor preset (%).
        /// </summary>
        public static void ApplyHumidityPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Humidity;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "%";
        }

        /// <summary>
        /// Common pressure sensor preset (hPa).
        /// </summary>
        public static void ApplyPressurePreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.AtmosphericPressure;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "hPa";
        }

        /// <summary>
        /// Common power sensor preset (W).
        /// </summary>
        public static void ApplyPowerPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Power;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "W";
        }

        /// <summary>
        /// Common energy sensor preset (Wh, monotonic total).
        /// </summary>
        public static void ApplyEnergyPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Energy;
            entity.StateClass = HomeAssistantStateClass.TotalIncreasing;
            entity.UnitOfMeasurement = "Wh";
        }

        /// <summary>
        /// Common water usage sensor preset (L, monotonic total).
        /// </summary>
        public static void ApplyWaterPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Water;
            entity.StateClass = HomeAssistantStateClass.TotalIncreasing;
            entity.UnitOfMeasurement = "L";
        }

        /// <summary>
        /// Common voltage sensor preset (V).
        /// </summary>
        public static void ApplyVoltagePreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Voltage;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "V";
        }

        /// <summary>
        /// Common current sensor preset (A).
        /// </summary>
        public static void ApplyCurrentPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Current;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "A";
        }

        /// <summary>
        /// Common illuminance sensor preset (lux).
        /// </summary>
        public static void ApplyIlluminancePreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Illuminance;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "lx";
        }

        /// <summary>
        /// Common battery sensor preset (%, diagnostic).
        /// </summary>
        public static void ApplyBatteryPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Battery;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "%";
            entity.EntityCategory = HomeAssistantEntityCategory.Diagnostic;
        }

        /// <summary>
        /// Common duration sensor preset (seconds).
        /// </summary>
        public static void ApplyDurationPreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.Duration;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "s";
        }

        /// <summary>
        /// Common CO2 sensor preset (ppm).
        /// </summary>
        public static void ApplyCarbonDioxidePreset(HomeAssistantDiscoveryEntity entity)
        {
            entity.DeviceClass = HomeAssistantDeviceClass.CarbonDioxide;
            entity.StateClass = HomeAssistantStateClass.Measurement;
            entity.UnitOfMeasurement = "ppm";
        }
    }
}
