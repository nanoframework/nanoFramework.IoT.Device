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
        /// See <see cref="AdvertisingReport"/>
        /// </summary>
        public readonly AdvertisingReport[] Reports;

        internal AdvertisingReportEventArgs(byte reportCount, AdvertisingReport[] reports)
        {
            ReportCount = reportCount;
            Reports = reports;
        }
    }
}
