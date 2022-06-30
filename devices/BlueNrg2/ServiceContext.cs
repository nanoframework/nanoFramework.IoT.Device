namespace Iot.Device.BlueNrg2
{
    internal struct ServiceContext
    {
        public string ServiceUuid;
        public Characteristic[] Characteristics;
        public ushort[] AttributeHandles;
        public Descriptor[] Descriptors;
    }
}
