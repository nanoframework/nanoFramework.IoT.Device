namespace Ld2410.Reporting
{
    public sealed class EngineeringModeReportFrame : BasicReportFrame
    {
        public byte MaxMovingDistanceGate { get; internal set; }

        public byte MaxStaticDistanceGate { get; internal set; }

        public byte Gate0MovingDistanceEnergy { get; internal set; }
        public byte Gate1MovingDistanceEnergy { get; internal set; }
        public byte Gate2MovingDistanceEnergy { get; internal set; }
        public byte Gate3MovingDistanceEnergy { get; internal set; }
        public byte Gate4MovingDistanceEnergy { get; internal set; }
        public byte Gate5MovingDistanceEnergy { get; internal set; }
        public byte Gate6MovingDistanceEnergy { get; internal set; }
        public byte Gate7MovingDistanceEnergy { get; internal set; }
        public byte Gate8MovingDistanceEnergy { get; internal set; }

        public byte Gate0StaticDistanceEnergy { get; internal set; }
        public byte Gate1StaticDistanceEnergy { get; internal set; }
        public byte Gate2StaticDistanceEnergy { get; internal set; }
        public byte Gate3StaticDistanceEnergy { get; internal set; }
        public byte Gate4StaticDistanceEnergy { get; internal set; }
        public byte Gate5StaticDistanceEnergy { get; internal set; }
        public byte Gate6StaticDistanceEnergy { get; internal set; }
        public byte Gate7StaticDistanceEnergy { get; internal set; }
        public byte Gate8StaticDistanceEnergy { get; internal set; }

        public byte[] AdditionalData { get; private set; }

        public EngineeringModeReportFrame()
        {
            this.DataType = ReportingType.EngineeringMode;
        }
    }
}
