// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Iot.Device.BlueNrg2.Aci.Events;

namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Flags for creating procedure codes.
    /// </summary>
    [Flags]
    public enum ProcedureCode : byte
    {
        /// <summary>
        /// Limited discovery process.
        /// <seealso cref="Gap.StartLimitedDiscoveryProcedure"/>
        /// <seealso cref="EventProcessor.GapLimitedDiscoverableEvent"/>
        /// </summary>
        LimitedDiscovery = 0x01,

        /// <summary>
        /// General discovery process.
        /// <seealso cref="Gap.StartGeneralDiscoveryProcedure"/>
        /// </summary>
        GeneralDiscovery = 0x02,

        /// <summary>
        /// Name discovery process.
        /// <seealso cref="Gap.StartNameDiscoveryProcedure"/>
        /// </summary>
        NameDiscovery = 0x04,

        /// <summary>
        /// Auto connection establishment process.
        /// <seealso cref="Gap.StartAutomaticConnectionEstablishProcedure"/>
        /// </summary>
        AutoConnectionEstablishment = 0x08,

        /// <summary>
        /// General connection establishment process.
        /// <seealso cref="Gap.StartGeneralConnectionEstablishProcedure"/>
        /// </summary>
        GeneralConnectionEstablishment = 0x10,

        /// <summary>
        /// Selective connection establishment process.
        /// <seealso cref="Gap.StartSelectiveConnectionEstablishProcedure"/>
        /// </summary>
        SelectiveConnectionEstablishment = 0x20,

        /// <summary>
        /// Direct connection establishment process.
        /// <seealso cref="Gap.SetDirectConnectable"/>
        /// </summary>
        DirectConnectionEstablishment = 0x40,

        /// <summary>
        /// Observation process.
        /// <seealso cref="Gap.StartObservationProcedure"/>
        /// </summary>
        Observation = 0x80
    }
}
