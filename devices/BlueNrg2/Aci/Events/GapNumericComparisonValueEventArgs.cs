using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapNumericComparisonValueEventArgs.
    /// </summary>
    public class GapNumericComparisonValueEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the underlying Pairing.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The numeric value to check.
        /// </summary>
        public readonly uint NumericValue;

        internal GapNumericComparisonValueEventArgs(ushort connectionHandle, uint numericValue)
        {
            ConnectionHandle = connectionHandle;
            NumericValue = numericValue;
        }
    }
}
