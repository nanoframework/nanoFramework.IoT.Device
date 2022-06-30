namespace Iot.Device.BlueNrg2
{
    internal struct Descriptor
    {
        public string Uuid;
        public byte AttributeFlags;
        public byte MinimumKeySize;
        public GattAccess AccessCallback;
        public byte[] Arg;
    }
}