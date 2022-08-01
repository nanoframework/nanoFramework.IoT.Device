namespace Iot.Device.BlueNrg2.Aci
{
    /// <summary>
    /// Struct for holding an attribute's handle information.
    /// </summary>
    public struct AttributeGroupHandlePair
    {
        /// <summary>
        /// Found attribute handle.
        /// </summary>
        public ushort FoundAttributeHandle;

        /// <summary>
        /// Group end handle.
        /// </summary>
        public ushort GroupEndHandle;
    }
}
