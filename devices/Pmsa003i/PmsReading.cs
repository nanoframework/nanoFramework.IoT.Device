// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Iot.Device.Pmsx003.Shared;

namespace Iot.Device.Pmsa003i
{
    /// <summary>
    /// Represents one particulate-matter measurement frame.
    /// </summary>
    public sealed class PmsReading
    {
        internal PmsReading(Pmsx003Reading reading)
        {
            Pm1Standard = reading.Pm1Standard;
            Pm2Point5Standard = reading.Pm2Point5Standard;
            Pm10Standard = reading.Pm10Standard;
            Pm1Atmospheric = reading.Pm1Atmospheric;
            Pm2Point5Atmospheric = reading.Pm2Point5Atmospheric;
            Pm10Atmospheric = reading.Pm10Atmospheric;
            ParticlesLargerThan0Point3Micrometers = reading.ParticlesLargerThan0Point3Micrometers;
            ParticlesLargerThan0Point5Micrometers = reading.ParticlesLargerThan0Point5Micrometers;
            ParticlesLargerThan1Micrometer = reading.ParticlesLargerThan1Micrometer;
            ParticlesLargerThan2Point5Micrometers = reading.ParticlesLargerThan2Point5Micrometers;
            ParticlesLargerThan5Micrometers = reading.ParticlesLargerThan5Micrometers;
            ParticlesLargerThan10Micrometers = reading.ParticlesLargerThan10Micrometers;
            Version = reading.Version;
            ErrorCode = reading.ErrorCode;
        }

        /// <summary>Gets the standard PM1.0 concentration in micrograms per cubic meter.</summary>
        public ushort Pm1Standard { get; }

        /// <summary>Gets the standard PM2.5 concentration in micrograms per cubic meter.</summary>
        public ushort Pm2Point5Standard { get; }

        /// <summary>Gets the standard PM10 concentration in micrograms per cubic meter.</summary>
        public ushort Pm10Standard { get; }

        /// <summary>Gets the atmospheric PM1.0 concentration in micrograms per cubic meter.</summary>
        public ushort Pm1Atmospheric { get; }

        /// <summary>Gets the atmospheric PM2.5 concentration in micrograms per cubic meter.</summary>
        public ushort Pm2Point5Atmospheric { get; }

        /// <summary>Gets the atmospheric PM10 concentration in micrograms per cubic meter.</summary>
        public ushort Pm10Atmospheric { get; }

        /// <summary>Gets the number of particles larger than 0.3 micrometers per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan0Point3Micrometers { get; }

        /// <summary>Gets the number of particles larger than 0.5 micrometers per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan0Point5Micrometers { get; }

        /// <summary>Gets the number of particles larger than 1.0 micrometer per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan1Micrometer { get; }

        /// <summary>Gets the number of particles larger than 2.5 micrometers per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan2Point5Micrometers { get; }

        /// <summary>Gets the number of particles larger than 5.0 micrometers per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan5Micrometers { get; }

        /// <summary>Gets the number of particles larger than 10 micrometers per 0.1 liter of air.</summary>
        public ushort ParticlesLargerThan10Micrometers { get; }

        /// <summary>Gets the sensor firmware version.</summary>
        public byte Version { get; }

        /// <summary>Gets the sensor error code.</summary>
        public byte ErrorCode { get; }
    }
}