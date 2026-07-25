// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace nanoFramework.HomeAssistant
{
    /// <summary>
    /// Home Assistant device class constants for sensors, binary sensors and other entities.
    /// Maps to official HA device classes for automatic UI presentation.
    /// </summary>
    public static class HomeAssistantDeviceClass
    {
        /// <summary>Device class identifier for apparent power sensors.</summary>
        public const string ApparentPower = "apparent_power";

        /// <summary>Device class identifier for AQI sensors.</summary>
        public const string Aqi = "aqi";

        /// <summary>Device class identifier for atmospheric pressure sensors.</summary>
        public const string AtmosphericPressure = "atmospheric_pressure";

        /// <summary>Device class identifier for battery sensors (sensor: charge level) and low-battery binary sensors.</summary>
        public const string Battery = "battery";

        /// <summary>Device class identifier for binary sensors indicating whether a device is currently charging or not charging.</summary>
        public const string BatteryCharging = "battery_charging";

        /// <summary>Device class identifier for blood glucose concentration sensors.</summary>
        public const string BloodGlucoseConcentration = "blood_glucose_concentration";

        /// <summary>Device class identifier for carbon dioxide sensors.</summary>
        public const string CarbonDioxide = "carbon_dioxide";

        /// <summary>Device class identifier for carbon monoxide sensors and binary sensors.</summary>
        public const string CarbonMonoxide = "carbon_monoxide";

        /// <summary>Device class identifier for cold binary sensors.</summary>
        public const string Cold = "cold";

        /// <summary>Device class identifier for connectivity binary sensors.</summary>
        public const string Connectivity = "connectivity";

        /// <summary>Device class identifier for current sensors.</summary>
        public const string Current = "current";

        /// <summary>Device class identifier for data rate sensors.</summary>
        public const string DataRate = "data_rate";

        /// <summary>Device class identifier for data size sensors.</summary>
        public const string DataSize = "data_size";

        /// <summary>Device class identifier for date sensors.</summary>
        public const string Date = "date";

        /// <summary>Device class identifier for distance sensors.</summary>
        public const string Distance = "distance";

        /// <summary>Device class identifier for door binary sensors.</summary>
        public const string Door = "door";

        /// <summary>Device class identifier for duration sensors.</summary>
        public const string Duration = "duration";

        /// <summary>Device class identifier for energy sensors.</summary>
        public const string Energy = "energy";

        /// <summary>Device class identifier for energy storage sensors.</summary>
        public const string EnergyStorage = "energy_storage";

        /// <summary>Device class identifier for frequency sensors.</summary>
        public const string Frequency = "frequency";

        /// <summary>Device class identifier for garage door binary sensors.</summary>
        public const string GarageDoor = "garage_door";

        /// <summary>Device class identifier for gas sensors and binary sensors.</summary>
        public const string Gas = "gas";

        /// <summary>Device class identifier for heat binary sensors.</summary>
        public const string Heat = "heat";

        /// <summary>Device class identifier for humidity sensors.</summary>
        public const string Humidity = "humidity";

        /// <summary>Device class identifier for illuminance sensors.</summary>
        public const string Illuminance = "illuminance";

        /// <summary>Device class identifier for irradiance sensors.</summary>
        public const string Irradiance = "irradiance";

        /// <summary>Device class identifier for light binary sensors.</summary>
        public const string Light = "light";

        /// <summary>Device class identifier for lock binary sensors.</summary>
        public const string Lock = "lock";

        /// <summary>Device class identifier for moisture sensors (sensor: reading) and wet/dry binary sensors.</summary>
        public const string Moisture = "moisture";

        /// <summary>Device class identifier for monetary value sensors.</summary>
        public const string Monetary = "monetary";

        /// <summary>Device class identifier for motion binary sensors.</summary>
        public const string Motion = "motion";

        /// <summary>Device class identifier for moving binary sensors.</summary>
        public const string Moving = "moving";

        /// <summary>Device class identifier for nitrogen dioxide sensors.</summary>
        public const string NitrogenDioxide = "nitrogen_dioxide";

        /// <summary>Device class identifier for nitrogen monoxide sensors.</summary>
        public const string NitrogenMonoxide = "nitrogen_monoxide";

        /// <summary>Device class identifier for nitrous oxide sensors.</summary>
        public const string NitrousOxide = "nitrous_oxide";

        [Obsolete("Use NitrogenDioxide, NitrogenMonoxide, or NitrousOxide instead.")]
        public const string NitrogenOxides = "nitrogen_oxides";

        /// <summary>Device class identifier for occupancy binary sensors.</summary>
        public const string Occupancy = "occupancy";

        /// <summary>Device class identifier for opening binary sensors.</summary>
        public const string Opening = "opening";

        /// <summary>Device class identifier for ozone sensors.</summary>
        public const string Ozone = "ozone";

        /// <summary>Device class identifier for PM1 particulate sensors.</summary>
        public const string PM1 = "pm1";

        /// <summary>Device class identifier for PM2.5 particulate sensors.</summary>
        public const string PM25 = "pm25";

        /// <summary>Device class identifier for PM10 particulate sensors.</summary>
        public const string PM10 = "pm10";

        /// <summary>Device class identifier for plug binary sensors.</summary>
        public const string Plug = "plug";

        /// <summary>Device class identifier for power sensors and power-detected binary sensors.</summary>
        public const string Power = "power";

        /// <summary>Device class identifier for power factor sensors.</summary>
        public const string PowerFactor = "power_factor";

        /// <summary>Device class identifier for presence binary sensors.</summary>
        public const string Presence = "presence";

        /// <summary>Device class identifier for pressure sensors.</summary>
        public const string Pressure = "pressure";

        /// <summary>Device class identifier for problem binary sensors.</summary>
        public const string Problem = "problem";

        /// <summary>Device class identifier for reactive power sensors.</summary>
        public const string ReactivePower = "reactive_power";

        /// <summary>Device class identifier for running binary sensors.</summary>
        public const string Running = "running";

        /// <summary>Device class identifier for safety binary sensors.</summary>
        public const string Safety = "safety";

        /// <summary>Device class identifier for signal strength sensors.</summary>
        public const string SignalStrength = "signal_strength";

        /// <summary>Device class identifier for smoke binary sensors.</summary>
        public const string Smoke = "smoke";

        /// <summary>Device class identifier for sound binary sensors.</summary>
        public const string Sound = "sound";

        /// <summary>Device class identifier for sound pressure sensors.</summary>
        public const string SoundPressure = "sound_pressure";

        /// <summary>Device class identifier for speed sensors.</summary>
        public const string Speed = "speed";

        /// <summary>Device class identifier for sulphur dioxide sensors.</summary>
        public const string SulphurDioxide = "sulphur_dioxide";
        
        [Obsolete("Use SulphurDioxide instead.")]
        public const string SulfurDioxide = SulphurDioxide;

        /// <summary>Device class identifier for tamper binary sensors.</summary>
        public const string Tamper = "tamper";

        /// <summary>Device class identifier for temperature sensors.</summary>
        public const string Temperature = "temperature";

        /// <summary>Device class identifier for timestamp sensors.</summary>
        public const string Timestamp = "timestamp";

        /// <summary>Device class identifier for update binary sensors.</summary>
        public const string Update = "update";

        /// <summary>Device class identifier for vibration binary sensors.</summary>
        public const string Vibration = "vibration";

        /// <summary>Device class identifier for volatile organic compounds sensors.</summary>
        public const string VolatileOrganicCompounds = "volatile_organic_compounds";

        /// <summary>Device class identifier for voltage sensors.</summary>
        public const string Voltage = "voltage";

        /// <summary>Device class identifier for volume sensors.</summary>
        public const string Volume = "volume";

        /// <summary>Device class identifier for volume flow rate sensors.</summary>
        public const string VolumeFlowRate = "volume_flow_rate";

        /// <summary>Device class identifier for volume storage sensors.</summary>
        public const string VolumeStorage = "volume_storage";

        /// <summary>Device class identifier for water sensors.</summary>
        public const string Water = "water";

        /// <summary>Device class identifier for weight sensors.</summary>
        public const string Weight = "weight";

        /// <summary>Device class identifier for wind direction sensors.</summary>
        public const string WindDirection = "wind_direction";

        /// <summary>Device class identifier for wind speed sensors.</summary>
        public const string WindSpeed = "wind_speed";

        /// <summary>Device class identifier for window binary sensors.</summary>
        public const string Window = "window";
    }
}
