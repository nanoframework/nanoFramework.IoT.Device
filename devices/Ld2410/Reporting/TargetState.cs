namespace Ld2410.Reporting
{
    /// <summary>
    /// The state of the radar target.
    /// </summary>
    public enum TargetState : byte
    {
        /// <summary>
        /// No target detected.
        /// </summary>
        NoTarget = 0x00,

        /// <summary>
        /// A moving target.
        /// </summary>
        MovingTarget = 0x01,

        /// <summary>
        /// A stationary target.
        /// </summary>
        StationaryTarget = 0x02,

        /// <summary>
        /// A stationary and moving target detected.
        /// </summary>
        MovementAndStationaryTarget = 0x03
    }
}
