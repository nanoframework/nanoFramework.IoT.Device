namespace Iot.Device.ePaper
{
    /// <summary>
    /// Defines the display current power state.
    /// </summary>
    public enum PowerState
    {
        /// <summary>
        /// The display is in an unknown power state.
        /// </summary>
        Unknown,

        /// <summary>
        /// The display is powered on and ready to accept commands and data.
        /// </summary>
        PoweredOn,

        /// <summary>
        /// The display is powered off (sleeping) and depending on <see cref="SleepMode"/> it may or may not allow some functions.
        /// </summary>
        PoweredOff,
    }

    /// <summary>
    /// Defines the sleep mode the display is operating in.
    /// </summary>
    public enum SleepMode : byte
    {
        /// <summary>
        /// Normal Sleep Mode.
        /// In this mode: 
        /// - DC/DC Off 
        /// - No Clock 
        /// - No Output load 
        /// - MCU Interface Access: ON
        /// - RAM Data Access: ON
        /// </summary>
        Normal = 0x00,

        /// <summary>
        /// Deep Sleep Mode 1.
        /// In this mode: 
        /// - DC/DC Off 
        /// - No Clock 
        /// - No Output load 
        /// - MCU Interface Access: OFF
        /// - RAM Data Access: ON (RAM contents retained)
        /// </summary>
        DeepSleepModeOne = 0x01,

        /// <summary>
        /// Deep Sleep Mode 2.
        /// In this mode: 
        /// - DC/DC Off 
        /// - No Clock 
        /// - No Output load 
        /// - MCU Interface Access: OFF
        /// - RAM Data Access: OFF (RAM contents NOT retained)
        /// </summary>
        DeepSleepModeTwo = 0x11,
    }

}
