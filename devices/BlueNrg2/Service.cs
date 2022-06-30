namespace Iot.Device.BlueNrg2
{
    internal struct Service
    {
        public byte Type;
        public string Uuid;
        public Service[] Includes;
        public Characteristic[] Characteristics;
    }
}