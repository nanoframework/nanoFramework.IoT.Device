using System;

namespace Iot.Device.Ft6xx6x
{
    /// <summary>
    /// The comsumption mode
    /// </summary>
    public enum ConsumptionMode
    {
        /// <summary>Active</summary>
        Active = 0x00,

        /// <summary>Monitor</summary>
        Monitor = 0x01,

        /// <summary>Standby</summary>
        Standby = 0x02,

        /// <summary>Hibernate</summary>
        Hibernate = 0x03,
    }
}
