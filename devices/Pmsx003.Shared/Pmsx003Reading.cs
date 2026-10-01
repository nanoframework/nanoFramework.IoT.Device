// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Iot.Device.Pmsx003.Shared
{
    internal sealed class Pmsx003Reading
    {
        internal Pmsx003Reading(ushort[] values, byte version, byte errorCode)
        {
            Pm1Standard = values[0];
            Pm2Point5Standard = values[1];
            Pm10Standard = values[2];
            Pm1Atmospheric = values[3];
            Pm2Point5Atmospheric = values[4];
            Pm10Atmospheric = values[5];
            ParticlesLargerThan0Point3Micrometers = values[6];
            ParticlesLargerThan0Point5Micrometers = values[7];
            ParticlesLargerThan1Micrometer = values[8];
            ParticlesLargerThan2Point5Micrometers = values[9];
            ParticlesLargerThan5Micrometers = values[10];
            ParticlesLargerThan10Micrometers = values[11];
            Version = version;
            ErrorCode = errorCode;
        }

        internal ushort Pm1Standard { get; }

        internal ushort Pm2Point5Standard { get; }

        internal ushort Pm10Standard { get; }

        internal ushort Pm1Atmospheric { get; }

        internal ushort Pm2Point5Atmospheric { get; }

        internal ushort Pm10Atmospheric { get; }

        internal ushort ParticlesLargerThan0Point3Micrometers { get; }

        internal ushort ParticlesLargerThan0Point5Micrometers { get; }

        internal ushort ParticlesLargerThan1Micrometer { get; }

        internal ushort ParticlesLargerThan2Point5Micrometers { get; }

        internal ushort ParticlesLargerThan5Micrometers { get; }

        internal ushort ParticlesLargerThan10Micrometers { get; }

        internal byte Version { get; }

        internal byte ErrorCode { get; }
    }
}