namespace Ld2410.Reporting
{
    /// <summary>
    /// The format of the radar report data.
    /// </summary>
    public enum ReportingType : byte
    {
        /// <summary>
        /// Engineering mode data.
        /// </summary>
        EngineeringMode = 0x01,

        /// <summary>
        /// Basic information mode data.
        /// </summary>
        BasicMode = 0x02
    }
}
