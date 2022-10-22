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
}
