using System;

namespace Iot.Device.BlueNrg2.Aci.Events
{
    /// <summary>
    /// Class containing AttFindByTypeValueResponseEventArgs
    /// </summary>
    public class AttFindByTypeValueResponseEventArgs : EventArgs
    {
        /// <summary>
        /// Connection handle related to the response.
        /// </summary>
        public readonly ushort ConnectionHandle;

        /// <summary>
        /// Number of attribute, group handle pairs
        /// </summary>
        public readonly byte NumberOfHandlePairs;

        /// <summary>
        /// See <see cref="AttributeGroupHandlePair"/>.
        /// </summary>
        public readonly AttributeGroupHandlePair[] AttributeGroupHandlePairs;

        internal AttFindByTypeValueResponseEventArgs(
            ushort connectionHandle,
            byte numberOfHandlePairs,
            AttributeGroupHandlePair[] attributeGroupHandlePairs)
        {
            ConnectionHandle = connectionHandle;
            NumberOfHandlePairs = numberOfHandlePairs;
            AttributeGroupHandlePairs = attributeGroupHandlePairs;
        }
    }
}
