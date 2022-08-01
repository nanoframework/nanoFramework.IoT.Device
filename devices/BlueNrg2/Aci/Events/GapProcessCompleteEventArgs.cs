using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GapProcessCompleteEventArgs.
    /// </summary>
    public class GapProcessCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Terminated procedure.
        /// </summary>
        public readonly ProcedureCode ProcedureCodes;

        /// <summary>
        /// For standard error codes see Bluetooth specification, Vol. 2,
        /// part D. For proprietary error code refer to Error codes section.
        /// </summary>
        public readonly byte Status;

        /// <summary>
        /// Length of <see cref="Data"/> in octets.
        /// </summary>
        public readonly byte DataLength;

        /// <summary>
        /// Procedure Specific Data: - For Name Discovery Procedure: the name
        /// of the peer device if the procedure completed successfully.
        /// </summary>
        public readonly byte[] Data;

        internal GapProcessCompleteEventArgs(ProcedureCode procedureCodes, byte status, byte dataLength, byte[] data)
        {
            ProcedureCodes = procedureCodes;
            Status = status;
            DataLength = dataLength;
            Data = data;
        }
    }
}
