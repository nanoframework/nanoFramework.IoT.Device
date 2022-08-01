using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing GattErrorResponseEventArgs.
    /// </summary>
    public class GattErrorResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// The request that generated this error response.
        /// </summary>
        public readonly byte RequestOpcode;

        /// <summary>
        /// The attribute handle that generated this error response.
        /// </summary>
        public readonly ushort AttributeHandle;

        /// <summary>
        /// The reason why the request has generated an error response (ATT error codes)
        /// </summary>
        public readonly AttErrorCode ErrorCode;

        internal GattErrorResponseEventArgs(
            ushort connectionHandle,
            byte requestOpcode,
            ushort attributeHandle,
            AttErrorCode errorCode)
        {
            ConnectionHandle = connectionHandle;
            RequestOpcode = requestOpcode;
            AttributeHandle = attributeHandle;
            ErrorCode = errorCode;
        }
    }
}
