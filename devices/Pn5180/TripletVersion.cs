using System;

namespace Iot.Device.Pn5180
{
    public class TripletVersion
    {
        public TripletVersion(Version product, Version firmware, Version eeprom)
        {
            Product = product;
            Firmware = firmware;
            Eeprom = eeprom;
        }

        public Version Product { get; set; }
        public Version Firmware { get; set; }
        public Version Eeprom { get; set; }
    }
}
