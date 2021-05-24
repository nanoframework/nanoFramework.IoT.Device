using System;

namespace Iot.Device.Max7219
{
    public class DeviceIdDigit
    {
        public DeviceIdDigit(int deviceId, int digit)
        {
            DeviceId = deviceId;
            Digit = digit;
        }

        public int DeviceId { get; set; }
        public int Digit { get; set; }
    }
}
