using System;

namespace Iot.Device.Axp192
{
    /// <summary>
    /// GPIO4 behavior
    /// </summary>
    public enum Gpio4Behavior
    {
        /// <summary>External charging</summary>
        ExternalCharing = 0b0000_0000,

        /// <summary>NMOS Leak Open Output</summary>
        MnosLeakOpenOutput = 0b0000_0100,

        /// <summary>Universal Input Function</summary>
        UniversalInputFunction = 0b0000_1000,
    }
}
