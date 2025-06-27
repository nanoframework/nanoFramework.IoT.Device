namespace Iot.Device.XPT2046
{
    /// <summary>
    /// A touch point
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Gets or sets X
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets Y
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets of sets the amount of pressure of the touch.
        /// </summary>
        public int Weight { get; set; }
    }
}