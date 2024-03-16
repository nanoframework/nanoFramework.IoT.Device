// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AdvertisingReportEventArgs
    /// </summary>
    public class AdvertisingReportEventArgs : EventArgs
    {
        /// <summary>
        /// Number of responses int this event.
        /// </summary>
        public readonly byte ReportCount;

        /// <summary>
        /// See <see cref="AdvertisingReportContainer"/>
        /// </summary>
        public readonly AdvertisingReportContainer[] Reports;

        internal AdvertisingReportEventArgs(byte reportCount, AdvertisingReportContainer[] reports)
        {
            ReportCount = reportCount;
            Reports = reports;
        }
    }
}
