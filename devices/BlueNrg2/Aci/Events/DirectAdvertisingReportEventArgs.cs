// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing DirectAdvertisingReportEventArgs.
    /// </summary>
    public class DirectAdvertisingReportEventArgs : EventArgs
    {
        /// <summary>
        /// Number of responses in this event.
        /// </summary>
        public readonly byte ReportCount;

        /// <summary>
        /// <see cref="DirectAdvertisingReportContainer"/>
        /// </summary>
        public readonly DirectAdvertisingReportContainer[] DirectAdvertisingReports;

        internal DirectAdvertisingReportEventArgs(byte reportCount, DirectAdvertisingReportContainer[] directAdvertisingReports)
        {
            ReportCount = reportCount;
            DirectAdvertisingReports = directAdvertisingReports;
        }
    }
}
