// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace IoT.Device.Sim7080
{
    /// <summary>
    /// The mode the system will use to connect to the provider network.
    /// </summary>
    public enum SystemMode
    {
        /// <summary>
        /// No Service detected.
        /// </summary>
        NoService = 0,

        /// <summary>
        /// Global System for Mobile Communications (GSM).
        /// </summary>
        GSM = 1,

        /// <summary>
        /// Enhanced General Packet Radio Service (EGPRS).
        /// </summary>
        EGPRS = 3,

        /// <summary>
        /// Long Term Evolution (LTE) enhanced Machine Type Communication.
        /// </summary>
        LTE_M1 = 7,

        /// <summary>
        /// Long Term Evolution (LTE) NarrowBand.
        /// </summary>
        LTE_NB = 9
    }
}
